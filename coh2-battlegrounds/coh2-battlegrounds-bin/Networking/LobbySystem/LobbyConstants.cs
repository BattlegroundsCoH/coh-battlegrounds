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

    public const byte TID_ALLIES = 0;
    public const byte TID_AXIS = 1;
    public const byte TID_OBS = 2;

    public const byte ROLE_HOST = 0;
    public const byte ROLE_PARTICIPANT = 1;
    public const byte ROLE_OBSERVER = 2;
    public const byte ROLE_AI = 3;

    public const byte STATE_OPEN = 0;
    public const byte STATE_OCCUPIED = 1;
    public const byte STATE_LOCKED = 2;
    public const byte STATE_DISABLED = 3;

}

public enum LobbyState {
    None = 0,
    InLobby = 1,
    Starting = 2,
    Playing = 3
}
