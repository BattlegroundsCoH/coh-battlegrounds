using System.Windows;

using Battlegrounds.Game;

namespace BattlegroundsApp.Lobby;

internal static class LobbyVisualsHelper {

    internal static Point ToPoint(this GamePosition position) => new(position.X, position.Y);

}
