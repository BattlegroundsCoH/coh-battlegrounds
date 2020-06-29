using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Battlegrounds {

    /// <summary>
    /// Representation of a match wherein the results of the match can be evaluated and verified within the <see cref="Session"/> the match was played.
    /// </summary>
    public class GameMatch {

        ReplayFile m_matchRecord;
        Session m_gameSession;
        PlayerResult[] m_matchPlayerResults;

        /// <summary>
        /// 
        /// </summary>
        public PlayerResult[] Players => m_matchPlayerResults;

        /// <summary>
        /// 
        /// </summary>
        public GameMatch(Session session) {
            m_matchPlayerResults = new PlayerResult[0];
            m_gameSession = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="replayFilepath"></param>
        /// <returns></returns>
        public bool LoadMatch(string replayFilepath) {

            // Create instance
            this.m_matchRecord = new ReplayFile(replayFilepath);

            // Load the replay
            if (!this.m_matchRecord.LoadReplay()) {
                return false;
            }

            // Return true
            return true;

        }

        /// <summary>
        /// Evaluate the result of the match based on the replay file
        /// </summary>
        public virtual void EvaluateResult() {

            // Get the players
            Player[] players = this.m_matchRecord.Players;

            // Setup variables
            this.m_matchPlayerResults = new PlayerResult[players.Length];

            // Run Setup results
            for (int i = 0; i < this.m_matchPlayerResults.Length; i++) {
                this.m_matchPlayerResults[i] = new PlayerResult(players[i]) {
                    IsOnWinningTeam = false
                };
            }

            // Create containers for storing all spawned entities and squads
            HashSet<Squad> allSquads = new HashSet<Squad>();
            HashSet<Entity> allEntities = new HashSet<Entity>();

            // Get the tick array
            var ticks = this.m_matchRecord.Ticks;

            // Run through the array
            for (int i = 0; i < ticks.Length; i++) {

                // If there's something to work with
                if (ticks[i].Events.Count > 0) {

                    // Loop through all the gave events in tick
                    foreach (GameEvent e in ticks[i].Events) {

                        // It is an event we can work with?
                        if (e.Type < (byte)GameEventType.EVENT_MAX) {

                            // Is it a broadcast message?
                            if (e.EventType == GameEventType.PCMD_BroadcastMessage) {
                                this.ParseBroadcastMessage(e.AttachedMessage, this.m_matchPlayerResults.FirstOrDefault(x => x.Player.ID == e.PlayerID), allSquads);
                            }

                        }

                    }

                }

            }

            Console.WriteLine("Updating companies");

            // Update all companies
            this.UpdateCompanies();

        }

        private void ParseBroadcastMessage(string msg, PlayerResult player, HashSet<Squad> allsquads) {

            if (msg.Length > 0) {

                Match messageMatchResult = Regex.Match(msg, @"(?<cmdtype>\w)\[(?<msg>\S+|\d+)*\]");

                if (messageMatchResult.Success) {

                    char msgtype = char.ToUpper(messageMatchResult.Groups["cmdtype"].Value[0]); // Always bump it to upper (incase it's forgotten in Scar script)

                    if (msgtype == 'D') {

                        Squad squad = new Squad(ushort.Parse(messageMatchResult.Groups["msg"].Value), player.Player, null);
                        allsquads.Add(squad);
                        player.AddSquad(squad);

                        Console.WriteLine("Player " + player.Player.Name + " deployed " + squad.SquadID);

                    } else if (msgtype == 'K') {

                        ushort squadID = ushort.Parse(messageMatchResult.Groups["msg"].Value);
                        Squad squad = allsquads.FirstOrDefault(x => x.SquadID == squadID);

                        if (squad != null) {

                            allsquads.Remove(squad);
                            player.RemoveSquad(squad);

                            Console.WriteLine("Player " + player.Player.Name + " lost " + squad.SquadID);

                        }

                    } // Other commands here

                }

            } else {
                // Some sort of error?
            }

        }

        private void UpdateCompanies() {

            for (int i = 0; i < m_matchPlayerResults.Length; i++) {

                Company company = m_gameSession.Companies.FirstOrDefault(x => x.Owner.Name == m_matchPlayerResults[i].Player.Name);

                if (company == null) {
                    // some error?
                }

                // We now remove all the squads lost
                foreach (Squad squad in m_matchPlayerResults[i].Losses) {
                    if (!company.RemoveSquad(squad.SquadID)) {
                        Console.WriteLine("Lost a squad that was not deployed by player!");
                    }
                }

                // And we now update squads
                foreach (Squad squad in m_matchPlayerResults[i].Alive) {

                    Squad companySquad = company.GetSquadByIndex(squad.SquadID);

                    // TODO: Increase rank, add upgrades, slot items etc.

                }

            }

        }

    }

}
