using System.Windows.Media;

using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed record ChatMessageViewModel(DateTime Timestamp, ChatChannel Channel, bool IsSelf, bool IsAllied, string Sender, string Message, bool IsSystemMessage = false) {
    public string FormattedTimestamp => $"{Timestamp:HH:mm:ss} -"; // Format the timestamp as needed
    public string FormattedChannel => IsSystemMessage ? "" : Channel switch {
        ChatChannel.All => "[All]",
        ChatChannel.Team => "[Team]",
        _ => "[Unknown]"
    };
    public string FormattedSender => $"{(IsSystemMessage ? "[System]" : Sender)}:"; // Format sender for system messages
    public SolidColorBrush ChannelColour => Channel switch {
        ChatChannel.All => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0x00)), // Yellow for All
        ChatChannel.Team => new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF)),
        _ => new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)) // Red for unknown channels
    };
    public SolidColorBrush SenderColour {
        get {
            if (IsSystemMessage) {
                return new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)); // Red for system messages
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
