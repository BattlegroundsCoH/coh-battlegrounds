using Battlegrounds.Game.Battlegrounds;
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
        internal static LobbyHub hub = new LobbyHub();

        private static ManagedLobby __LobbyInstance;

        private static void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message)
        {
            var mainWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

            switch (type)
            {
                case ManagedLobbyPlayerEventType.Join:
                    {
                        string joinMessage = $"[Lobby] {mainWindow.user.Name} has joined.\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + joinMessage;

                        mainWindow.AddPlayer();

                        break;
                    }
                case ManagedLobbyPlayerEventType.Leave:
                    {
                        string leaveMessage = $"[Lobby] {mainWindow.user.Name} has left.\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + leaveMessage;

                        mainWindow.RemovePlayer();

                        break;
                    }
                case ManagedLobbyPlayerEventType.Kicked:
                    {
                        string kickMessage = $"[Lobby] {mainWindow.user.Name} has been kicked.\n";
                        mainWindow.chatBox.Text = mainWindow.chatBox.Text + kickMessage;

                        mainWindow.RemovePlayer();

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

        public static void OnServerResponse(ManagedLobbyStatus status, ManagedLobby resault)
        {
            if (status.Success)
            {
                __LobbyInstance = resault;

                __LobbyInstance.OnPlayerEvent += OnPlayerEvent;
            }

            //resault.OnLocalDataRequested {}

            //resault.OnDataRequest {}
        }
    }
}
