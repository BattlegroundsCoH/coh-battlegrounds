namespace Battlegrounds.Models.Lobbies;

public enum LobbySettingType {
    Boolean,
    Integer,
    Selection
}

public sealed record LobbySettingOption(string Name, string Value);

public sealed class LobbySetting {

    public const string SETTING_GAMEMODE = "gamemode";
    public const string SETTING_GAMEMODE_OPTION = "gamemode_option";

    public required string Name { get; init; }

    public int Priority { get; init; }

    public int Value { get; set; }

    public required LobbySettingType Type { get; init; }

    public LobbySettingOption[]? Options { get; set; }

    public int MinValue { get; set; } = int.MinValue;

    public int MaxValue { get; set; } = int.MaxValue;

    public int Step { get; set; } = 1;

}
