using System.Globalization;
using System.Windows.Data;

using Battlegrounds.Controls;
using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.Converters;

/// <summary>
/// Tints a <see cref="StatusBadge"/> by lobby connection state.
/// </summary>
public sealed class ConnectionStateToneConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch {
        LobbyConnectionState.Connected => BadgeTone.Success,
        LobbyConnectionState.Connecting or LobbyConnectionState.Reconnecting => BadgeTone.Caution,
        LobbyConnectionState.Disconnected or LobbyConnectionState.Disposed => BadgeTone.Danger,
        _ => BadgeTone.Neutral
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

}

/// <summary>
/// Explains a lobby connection state in a sentence, for the badge's tooltip.
/// </summary>
/// <remarks>
/// The badge itself is one word, which says what the state is but not what it refers to —
/// a green chip on its own reads as "something is fine" without saying what.
/// </remarks>
public sealed class ConnectionStateDescriptionConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch {
        LobbyConnectionState.Connected => "Connected to the lobby server.",
        LobbyConnectionState.Connecting => "Connecting to the lobby server…",
        LobbyConnectionState.Reconnecting => "Connection lost — trying to reconnect.",
        LobbyConnectionState.Disconnected => "Disconnected from the lobby server.",
        LobbyConnectionState.Disposed => "This lobby has been closed.",
        _ => "Connection to the lobby server."
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

}
