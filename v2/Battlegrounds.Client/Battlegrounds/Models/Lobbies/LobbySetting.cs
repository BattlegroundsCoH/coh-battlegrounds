namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents the type of a lobby setting.
/// </summary>
public enum LobbySettingType {

    /// <summary>
    /// Represents a boolean setting type.
    /// </summary>
    Boolean,

    /// <summary>
    /// Represents an integer setting type.
    /// </summary>
    Integer,

    /// <summary>
    /// Represents a selection setting type, where the user can choose from a predefined set of options.
    /// </summary>
    Selection

}

/// <summary>
/// Represents an option for a lobby setting, consisting of a name and a value.
/// </summary>
/// <param name="Name">The name of the option.</param>
/// <param name="Value">The value of the option.</param>
public sealed record LobbySettingOption(string Name, string Value);

/// <summary>
/// Represents a setting in a lobby, including its name, type, value, and other properties.
/// </summary>
public sealed class LobbySetting {

    /// <summary>
    /// Represents the name of the game mode setting in a lobby.
    /// </summary>
    public const string SETTING_GAMEMODE = "gamemode";

    /// <summary>
    /// Represents the name of the victory points setting in a lobby.
    /// </summary>
    public const string SETTING_VICTORY_POINTS = "victory_points";

    /// <summary>
    /// Represents the name of the supply system setting in a lobby.
    /// </summary>
    public const string SETTING_SUPPLY_SYSTEM = "supply_system";

    /// <summary>
    /// Represents the name of the domination game mode option setting in a lobby.
    /// </summary>
    public const string SETTING_OPTION_DOMINATION = "domination";

    /// <summary>
    /// Represents the name of the victory points game mode option setting in a lobby.
    /// </summary>
    public const string SETTING_OPTION_VICTORY_POINTS = "victory_points";

    /// <summary>
    /// Represents the default settings for a lobby, including the game mode and victory points settings. These settings are used to initialize a lobby with default values.
    /// </summary>
    public static readonly LobbySetting[] DefaultSettings = [
        new LobbySetting { Name = SETTING_GAMEMODE, Type = LobbySettingType.Selection, Options = [new("Domination", SETTING_OPTION_DOMINATION), new("Victory Points", SETTING_OPTION_VICTORY_POINTS)] },
        new LobbySetting { Name = SETTING_VICTORY_POINTS, Type = LobbySettingType.Selection, Options = [new("100 Points", "100"), new("500 Points", "500"), new("750 Points", "750"), new("1000 Points", "1000")], IsVisible = true },
        new LobbySetting { Name = SETTING_SUPPLY_SYSTEM, Type = LobbySettingType.Selection, Options = [new("False", "0"), new("True", "1")], IsVisible = false },
    ];

    /// <summary>
    /// Represents the name of the game mode option setting in a lobby.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Represents the priority of the lobby setting, which can be used to determine the order in which settings are displayed or processed.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Represents the value of the lobby setting, which can be of different types (boolean, integer, or selection) depending on the setting type.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Represents the type of the lobby setting, which determines how the value is interpreted (boolean, integer, or selection).
    /// </summary>
    public required LobbySettingType Type { get; init; }

    /// <summary>
    /// Represents the options available for a selection-type lobby setting. This property is only applicable when the setting type is 'Selection'. It can be null if the setting type is not 'Selection'.
    /// </summary>
    public LobbySettingOption[]? Options { get; set; }

    /// <summary>
    /// Represents the minimum value for an integer-type lobby setting. This property is only applicable when the setting type is 'Integer'. It can be set to int.MinValue by default, indicating that there is no specific minimum value constraint.
    /// </summary>
    public int MinValue { get; set; } = int.MinValue;

    /// <summary>
    /// Represents the maximum value for an integer-type lobby setting. This property is only applicable when the setting type is 'Integer'. It can be set to int.MaxValue by default, indicating that there is no specific maximum value constraint.
    /// </summary>
    public int MaxValue { get; set; } = int.MaxValue;

    /// <summary>
    /// Represents the step value for an integer-type lobby setting. This property is only applicable when the setting type is 'Integer'. It can be set to 1 by default, indicating that the value can be incremented or decremented by 1.
    /// </summary>
    public int Step { get; set; } = 1;

    /// <summary>
    /// Indicates whether the lobby setting is visible to users. This property can be used to control the visibility of the setting in the user interface. 
    /// It is set to true by default, meaning that the setting is visible unless explicitly set to false.
    /// </summary>
    public bool IsVisible { get; set; } = true;

}
