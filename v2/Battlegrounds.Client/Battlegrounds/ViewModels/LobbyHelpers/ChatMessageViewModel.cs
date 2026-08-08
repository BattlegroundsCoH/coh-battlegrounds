using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public enum SystemMessageType {
    None = 0,
    Info,
    Warning,
    Error
}

/// <summary>
/// The role a piece of a chat line plays, which the view maps onto a themed brush.
/// </summary>
/// <remarks>
/// A tone rather than a colour: these used to be SolidColorBrushes built from raw hex here in
/// the view-model, which put literal colours outside the palette and made the chat the one part
/// of the app a recolour would miss. <see cref="Converters.ChatToneBrushConverter"/> resolves
/// them against the theme instead.
/// </remarks>
public enum ChatTone {
    ChannelAll,
    ChannelTeam,
    Self,
    Ally,
    Enemy,
    SystemInfo,
    SystemWarning,
    SystemError
}

public sealed record ChatMessageViewModel(DateTime Timestamp, ChatChannel Channel, bool IsSelf, bool IsAllied, string Sender, string Message, SystemMessageType SystemMessageKind = SystemMessageType.None) {

    public string FormattedTimestamp => $"{Timestamp:HH:mm:ss}";

    /// <summary>
    /// The channel tag, carrying its own trailing separator.
    /// </summary>
    /// <remarks>
    /// The separator belongs to the tag because a system message has no channel: with a
    /// space on each side of an empty tag, "[System]" ended up double-spaced from the time.
    /// </remarks>
    public string FormattedChannel => SystemMessageKind != SystemMessageType.None ? "" : Channel switch {
        ChatChannel.All => "[All] ",
        ChatChannel.Team => "[Team] ",
        _ => "[Unknown] "
    };

    /// <summary>
    /// The sender tag. A player's name is followed by a colon because they are speaking;
    /// the system is labelling, so <c>[System]</c> stands on its own.
    /// </summary>
    public string FormattedSender => SystemMessageKind != SystemMessageType.None ? "[System]" : $"{Sender}:";

    public ChatTone ChannelTone => Channel switch {
        ChatChannel.Team => ChatTone.ChannelTeam,
        _ => ChatTone.ChannelAll
    };

    public ChatTone SenderTone => SystemMessageKind switch {
        SystemMessageType.Info => ChatTone.SystemInfo,
        SystemMessageType.Warning => ChatTone.SystemWarning,
        SystemMessageType.Error => ChatTone.SystemError,
        _ => IsSelf ? ChatTone.Self : IsAllied ? ChatTone.Ally : ChatTone.Enemy
    };

}
