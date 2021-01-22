using Battlegrounds.Online;
using Battlegrounds.Online.Lobby;
using Battlegrounds.Online.Services;

namespace Battlegrounds.Game.Match.Finalizer {

    /// <summary>
    /// Finalize strategy for online matches. Extension of <see cref="SingleplayerFinalizer"/>.
    /// </summary>
    public class MultiplayerFinalizer : SingleplayerFinalizer {

        public override void Synchronize(object synchronizeObject) {

            // Get the lobby object
            var lobby = synchronizeObject as ManagedLobby;

            // Get the connection
            if (lobby.GetConnection() is Connection connection) {

                // Upload all external user companies
                foreach (var pair in this.m_companies) {
                    if (pair.Key.IsAIPlayer) {
                        continue;
                    }
                    if (!BattlegroundsInstance.IsLocalUser(pair.Key.ID)) {

                        string uploadName = $"{pair.Key.ID}_company.json";
                        byte[] companyBytes = pair.Value.ToBytes();

                        if (!FileHub.UploadFile(companyBytes, uploadName, lobby.LobbyFileID)) {
                            // TODO: Handle
                        }

                    } else {
                        this.CompanyHandler?.Invoke(pair.Value);
                    }
                }

                // Send message to external users that they can now get their updated company from the filehub.
                connection.SendMessage(new Message(MessageType.LOBBY_NOTIFY_MATCH));

            }

        }

    }

}
