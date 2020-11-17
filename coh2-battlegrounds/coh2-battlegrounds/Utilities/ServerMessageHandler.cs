using Battlegrounds.Game;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Online.Lobby;
using System;
using System.IO;
using System.Diagnostics;

using BattlegroundsApp.Views;

namespace BattlegroundsApp {

    public class ServerMessageHandler {

        private ManagedLobby m_lobbyInstance;
        private GameLobbyView m_lobbyWindow;

        public ManagedLobby Lobby => this.m_lobbyInstance;

        // only thing that semiworks in the lobby now is the chat; And that will also have to be moved to the VM right now it is in the View
        public ServerMessageHandler(GameLobbyView lobbyObject) {
            this.m_lobbyWindow = lobbyObject;
        }

        private void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message) {
            Trace.WriteLine($"[ManagedLobbyPlayerEventType.{type}] {from}: \"{message}\"");
            this.m_lobbyWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyPlayerEventType.Join:
                        this.m_lobbyWindow.lobbyChat.Text += $"[Lobby] {from} has joined.\n";
                        break;
                    case ManagedLobbyPlayerEventType.Leave:
                        this.m_lobbyWindow.lobbyChat.Text += $"[Lobby] {from} has left.\n";
                        break;
                    case ManagedLobbyPlayerEventType.Kicked:
                        this.m_lobbyWindow.lobbyChat.Text += $"[Lobby] {from} has been kicked.\n";
                        break;
                    case ManagedLobbyPlayerEventType.Message:
                        this.m_lobbyWindow.lobbyChat.Text += $"{from}: {message}\n";
                        break;
                    case ManagedLobbyPlayerEventType.Meta:
                        string metaMessage = $"{from}: {message}";
                        Trace.WriteLine(metaMessage);
                        break;
                    default:
                        Trace.WriteLine($"Unhandled event type \"ManagedLobbyPlayerEventType.{type}\"");
                        break;
                }
            });
        }

        public object OnLocalDataRequest(string type) {

            if (type.CompareTo("CompanyData") == 0) {
                return Company.ReadCompanyFromFile("test_company.json");
            } else if (type.CompareTo("MatchInfo") == 0) {
                return this.m_lobbyWindow.CreateSessionInfo();
            } else {
                return null;
            }

        }

        public void OnDataRequest(bool isFileRequest, string asker, string requestedData, int id) {
            if (requestedData.CompareTo("CompanyData") == 0) {
                /*if (__LobbyInstance.SendFile(asker, "test_company.json", id, true) == -1) {
                    Trace.WriteLine("Failed to send test company");
                }*/
            }
        }

        public void StartMatchCommandReceived() {

            // Statrt the game
            if (!CoH2Launcher.Launch()) {
                Trace.WriteLine("Failed to launch Company of Heroes 2...");
            } else {
                Trace.WriteLine("Launched Company of Heroes 2");
            }

        }

        public void OnFileReceived(string sender, string filename, bool received, byte[] content, int id) {

            // Did we receive the battlegrounds .sga
            if (received && filename.CompareTo("coh2_battlegrounds_wincondition.sga") == 0) {

                this.m_lobbyWindow.UpdateGUI(() => {

                    // Path to the sga file we'll write to
                    string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

                    // Delete file if it already exists
                    if (File.Exists(sgapath)) {
                        File.Delete(sgapath);
                    }

                    // Write all byte content
                    File.WriteAllBytes(sgapath, content);

                    // Let the user know we've received the win condition
                    this.m_lobbyWindow.lobbyChat.Text += "[Lobby] Received wincondition file from host.\n";

                    // Write a log message
                    Trace.WriteLine($"Received and saved .sga to \"{sgapath}\"");

                });

            } else {
                Trace.WriteLine(sender + ":" + filename + ":" + received);
                // TODO: Handle other cases
            }

        }

        private void OnLocalEvent(ManagedLobbyLocalEventType type, string message) {
            Trace.WriteLine($"[ManagedLobbyLocalEventType.{type}] \"{message}\"");
            this.m_lobbyWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyLocalEventType.Host:
                        m_lobbyWindow.lobbyChat.Text += "[Lobby] You have been assigned as host.\n";
                        break;
                    case ManagedLobbyLocalEventType.Kicked:
                        // TODO: Messagebox for this
                        break;
                    default:
                        Trace.WriteLine($"Unhandled event type \"ManagedLobbyLocalEventType.{type}\"");
                        break;
                }
            });
        }

        public void OnServerResponse(ManagedLobbyStatus status, ManagedLobby result) {
            if (status.Success) {

                // Assign instance
                this.m_lobbyInstance = result;

                // Hook callback methods
                this.m_lobbyInstance.OnPlayerEvent += OnPlayerEvent;
                this.m_lobbyInstance.OnLocalEvent += OnLocalEvent;
                this.m_lobbyInstance.OnLocalDataRequested += OnLocalDataRequest;
                this.m_lobbyInstance.OnDataRequest += OnDataRequest;
                this.m_lobbyInstance.OnStartMatchReceived += StartMatchCommandReceived;

                // Connect method
                this.m_lobbyWindow.ServerConnectResponse(true);

            } else {
                Trace.WriteLine(status.Message);
                this.m_lobbyWindow.ServerConnectResponse(false);
            }
        }

        public void LeaveLobby() {
            if (this.m_lobbyInstance != null) {
                this.m_lobbyInstance.Leave(); // Async... will need a callback for this when done.
            }
        }

    }

}
