namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed record PickableChatChannel(string ChannelName) {
    public string DisplayName => ChannelName switch {
        "all" => "All Players",
        "team" => "Team",
        _ => throw new ArgumentOutOfRangeException(nameof(ChannelName), ChannelName, "Unknown chat channel")
    };
}
