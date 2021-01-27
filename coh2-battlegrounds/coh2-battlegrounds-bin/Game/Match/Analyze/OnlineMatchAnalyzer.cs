using System;
using System.Collections.Generic;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Online;
using Battlegrounds.Online.Lobby;
using Battlegrounds.Online.Services;

namespace Battlegrounds.Game.Match.Analyze {

    /// <summary>
    /// Multiplayer match analysis strategy for analyzing multiplayer match data through a <see cref="ManagedLobby"/> instance. Extension of <see cref="SingleplayerMatchAnalyzer"/>.
    /// Can be extended with custom behaviour.
    /// </summary>
    public class OnlineMatchAnalyzer : SingleplayerMatchAnalyzer {

        private ReplayMatchData m_selfData; // This will be the data we'll be comparing against
        private List<IMatchData> m_matchData;
        private int m_humanCount; // Not including host

        public OnlineMatchAnalyzer() {
            this.m_matchData = new List<IMatchData>();
            this.m_humanCount = -1;
        }

        public override void OnPrepare(object caller, IMatchData toAnalyze) {

            // Set self data
            if (toAnalyze is ReplayMatchData replay) {
                this.m_selfData = replay;
            } else {
                // TODO: Handle
            }

            // Get the managed lobby
            var lobby = caller as ManagedLobby;

            // Try get connection
            if (lobby.GetConnection() is Connection connection) {

                // Get the amount of human players
                lobby.Teams.ForEach(x => x.ForEachMember(y => this.m_humanCount += y is HumanLobbyMember ? 1 : 0));

                // The amount of player's we've collected data from.
                int collectedCount = 0;

                // Collection method
                void Collect(Message message) {
                    if (message.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                        if (ulong.TryParse(message.Argument1, out ulong senderID)) { // Verify it's an ID
                            string filename = $"{senderID}_match.json";
                            string destination = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, filename);
                            if (FileHub.DownloadFile(destination, filename, lobby.LobbyFileID)) {
                                this.m_matchData.Add(JsonPlayback.FromJsonFile(destination));
                                collectedCount++;
                            } else {
                                // TODO: resend request or try again.
                            }
                        } else {
                            // TODO: Handle...
                        }
                    }
                }

                // Send the message
                connection.SendMessageWithResponse(new Message(MessageType.LOBBY_SYNCMATCH, toAnalyze.Session.SessionID.ToString()), Collect);

                // Wait until we've collected all
                bool timedOut = SyncService.WaitUntil(() => collectedCount >= this.m_humanCount, 100, 50).Then(() => { 
                    // TODO: Handle
                });

                if (timedOut) {
                    // TODO: Handle
                }

            } else {
                // TODO: Handle
            }

        }

        public override void OnAnalyze(object caller) {
            if (this.AnalyzeReplayData(this.m_selfData)) {
                foreach (IMatchData matchData in this.m_matchData) {
                    if (matchData is JsonPlayback playback) {
                        if (!playback.CompareAgainst(this.m_analysisResult)) {
                            
                            // TODO: Handle

                            break;

                        }
                    } else {
                        throw new NotImplementedException();
                    }
                }
            } else {
                // TODO: Handle
            }
        }

    }

}
