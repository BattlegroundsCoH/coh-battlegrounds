using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Battlegrounds {

    /// <summary>
    /// 
    /// </summary>
    public class GameMatch {

        ReplayFile m_matchRecord;
        PlayerResult[] m_matchPlayerResults;

        /// <summary>
        /// 
        /// </summary>
        public PlayerResult[] Players => m_matchPlayerResults;

        /// <summary>
        /// 
        /// </summary>
        public GameMatch() {
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



        }

        private void ParseBroadcastMessage(string msg, PlayerResult player, HashSet<Squad> allsquads) {

            if (msg.Length > 0) {

                char msgtype = char.ToUpper(msg[0]); // Always bump it to upper (incase it's forgotten in Scar script)

                if (msgtype == 'S') {

                    Match result = Regex.Match(msg.Substring(1), @"\[(?<uid>\d+),(?<bp>\S+)\]");

                    ushort uid = ushort.Parse(result.Groups["uid"].Value);
                    string bp = result.Groups["bp"].Value;

                    Squad squad = new Squad(uid, player.Player, BlueprintManager.FromBlueprintName(bp, BlueprintType.SBP));
                    allsquads.Add(squad);
                    player.AddSquad(squad);

                } else if (msgtype == 'K') {

                    Match result = Regex.Match(msg.Substring(1), @"\[(?<uid>\d+)\]");
                    ushort uid = ushort.Parse(result.Groups["uid"].Value);

                    Squad s = allsquads.FirstOrDefault(x => x.SquadID == uid);

                    if (s != null) {

                        player.RemoveSquad(s);
                        allsquads.Remove(s);

                    }

                }

            } else {
                // Some sort of error?
            }

        }

    }

}
