using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Online.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Battlegrounds.Steam;
using BattlegroundsApp.Views;

namespace BattlegroundsApp {

    public class ServerMessageHandler {

        private ManagedLobby m_LobbyInstance; // Oughta rename this to m_lobbyInstance.... my bad :D
        private GameLobbyView m_lobbyWindow; // I have no idea where the lobby logic is; The actual lobby

        public ManagedLobby Lobby => this.m_LobbyInstance;

        // only thing that semiworks in the lobby now is the chat; And that will also have to be moved to the VM right now it is in the View
        public ServerMessageHandler(GameLobbyView lobbyObject) {
            this.m_lobbyWindow = lobbyObject;
        }

        private void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message) {
            Trace.WriteLine($"[ManagedLobbyPlayerEventType.{type}] {from}: \"{message}\"");
            m_lobbyWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyPlayerEventType.Join:
                        m_lobbyWindow.lobbyChat.Text += $"[Lobby] {from} has joined.\n";
                        m_lobbyWindow.AddPlayer(from);
                        break;
                    case ManagedLobbyPlayerEventType.Leave:
                        m_lobbyWindow.lobbyChat.Text += $"[Lobby] {from} has left.\n";
                        m_lobbyWindow.RemovePlayer(from);
                        break;
                    case ManagedLobbyPlayerEventType.Kicked:
                        m_lobbyWindow.lobbyChat.Text += $"[Lobby] {from} has been kicked.\n";
                        m_lobbyWindow.RemovePlayer(from);
                        break;
                    case ManagedLobbyPlayerEventType.Message:
                        m_lobbyWindow.lobbyChat.Text += $"{from}: {message}\n";
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
                return new SessionInfo() { // should probably be redirected to Mainwindow and let it set up this (when considering players and settings)
                    SelectedGamemode = WinconditionList.GetWinconditionByName("Victory Points"),
                    SelectedGamemodeOption = 1,
                    SelectedScenario = ScenarioList.FromFilename("2p_angoville_farms"),
                    SelectedTuningMod = new BattlegroundsTuning(),
                    Allies = new SessionParticipant[] { new SessionParticipant(SteamUser.FromID(76561198003529969UL), null, 0, 0) }, // We'll have to solve participant setup later
                    Axis = new SessionParticipant[] { new SessionParticipant(SteamUser.FromID(76561198157626935UL), null, 0, 0) },
                    FillAI = false,
                    DefaultDifficulty = AIDifficulty.AI_Hard,
                };
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
                    m_lobbyWindow.lobbyChat.Text += "[Lobby] Received wincondition file from host.\n";

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
            m_lobbyWindow.UpdateGUI(() => {
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

                this.m_LobbyInstance = result;

                this.m_LobbyInstance.OnPlayerEvent += OnPlayerEvent;
                this.m_LobbyInstance.OnLocalEvent += OnLocalEvent;
                this.m_LobbyInstance.OnLocalDataRequested += OnLocalDataRequest;
                this.m_LobbyInstance.OnDataRequest += OnDataRequest;
                this.m_LobbyInstance.OnStartMatchReceived += StartMatchCommandReceived;

                this.m_lobbyWindow.ServerConnectResponse(true);

                Trace.WriteLine("Server responded with OK");

            } else {
                Trace.WriteLine(status.Message);
                this.m_lobbyWindow.ServerConnectResponse(false);
            }
        }

        public void LeaveLobby() {
            if (this.m_LobbyInstance != null) {
                this.m_LobbyInstance.Leave(); // Async... will need a callback for this when done.
            }
        }

    }

}
