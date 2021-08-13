using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.Json.Serialization;

using Battlegrounds.Modding;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;

using static Battlegrounds.Game.Match.SessionParticipantTeam;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Represents a game session where a match will take place between players with a pre-selected <see cref="Company"/> and using a set of preset settings.
    /// Implements <see cref="ISession"/>.
    /// </summary>
    public class Session : ISession {

        SessionParticipant[] m_participants;

        /// <summary>
        /// Get an array of participating players in the <see cref="Session"/>. This should only contain <see cref="SessionParticipant"/> instances for players and AI (not observers).
        /// </summary>
        [JsonIgnore]
        public SessionParticipant[] Participants => m_participants;

        /// <summary>
        /// Get the name of the scenario file to play.
        /// </summary>
        public Scenario Scenario { get; }

        /// <summary>
        /// Get the <see cref="Wincondition"/> to use when playing.
        /// </summary>
        public IGamemode Gamemode { get; }

        /// <summary>
        /// Get the <see cref="WinconditionOption"/>? to use when playing.
        /// </summary>
        public string GamemodeOption { get; private set; }

        /// <summary>
        /// Get the associated <see cref="ITuningMod"/> with the <see cref="Session"/>.
        /// </summary>
        [JsonIgnore] public ITuningMod TuningMod { get; }

        /// <summary>
        /// Get a list of all settings to apply for the <see cref="Session"/>.
        /// </summary>
        public Dictionary<string, object> Settings { get; }

        public Guid SessionID { get; }

        [JsonIgnore] public bool AllowPersistency => this.m_participants.All(x => x.Difficulty.AllowsPersistency());

        private Session(Scenario scenario, IGamemode gamemode, ITuningMod tuning) {
            this.Settings = new Dictionary<string, object>();
            this.SessionID = Guid.NewGuid();
            this.Scenario = scenario;
            this.Gamemode = gamemode;
            this.TuningMod = tuning;
            this.m_participants = Array.Empty<SessionParticipant>();
        }

        /// <summary>
        /// Add a custom setting to the <see cref="Session"/>.
        /// </summary>
        /// <param name="setting">The name of the setting to add.</param>
        /// <param name="value">The value of the setting (Automatically converted to a Lua code equivalent).</param>
        public void AddSetting(string setting, object value) {
            if (this.Settings.ContainsKey(setting)) {
                this.Settings[setting] = value;
            } else {
                this.Settings.Add(setting, value);
            }
        }

        public Company FindCompany(string playername, Faction faction)
            => this.m_participants.FirstOrDefault(x => x.IsHumanParticipant && x.UserDisplayname.CompareTo(playername) == 0 && x.ParticipantFaction == faction).ParticipantCompany;

        /// <summary>
        /// Get the player company associated with the given steam index.
        /// </summary>
        /// <param name="steamIndex">The index of the steam user.</param>
        /// <returns>The <see cref="Company"/> associated with the given steam user.</returns>
        public Company GetPlayerCompany(ulong steamIndex)
            => this.m_participants.FirstOrDefault(x => x.UserID == steamIndex).ParticipantCompany;

        /// <summary>
        /// Create a new <see cref="Session"/> instance with a unique <see cref="Guid"/>.
        /// </summary>
        /// <remarks>
        /// Player companies must have been assigned beforehand.
        /// </remarks>
        /// <param name="sessionInfo">The map name of the scenario to play on.</param>
        /// <returns>New <see cref="Session"/> with the given data.</returns>
        public static Session CreateSession(SessionInfo sessionInfo) {

            // Create the session
            Session session = new Session(sessionInfo.SelectedScenario, sessionInfo.SelectedGamemode, sessionInfo.SelectedTuningMod);

            // defaults
            var defDif = sessionInfo.DefaultDifficulty;

            // Get player count
            int playerCount = GetPlayerCount(sessionInfo.FillAI, sessionInfo.Allies?.Length ?? 0, sessionInfo.Axis?.Length ?? 0, out int alliedFillAI, out int axisFillAI);

            // Add players
            session.m_participants = new SessionParticipant[playerCount];
            byte currentIndex = 0;
            byte playerTeamIndex = 0;

            // Assign allied players
            session.AssignPlayers(TEAM_ALLIES, sessionInfo.Allies, ref currentIndex, ref playerTeamIndex);
            session.AddAIPlayers(TEAM_ALLIES, defDif, alliedFillAI, ref currentIndex, ref playerTeamIndex, x => sessionInfo.Axis[sessionInfo.Allies?.Length ?? 1 - 1 + x].ParticipantFaction);

            // Assign axis players
            session.AssignPlayers(TEAM_AXIS, sessionInfo.Axis, ref currentIndex, ref playerTeamIndex);
            session.AddAIPlayers(TEAM_AXIS, defDif, axisFillAI, ref currentIndex, ref playerTeamIndex, x => sessionInfo.Allies[sessionInfo.Axis?.Length ?? 1 - 1 + x].ParticipantFaction);

            // Set the game mode
            if (sessionInfo.SelectedGamemode != null) {
                if (sessionInfo.IsOptionValue) {
                    session.AddSetting("gamemode_setting", sessionInfo.SelectedGamemodeOption);
                } else {
                    session.AddSetting("gamemode_setting", sessionInfo.SelectedGamemode.Options[sessionInfo.SelectedGamemodeOption].Value);
                }
            } else {
                Trace.WriteLine("Failed to read selected gamemode - using 500 as default option", "Session.Create");
                session.AddSetting("gamemode_setting", 500);
            }

            // Set session gamemode
            session.GamemodeOption = session.Settings["gamemode_setting"].ToString();

            // Set day/night flag
            if (sessionInfo.EnableDayNightCycle) {
                session.AddSetting("day_night_cycle", true);
            }

            // Set day/night flag
            if (sessionInfo.EnableSupply) {
                session.AddSetting("sypply_system", true);
            }

            // Return the new session
            return session;

        }

        private void AssignPlayers(SessionParticipantTeam team, SessionParticipant[] participants, ref byte currentIndex, ref byte playerTeamIndex) {

            if (participants != null) {
                foreach (SessionParticipant participant in participants) {
                    if (participant.TeamIndex == team) {
                        this.m_participants[currentIndex++] = participant;
                    }
                }
                playerTeamIndex = (byte)participants.Length;
            }

        }

        private void AddAIPlayers(SessionParticipantTeam team, AIDifficulty aIDifficulty, int fillAICount, ref byte currentIndex, ref byte playerTeamIndex, Func<int, Faction> complementaryFunc) {
            for (int i = 0; i < fillAICount; i++) {
                byte pIndex = currentIndex++;
                Faction complementary = Faction.GetComplementaryFaction(complementaryFunc(i));
                Company aiCompany = CompanyGenerator.Generate(complementary, this.TuningMod.Guid.ToString(), false, true, false);
                aiCompany.Owner = "AIPlayer";
                this.m_participants[pIndex] = new SessionParticipant(aIDifficulty, aiCompany, team, playerTeamIndex++);
            }
        }

        private static int GetPlayerCount(bool fillAI, int alliesCount, int axisCount, out int alliesAI, out int axisAI) {

            // The allies and axis AI to fill
            alliesAI = 0;
            axisAI = 0;

            // If matching count, simply sum them
            if (alliesCount == axisCount) {

                return alliesCount + axisCount;

            } else if (fillAI) { // not matching - and we should fill

                if (alliesCount < axisCount) { // less allied players?
                    alliesAI = axisCount - alliesCount; // calculate missing AI
                    return axisCount + alliesCount + alliesAI; // axis count + allies count + whatever allies AI we're adding
                } else { // less axis players
                    axisAI = alliesCount - axisCount; // calculate missing AI
                    return alliesCount + axisCount + axisAI; // allies count + axis count + whatever axis AI count we're adding
                }

            } else { // not matching - but we should not fill

                return alliesCount + axisCount;

            }

        }

        /// <summary>
        /// Connect all companies to their respective owners using the <see cref="SessionInfo"/> to connect players with their companies.
        /// </summary>
        /// <remarks>
        /// The companies are connected through the user IDs and names are then assigned afterwards.
        /// </remarks>
        /// <param name="allCompanies">Array of all the <see cref="Company"/> instances to connect with their respective player.</param>
        /// <param name="sessionInfo">Session info containing all the relevant information of the users.</param>
        public static void ZipCompanies(Company[] allCompanies, ref SessionInfo sessionInfo) {

            for (int i = 0; i < allCompanies.Length; i++) {

                if (allCompanies[i].Army.IsAllied) {

                    int j = sessionInfo.Allies.IndexOf(x => x.GetID().ToString().CompareTo(allCompanies[i].Owner) == 0 && x.Difficulty == AIDifficulty.Human);
                    if (j >= 0) {
                        allCompanies[i].Owner = sessionInfo.Allies[j].UserDisplayname;
                        sessionInfo.Allies[j] = new SessionParticipant(sessionInfo.Allies[j].UserDisplayname, sessionInfo.Allies[j].UserID, allCompanies[i], TEAM_ALLIES, 0);
                    } else {
                        Trace.WriteLine($"Failed to pair allied company '{allCompanies[i].Name}' with a player...", "Session.Zip");
                    }

                } else {

                    int j = sessionInfo.Axis.IndexOf(x => x.GetID().ToString().CompareTo(allCompanies[i].Owner) == 0 && x.Difficulty == AIDifficulty.Human);
                    if (j >= 0) {
                        allCompanies[i].Owner = sessionInfo.Axis[j].UserDisplayname;
                        sessionInfo.Axis[j] = new SessionParticipant(sessionInfo.Axis[j].UserDisplayname, sessionInfo.Axis[j].UserID, allCompanies[i], TEAM_AXIS, 0);
                    } else {
                        Trace.WriteLine($"Failed to pair axis company '{allCompanies[i].Name}' with a player...", "Session.Zip");
                    }

                }

            }

        }

    }

}
