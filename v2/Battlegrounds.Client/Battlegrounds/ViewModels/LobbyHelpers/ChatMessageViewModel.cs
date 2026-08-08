using System.Windows.Media;

using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public enum SystemMessageType {
    None = 0,
    Info,
    Warning,
    Error
}

public sealed record ChatMessageViewModel(DateTime Timestamp, ChatChannel Channel, bool IsSelf, bool IsAllied, string Sender, string Message, SystemMessageType SystemMessageKind = SystemMessageType.None) {
    public string FormattedTimestamp => $"{Timestamp:HH:mm:ss} -"; // Format the timestamp as needed
    public string FormattedChannel => SystemMessageKind != SystemMessageType.None ? "" : Channel switch {
        ChatChannel.All => "[All]",
        ChatChannel.Team => "[Team]",
        _ => "[Unknown]"
    };
    public string FormattedSender => $"{(SystemMessageKind != SystemMessageType.None ? "[System]" : Sender)}:"; // Format sender for system messages
    public SolidColorBrush ChannelColour => Channel switch {
        ChatChannel.All => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0x00)), // Yellow for All
        ChatChannel.Team => new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF)),
        _ => new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)) // Red for unknown channels
    };
    public SolidColorBrush SenderColour {
        get {
            if (SystemMessageKind is not SystemMessageType.None) {
                return SystemMessageKind switch {
                    SystemMessageType.Info => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)), // White for info messages
                    SystemMessageType.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)), // Orange for warning messages
                    SystemMessageType.Error => new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)), // Red for error messages
                    _ => new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)) // Red for unknown system messages
                };
            } else if (IsSelf) {
                return new SolidColorBrush(Color.FromRgb(0x45, 0xA7, 0xe5)); // Blue for self
            } else if (IsAllied) {
                return new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)); // Orange for allied players
            } else {
                return new SolidColorBrush(Color.FromRgb(0xE5, 0x45, 0x45)); // Red for other players
            }
        }
    }
}
