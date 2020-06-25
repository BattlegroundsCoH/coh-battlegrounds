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
                    TotalLosses = 0,
                    IsOnWinningTeam = false
                };
            }

            // Get the tick array
            var ticks = this.m_matchRecord.Ticks;

            // Run through the array
            for (int i = 0; i < ticks.Length; i++) {

                // If there's something to work with
                if (ticks[i].Events.Count > 0) {

                    // Loop through all the gave events in tick
                    foreach (GameEvent e in ticks[i].Events) {

                        if (e.Type <= (byte)GameEventType.PCMD_COUNT) {

                            Console.WriteLine(e.EventType);

                            // If a squad was built
                            if (e.EventType == GameEventType.CMD_BuildSquad) {



                            }

                        }

                    }

                    Console.WriteLine(ticks[i].TimeStamp);

                }

            }

        }

    }

}
