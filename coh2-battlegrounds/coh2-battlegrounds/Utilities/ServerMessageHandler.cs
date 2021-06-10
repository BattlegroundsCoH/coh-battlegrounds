using System;
using System.IO;
using System.Diagnostics;

using Battlegrounds.Game;
using Battlegrounds.Online.Lobby;

using BattlegroundsApp.Views;
using BattlegroundsApp.Controls.Lobby.Chatting;

namespace BattlegroundsApp {

    public delegate void MetaMessageListener(string from, string meta);

    public delegate void PlayerJoinedListener(string name, ulong id);

    public delegate void MapChangedListener(string scenario);

    public delegate void GamemodechangedListener(string gamemode, string setting);

    public delegate void KickedListener(string message);

    public delegate void CompanyRequestListener(string reason);

    public delegate void StartingMatchListener(int time, string guid);

    public class ServerMessageHandler {

        private ManagedLobby m_lobbyInstance;
        private GameLobbyView m_lobbyWindow;

        public ManagedLobby Lobby => this.m_lobbyInstance;

        public string Self => this.Lobby.Self.Name;

        public event MetaMessageListener MetaMessageReceived;

        public event PlayerJoinedListener OnPlayerJoined;

        public event KickedListener OnPlayerKicked;

        public event MapChangedListener OnMapChanged;

        public event GamemodechangedListener OnGamemodeChanged;

        public event CompanyRequestListener OnCompanyRequested;

        public event StartingMatchListener OnMatchStarting;


        public ServerMessageHandler(GameLobbyView view, ManagedLobby lobby) {
            
            // Set lobby view
            this.m_lobbyWindow = view;

            // Set lobby instance
            this.m_lobbyInstance = lobby;

            // Hook callback methods
            this.m_lobbyInstance.OnPlayerEvent += this.OnPlayerEvent;
            this.m_lobbyInstance.OnLocalEvent += this.OnLocalEvent;
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
                            if (int.TryParse(arg1, out int time)) {
                                this.OnMatchStarting?.Invoke(time, arg2);
                                //this.OnReceived?.Invoke("[Lobby]", "The host has pressed the start game button.");
                                this.OnCompanyRequested?.Invoke("HOST"); // Tell member to upload company
                            }
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
        }

        public void OnDataRequest(bool isFileRequest, string asker, string requestedData, int id) {
        }

        public void StartMatchCommandReceived() {

        }

        public void OnFileReceived(string sender, string filename, bool received, byte[] content, int id) {

        }

        private void OnLocalEvent(ManagedLobbyLocalEventType type, string message) {
        }

        public void LeaveLobby(Action onLeft) {
            if (this.m_lobbyInstance != null) {
                this.m_lobbyInstance.Leave(x => onLeft?.Invoke()); // Async... will need a callback for this when done.
            }
        }

        public void SendChatMessage(string message) => this.m_lobbyInstance.SendChatMessage(message);

    }

}
