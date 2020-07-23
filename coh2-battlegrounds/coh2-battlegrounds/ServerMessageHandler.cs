using Battlegrounds;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Online.Services;
using coh2_battlegrounds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace BattlegroundsApp
{
    internal static class ServerMessageHandler
    {

        private static ManagedLobby __LobbyInstance;

        public static ManagedLobby CurrentLobby => __LobbyInstance;

        private static void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message)
        {
            var mainWindow = MainWindow.Instance;

            switch (type)
            {
                case ManagedLobbyPlayerEventType.Join:
                    {
                        string joinMessage = $"[Lobby] {mainWindow.user.Name} has joined.\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + joinMessage;

                        mainWindow.AddPlayer(mainWindow.user.Name);

                        break;
                    }
                case ManagedLobbyPlayerEventType.Leave:
                    {
                        string leaveMessage = $"[Lobby] {mainWindow.user.Name} has left.\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + leaveMessage;

                        mainWindow.RemovePlayer(mainWindow.user.Name);

                        break;
                    }
                case ManagedLobbyPlayerEventType.Kicked:
                    {
                        string kickMessage = $"[Lobby] {mainWindow.user.Name} has been kicked.\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + kickMessage;

                        mainWindow.RemovePlayer(mainWindow.user.Name);

                        break;
                    }
                case ManagedLobbyPlayerEventType.Message:
                    {
                        string messageMessage = $"{from}: {message}\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + messageMessage;

                        break;
                    }
                case ManagedLobbyPlayerEventType.Meta:
                    {
                        string metaMessage = $"{from}: {message}";
                        Console.WriteLine(metaMessage);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Something went wrong.");
                        break;
                    }
            }
        }

        public static object OnLocalDataRequest(string type) {

            if (type.CompareTo("CompanyData") == 0) {
                return Company.ReadCompanyFromFile("test_company.json");
            } else if (type.CompareTo("MatchInfo") == 0) {
                return new SessionInfo() { // should probably be redirected to Mainwindow and let it set up this (when considering players and settings)
                    SelectedGamemode = WinconditionList.GetWinconditionByName("Victory Point"),
                    SelectedGamemodeOption = 1,
                    SelectedScenario = ScenarioList.FromFilename("2p_angoville_farms"),
                    SelectedTuningMod = new BattlegroundsTuning(),
                    Allies = new SessionParticipant[] { new SessionParticipant(BattlegroundsInstance.LocalSteamuser, null, 0, 0) }, // Should not be using LocalSteamuser here, will have to figure something out
                    Axis = new SessionParticipant[] { new SessionParticipant(BattlegroundsInstance.LocalSteamuser, null, 0, 0) },
                    FillAI = false,
                    DefaultDifficulty = Battlegrounds.Game.AIDifficulty.AI_Hard,
                };
            } else {
                return null;
            }

        }

        public static void OnDataRequest(bool isFileRequest, string asker, string requestedData, int id) {
            if (requestedData.CompareTo("CompanyData") == 0) {
                __LobbyInstance.SendFile(asker, "test_company.json", id);
            }
        }

        public static void OnServerResponse(ManagedLobbyStatus status, ManagedLobby result)
        {
            if (status.Success) {

                __LobbyInstance = result;

                MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.OnLobbyEnter(result) );

                __LobbyInstance.OnPlayerEvent += OnPlayerEvent;

                __LobbyInstance.OnLocalDataRequested += OnLocalDataRequest;
                __LobbyInstance.OnDataRequest += OnDataRequest;

            } else {
                Console.WriteLine(status.Message);
            }
        }
    }
}
