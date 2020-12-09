using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Json;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Game.Database;

using static Battlegrounds.Game.Battlegrounds.SessionParticipantTeam;
using Battlegrounds.Functional;
using System.Diagnostics;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Represents a game session where a match will take place between players with a pre-selected <see cref="Company"/> and using a set of preset settings. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public class Session : IJsonObject {

        SessionParticipant[] m_participants;

        /// <summary>
        /// Array of participating players in the <see cref="Session"/>. This should only contain <see cref="SessionParticipant"/> instances for players and AI (not observers).
        /// </summary>
        [JsonIgnore]
        public SessionParticipant[] Participants => m_participants;

        /// <summary>
        /// The name of the scenario file to play.
        /// </summary>
        public Scenario Scenario { get; }

        /// <summary>
        /// The <see cref="Wincondition"/> to use when playing.
        /// </summary>
        public IWinconditionMod Gamemode { get; }

        /// <summary>
        /// A list of all settings to apply for the <see cref="Session"/>.
        /// </summary>
        public Dictionary<string, object> Settings { get; }

        /// <summary>
        /// The unique GUID to use to verify match data.
        /// </summary>
        public Guid SessionID { get; }

        /// <summary>
        /// Does the <see cref="Session"/> allow for persistency (events ingame will be saved in the company).
        /// </summary>
        [JsonIgnore] public bool AllowPersistency => this.m_participants.All(x => x.Difficulty.AllowsPersistency());

        /// <summary>
        /// The associated <see cref="ITuningMod"/> with the <see cref="Session"/>.
        /// </summary>
        [JsonIgnore] public ITuningMod TuningMod { get; private set; }

        private Session(Scenario scenario, IWinconditionMod gamemode) {
            this.Settings = new Dictionary<string, object>();
            this.SessionID = Guid.NewGuid();
            this.Scenario = scenario;
            this.Gamemode = gamemode;
            this.m_participants = new SessionParticipant[0];
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
            => m_participants.FirstOrDefault(x => x.IsHumanParticipant && x.UserDisplayname.CompareTo(playername) == 0 && x.ParticipantFaction == faction).ParticipantCompany;

        /// <summary>
        /// Create a new <see cref="Session"/> instance with a unique <see cref="Guid"/>.
        /// </summary>
        /// <param name="sessionInfo">The map name of the scenario to play on.</param>
        /// <returns>New <see cref="Session"/> with the given data.</returns>
        public static Session CreateSession(SessionInfo sessionInfo) {

            // Create the session
            Session session = new Session(sessionInfo.SelectedScenario, sessionInfo.SelectedGamemode) {
                TuningMod = sessionInfo.SelectedTuningMod
            };

            // Get player count
            int playerCount = GetPlayerCount(sessionInfo.FillAI, sessionInfo.Allies?.Length ?? 0, sessionInfo.Axis?.Length ?? 0, out int alliedFillAI, out int axisFillAI);

            // Add players
            session.m_participants = new SessionParticipant[playerCount];
            byte currentIndex = 0;
            byte playerTeamIndex = 0;

            if (sessionInfo.Allies != null) {
                foreach (SessionParticipant participant in sessionInfo.Allies) {
                    if (participant.TeamIndex != TEAM_ALLIES) {
                        throw new ArgumentException("A participant playing as 'Axis' was added to the 'Allies' team!");
                    }
                    session.m_participants[currentIndex++] = participant;
                }
                playerTeamIndex = (byte)sessionInfo.Allies.Length;
            }

            // Add the missing allied AI players
            AddAIPlayers(sessionInfo, session, TEAM_ALLIES, alliedFillAI, ref currentIndex, ref playerTeamIndex, x => sessionInfo.Axis[sessionInfo.Allies?.Length ?? 1 - 1 + x].ParticipantFaction);

            if (sessionInfo.Axis != null) {
                foreach (SessionParticipant participant in sessionInfo.Axis) {
                    if (participant.TeamIndex != TEAM_AXIS) {
                        throw new ArgumentException("A participant playing as 'Axis' was added to the 'Allies' team!");
                    }
                    session.m_participants[currentIndex++] = participant;
                }
                playerTeamIndex = (byte)sessionInfo.Axis.Length;
            } else {
                playerTeamIndex = 0;
            }

            // Add the missing axis AI players
            AddAIPlayers(sessionInfo, session, TEAM_AXIS, axisFillAI, ref currentIndex, ref playerTeamIndex, x => sessionInfo.Allies[sessionInfo.Axis?.Length ?? 1 - 1 + x].ParticipantFaction);

            // Set the game mode
            if (sessionInfo.SelectedGamemode != null) {
                session.AddSetting("gamemode_setting", sessionInfo.SelectedGamemode.Options[sessionInfo.SelectedGamemodeOption].Value);
            } else {
                Trace.WriteLine("Failed to read selected gamemode - using 500 as default");
                session.AddSetting("gamemode_setting", 500);
            }

            // Return the new session
            return session;

        }

        private static void AddAIPlayers(SessionInfo sinfo, Session session, SessionParticipantTeam team, 
            int fillAICount, ref byte currentIndex, ref byte playerTeamIndex, Func<int, Faction> complementaryFunc) {
            for (int i = 0; i < fillAICount; i++) {
                byte pIndex = currentIndex++;
                Faction complementary = Faction.GetComplementaryFaction(complementaryFunc(i));
                Company aiCompany = CompanyGenerator.Generate(complementary, sinfo.SelectedTuningMod.Guid.ToString().Replace("-", ""), false, true, false);
                aiCompany.Owner = "AIPlayer";
                session.m_participants[pIndex] = new SessionParticipant(sinfo.DefaultDifficulty, aiCompany, team, playerTeamIndex++);
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

        public static void ZipCompanies(Company[] allCompanies, ref SessionInfo sessionInfo) {

            for (int i = 0; i < allCompanies.Length; i++) {

                if (allCompanies[i].Army.IsAllied) {

                    int j = sessionInfo.Allies.IndexOf(x => x.GetID().ToString().CompareTo(allCompanies[i].Owner) == 0 && x.Difficulty == AIDifficulty.Human);
                    if (j >= 0) {
                        allCompanies[i].Owner = sessionInfo.Allies[j].UserDisplayname;
                        sessionInfo.Allies[j] = new SessionParticipant(sessionInfo.Allies[j].UserDisplayname, sessionInfo.Allies[j].UserID, allCompanies[i], TEAM_ALLIES, 0);
                    } else {
                        Trace.WriteLine($"Failed to pair allied company '{allCompanies[i].Name}' with a player...");
                    }

                } else {

                    int j = sessionInfo.Axis.IndexOf(x => x.GetID().ToString().CompareTo(allCompanies[i].Owner) == 0 && x.Difficulty == AIDifficulty.Human);
                    if (j >= 0) {
                        allCompanies[i].Owner = sessionInfo.Axis[j].UserDisplayname;
                        sessionInfo.Axis[j] = new SessionParticipant(sessionInfo.Axis[j].UserDisplayname, sessionInfo.Axis[j].UserID, allCompanies[i], TEAM_AXIS, 0);
                    } else {
                        Trace.WriteLine($"Failed to pair axis company '{allCompanies[i].Name}' with a player...");
                    }

                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => this.SessionID.ToString();

    }

}
