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

namespace BattlegroundsApp {

    internal static class ServerMessageHandler {

        private static ManagedLobby __LobbyInstance;

        public static ManagedLobby CurrentLobby => __LobbyInstance;

        private static void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message) {
            var mainWindow = MainWindow.Instance;
            Trace.WriteLine($"[ManagedLobbyPlayerEventType.{type}] {from}: \"{message}\"");
            mainWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyPlayerEventType.Join:
                        mainWindow.chatBox.Text += $"[Lobby] {from} has joined.\n";
                        mainWindow.AddPlayer(from);
                        break;
                    case ManagedLobbyPlayerEventType.Leave:
                        mainWindow.chatBox.Text += $"[Lobby] {from} has left.\n";
                        mainWindow.RemovePlayer(from);
                        break;
                    case ManagedLobbyPlayerEventType.Kicked:
                        mainWindow.chatBox.Text += $"[Lobby] {from} has been kicked.\n";
                        mainWindow.RemovePlayer(from);
                        break;
                    case ManagedLobbyPlayerEventType.Message:
                        mainWindow.chatBox.Text += $"{from}: {message}\n";
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

        public static object OnLocalDataRequest(string type) {

            if (type.CompareTo("CompanyData") == 0) {
                return Company.ReadCompanyFromFile("test_company.json");
            } else if (type.CompareTo("MatchInfo") == 0) {
                return new SessionInfo() { // should probably be redirected to Mainwindow and let it set up this (when considering players and settings)
                    SelectedGamemode = WinconditionList.GetWinconditionByName("Victory Points"),
                    SelectedGamemodeOption = 1,
                    SelectedScenario = ScenarioList.FromFilename("2p_angoville_farms"),
                    SelectedTuningMod = new BattlegroundsTuning(),
                    Allies = new SessionParticipant[] { new SessionParticipant("CoDiEx", null, 0, 0) }, // We'll have to solve this later
                    Axis = new SessionParticipant[] { new SessionParticipant("Ragnar", null, 0, 0) },
                    FillAI = false,
                    DefaultDifficulty = AIDifficulty.AI_Hard,
                };
            } else {
                return null;
            }

        }

        public static void OnDataRequest(bool isFileRequest, string asker, string requestedData, int id) {
            if (requestedData.CompareTo("CompanyData") == 0) {
                if (__LobbyInstance.SendFile(asker, "test_company.json", id) == -1) {
                    Trace.WriteLine("Failed to send test company");
                }
            }
        }

        public static void StartMatchCommandReceived() {

            // Statrt the game
            if (!CoH2Launcher.Launch()) {
                Trace.WriteLine("Failed to launch Company of Heroes 2...");
            }

        }

        public static void OnFileReceived(string sender, string filename, bool received, byte[] content, int id) {

            // Did we receive the battlegrounds .sga
            if (received && filename.CompareTo("coh2_battlegrounds_wincondition.sga") == 0) {

                // Path to the sga file we'll write to
                string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\coh2_battlegrounds_wincondition.sga";

                // Delete file if it already exists
                if (File.Exists(sgapath)) {
                    File.Delete(sgapath);
                }

                // Write all byte content
                File.WriteAllBytes(sgapath, content);

                // Let the user know we've received the win condition
                MainWindow.Instance.chatBox.Text += "[Lobby] Received wincondition file from host.\n";

                // Write a log message
                Trace.WriteLine("Received and saved .sga");

            } else {
                // TODO: Handle other cases
            }

        }

        private static void OnLocalEvent(ManagedLobbyLocalEventType type, string message) {
            var mainWindow = MainWindow.Instance;
            Trace.WriteLine($"[ManagedLobbyLocalEventType.{type}] \"{message}\"");
            mainWindow.UpdateGUI(() => {
                switch (type) {
                    case ManagedLobbyLocalEventType.Host:
                        mainWindow.chatBox.Text += "[Lobby] You have been assigned as host.\n";
                        break;
                    case ManagedLobbyLocalEventType.Kicked:
                        mainWindow.ClearLobby();
                        // TODO: Messagebox for this
                        break;
                    default:
                        Trace.WriteLine($"Unhandled event type \"ManagedLobbyLocalEventType.{type}\"");
                        break;
                }
            });
        }

        public static void OnServerResponse(ManagedLobbyStatus status, ManagedLobby result) {
            if (status.Success) {

                __LobbyInstance = result;

                __LobbyInstance.OnPlayerEvent += OnPlayerEvent;
                __LobbyInstance.OnLocalEvent += OnLocalEvent;
                __LobbyInstance.OnLocalDataRequested += OnLocalDataRequest;
                __LobbyInstance.OnDataRequest += OnDataRequest;
                __LobbyInstance.OnStartMatchReceived += StartMatchCommandReceived;
                __LobbyInstance.OnFileReceived += OnFileReceived;

                MainWindow.Instance.UpdateGUI(() => MainWindow.Instance.OnLobbyEnter(__LobbyInstance));

                Trace.WriteLine("Server responded with OK");

            } else {
                Trace.WriteLine(status.Message);
            }
        }

        public static void LeaveLobby() {
            if (__LobbyInstance != null) {
                __LobbyInstance.Leave(); // Async... will need a callback for this when done.
                //__LobbyInstance = null;
            }
        }

    }

}
