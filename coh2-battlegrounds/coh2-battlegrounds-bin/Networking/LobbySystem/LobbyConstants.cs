namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Constant utility class for constant values related to the lobby
/// </summary>
public static class LobbyConstants {

    public const string SETTING_MAP = "selected_map";
    public const string SETTING_GAMEMODE = "selected_wc";
    public const string SETTING_GAMEMODEOPTION = "selected_wco";
    public const string SETTING_WEATHER = "selected_daynight";
    public const string SETTING_LOGISTICS = "selected_supply";
    public const string SETTING_MODPACK = "selected_tuning";

}

public enum LobbyState {
    None = 0,
    InLobby = 1,
    Starting = 2,
    Playing = 3
}
