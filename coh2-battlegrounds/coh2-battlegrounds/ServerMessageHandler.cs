using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Online.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BattlegroundsApp
{
    internal static class ServerMessageHandler
    {
        internal static LobbyHub hub = new LobbyHub();

        private static ManagedLobby __LobbyInstance;

        private static void OnPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message)
        {
            switch (type)
            {
                case ManagedLobbyPlayerEventType.Join:
                    {
                        break;
                    }
                case ManagedLobbyPlayerEventType.Leave:
                    {
                        break;
                    }
                case ManagedLobbyPlayerEventType.Kicked:
                    {
                        break;
                    }
                case ManagedLobbyPlayerEventType.Message:
                    {
                        break;
                    }
                case ManagedLobbyPlayerEventType.Meta:
                    {
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
        }
    }
}
