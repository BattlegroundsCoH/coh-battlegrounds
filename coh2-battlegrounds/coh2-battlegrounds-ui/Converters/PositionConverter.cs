using System.Windows;

using Battlegrounds.Game;

namespace Battlegrounds.UI.Converters;

public static class PositionConverter {

    public static Point ToPoint(this GamePosition position) => new(position.X, position.Y);

}
