using Battlegrounds.Game;
using Battlegrounds.Online.Lobby;
using System;
using System.IO;
using System.Diagnostics;

using BattlegroundsApp.Views;

namespace BattlegroundsApp {

    public delegate void MetaMessageListener(string from, string meta);

    public delegate void PlayerJoinedListener(string name, ulong id);

    public delegate void MapChangedListener(string scenario);

    public delegate void GamemodechangedListener(string gamemode, string setting);

    public delegate void KickedListener(string message);

    public delegate void CompanyRequestListener(string reason);

    public class ServerMessageHandler {

        private ManagedLobby m_lobbyInstance;
        private GameLobbyView m_lobbyWindow;

        public ManagedLobby Lobby => this.m_lobbyInstance;

        public event MetaMessageListener MetaMessageReceived;

        public event PlayerJoinedListener OnPlayerJoined;

        public event KickedListener OnPlayerKicked;

        public event MapChangedListener OnMapChanged;

        public event GamemodechangedListener OnGamemodeChanged;

        public event CompanyRequestListener OnCompanyRequested;

        public ServerMessageHandler(GameLobbyView view, ManagedLobby lobby) {
            
            // Set lobby view
            this.m_lobbyWindow = view;

            // Set lobby instance
            this.m_lobbyInstance = lobby;

            // Hook callback methods
            this.m_lobbyInstance.OnPlayerEvent += this.OnPlayerEvent;
            this.m_lobbyInstance.OnLocalEvent += this.OnLocalEvent;
            this.m_lobbyInstance.OnLocalDataRequested += this.OnLocalDataRequest;
            this.m_lobbyInstance.OnDataRequest += this.OnDataRequest;
            this.m_lobbyInstance.OnStartMatchReceived += this.StartMatchCommandReceived;
            this.m_lobbyInstance.OnLobbyInfoChanged += this.OnLobbyInfoChanged;
            this.m_lobbyInstance.OnMatchInfoReceived += this.OnLobbyMatchInfo;

        }

        private void OnLobbyMatchInfo(string type, string arg1, string arg2) {
            this.m_lobbyWindow.UpdateGUI(() => {
                switch (type) {
                    case "LOBBY_STARTING":
                        if (!this.m_lobbyInstance.IsHost) {
                            this.m_lobbyWindow.lobbyChat.Text += $"[Lobby] The host has presset the start game button.\n";
                            this.OnCompanyRequested?.Invoke("HOST");
                        }
                        break;
                    default:
                        break;
                }
            });
        }

        private void OnLobbyInfoChanged(string info, string value) {
            this.m_lobbyWindow.UpdateGUI(() => {
                switch (info) {
                    case ManagedLobby.LOBBYINFO_SELECTEDMAP:
                        this.OnMapChanged?.Invoke(value);
                        break;
                    case ManagedLobby.LOBBYINFO_SELECTEDGAMEMODE:
                        this.OnGamemodeChanged?.Invoke(value, this.m_lobbyInstance.SelectedGamemodeOption);
                        break;
                    case ManagedLobby.LOBBYINFO_SELECTEDGAMEMODEOPTION:
                        this.OnGamemodeChanged?.Invoke(this.m_lobbyInstance.SelectedGamemode, value);
                        break;
                    case ManagedLobby.LOBBYINFO_CAPACITY:
                        this.m_lobbyWindow.RefreshTeams(null);
                        break;
                    default:
                        Trace.WriteLine($"Unknown lobby info change [{info}:{value}]", "ServerMessageHandler");
                        break;
                }
            });
        }

        private void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message) {
            Trace.WriteLine($"[ManagedLobbyPlayerEventType.{type}] {from}: \"{message}\"", "ServerMessageHandler");
            this.m_lobbyWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyPlayerEventType.Join:
                        this.OnPlayerJoined?.Invoke(from, ulong.Parse(message));
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
                        this.MetaMessageReceived?.Invoke(from, message);
                        Trace.WriteLine(metaMessage, "ServerMessageHandler");
                        break;
                    default:
                        Trace.WriteLine($"Unhandled event type \"ManagedLobbyPlayerEventType.{type}\"", "ServerMessageHandler");
                        break;
                }
            });
        }

        public object OnLocalDataRequest(string type) {

            if (type.CompareTo("CompanyData") == 0) {
                return this.m_lobbyWindow.GetLocalCompany();// Company.ReadCompanyFromFile("test_company.json");
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
                Trace.WriteLine("Failed to launch Company of Heroes 2...", "ServerMessageHandler");
            } else {
                Trace.WriteLine("Launched Company of Heroes 2", "ServerMessageHandler");
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
                    Trace.WriteLine($"Received and saved .sga to \"{sgapath}\"", "ServerMessageHandler");

                });

            } else {
                Trace.WriteLine(sender + ":" + filename + ":" + received, "ServerMessageHandler");
                // TODO: Handle other cases
            }

        }

        private void OnLocalEvent(ManagedLobbyLocalEventType type, string message) {
            Trace.WriteLine($"[ManagedLobbyLocalEventType.{type}] \"{message}\"");
            this.m_lobbyWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyLocalEventType.Host:
                        this.m_lobbyWindow.lobbyChat.Text += "[Lobby] You have been assigned as host.\n";
                        this.m_lobbyWindow.EnableHostMode(true);
                        break;
                    case ManagedLobbyLocalEventType.HostRemove:
                        this.m_lobbyWindow.lobbyChat.Text += "[Lobby] You have been un-assigned as host.\n";
                        this.m_lobbyWindow.EnableHostMode(false);
                        break;
                    case ManagedLobbyLocalEventType.Kicked:
                        this.OnPlayerKicked?.Invoke(message);
                        break;
                    default:
                        Trace.WriteLine($"Unhandled event type \"ManagedLobbyLocalEventType.{type}\"", "ServerMessageHandler");
                        break;
                }
            });
        }

        public void LeaveLobby() {
            if (this.m_lobbyInstance != null) {
                this.m_lobbyInstance.Leave(); // Async... will need a callback for this when done.
            }
        }

    }

}
