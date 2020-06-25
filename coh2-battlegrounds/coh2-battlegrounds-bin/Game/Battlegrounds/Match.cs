using System;
using System.Collections.Generic;
using System.Text;
using coh2_battlegrounds_bin.Game.Gameplay;

namespace coh2_battlegrounds_bin.Game.Battlegrounds {

    /// <summary>
    /// 
    /// </summary>
    public class Match {

        ReplayFile m_matchRecord;
        PlayerResult[] m_matchPlayerResults;

        /// <summary>
        /// 
        /// </summary>
        public PlayerResult[] Players => m_matchPlayerResults;

        /// <summary>
        /// 
        /// </summary>
        public Match() {
            m_matchPlayerResults = new PlayerResult[0];
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

            // Log out what we're doing
            Console.WriteLine($"Evaluating match results for match on: {this.m_matchRecord.Scenario.Name}.");

            // Get the players
            Player[] players = this.m_matchRecord.Players;

            // Setup variables
            this.m_matchPlayerResults = new PlayerResult[players.Length];

            // Run Setup results
            for (int i = 0; i < this.m_matchPlayerResults.Length; i++) {
                this.m_matchPlayerResults[i] = new PlayerResult(players[i]) {
                    TotalLosses = 0,
                    IsOnWinningTeam = false
                };
            }

            // Create containers for storing all spawned entities and squads
            Dictionary<uint, Entity> allEntities = new Dictionary<uint, Entity>();
            Dictionary<uint, Squad> allSquads = new Dictionary<uint, Squad>();

            // Get the tick array
            var ticks = this.m_matchRecord.Ticks;

            // Run through the array
            for (int i = 0; i < ticks.Length; i++) {

                // If there's something to work with
                if (ticks[i].Events.Count > 0) {

                    // Loop through all the gave events in tick
                    foreach (GameEvent e in ticks[i].Events) {

                        // We don't care about AI players
                        //if (players[e.PlayerID].IsAIPlayer)
                        //    continue;

                        if (e.Type <= (byte)GameEventType.PCMD_COUNT) {

                            //Console.WriteLine(e.EventType);

                            if (e.TargetType == 16) {

                                if (e.EventType == GameEventType.CMD_BuildSquad || e.EventType == GameEventType.CMD_Upgrade) {
                                    Console.WriteLine($"Entity: {e.EventType}; Player={e.PlayerID}; ID={e.UnitID}; BP={e.BlueprintID}");
                                } else {
                                    Console.WriteLine($"Entity: {e.EventType}; Player={e.PlayerID}; ID={e.UnitID}");
                                }

                            } else if (e.TargetType == 32) {

                                if (e.EventType == GameEventType.SCMD_Upgrade || e.EventType == GameEventType.SCMD_InstantUpgrade) {
                                    Console.WriteLine($"Squad: {e.EventType}; Player={e.PlayerID}; ID={e.UnitID}; BP={e.BlueprintID}");
                                } else {
                                    Console.WriteLine($"Squad: {e.EventType}; Player={e.PlayerID}; ID={e.UnitID}");
                                }

                            }

                        }

                    }

                }

            }

            Console.WriteLine($"Registered {allSquads.Count} squads at end of match");
            Console.WriteLine($"Registered {allEntities.Count} entities at end of match");

        }

    }

}
