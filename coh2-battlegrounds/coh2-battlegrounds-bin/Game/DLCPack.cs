using Battlegrounds.Steam;

namespace Battlegrounds.Game;

/// <summary>
/// Represents a DLC tied to Company of Heroes 2 or Company of Heroes 3.
/// </summary>
public sealed class DLCPack {

    /// <summary>
    /// The type of DLC a <see cref="DLCPack"/> may be of
    /// </summary>
    public enum DLCType {

        /// <summary>
        /// Faction that unlocks and army
        /// </summary>
        Faction,

        /// <summary>
        /// Vehicle Skin
        /// </summary>
        Skin,

        /// <summary>
        /// Ingame commander
        /// </summary>
        Commander,

        /// <summary>
        /// A base game (ie. CoH2 with multiplayer or CoH3 with multiplayer)
        /// </summary>
        Game,

        /// <summary>
        /// A battlegroup DLC (same as CoH2 commanders)
        /// </summary>
        Battlegroup = Commander

    }

    /// <summary>
    /// The name of the DLC pack
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Unique app ID assigned to this pack
    /// </summary>
    public AppId SteamAppID { get; }

    /// <summary>
    /// The type of DLC
    /// </summary>
    public DLCType Type { get; }

    /// <summary>
    /// Get the game associated with this DLC.
    /// </summary>
    public GameCase Game => GameCases.FromAppId(SteamAppID);

    private DLCPack(string name, AppId steamid, DLCType dlctype) {
        this.Name = name;
        this.SteamAppID = steamid;
        this.Type = dlctype;
    }

    /// <inheritdoc/>
    public override string ToString() => $"DLC: '{this.Name}' (Steam App ID: {this.SteamAppID.Identifier})";

    #region Static Region 

    private static readonly DLCPack basegame;
    private static readonly DLCPack wfa_aef;
    private static readonly DLCPack wfa_okw;
    private static readonly DLCPack ukf;

    private static readonly DLCPack coh3;

    /// <summary>
    /// The base CoH2 game (with multiplayer access)
    /// </summary>
    public static DLCPack CoH2Base => basegame;

    /// <summary>
    /// The base CoH3 game
    /// </summary>
    public static DLCPack CoH3Base => coh3;

    /// <summary>
    /// The Western Front Armies (USA)
    /// </summary>
    public static DLCPack WesternFrontArmiesUSA => wfa_aef;

    /// <summary>
    /// The Western Front Armies (OKW)
    /// </summary>
    public static DLCPack WesternFrontArmiesOKW => wfa_okw;

    /// <summary>
    /// The British Forces
    /// </summary>
    public static DLCPack UKF => ukf;

    static DLCPack() {
        basegame = new DLCPack("Company of Heroes 2", AppId.Game(GameCases.CoH2AppId), DLCType.Faction);
        wfa_aef = new DLCPack("Western Front Armies - US Forces", AppId.DLC(39986, GameCases.CoH2AppId), DLCType.Faction);
        wfa_okw = new DLCPack("Western Front Armies - Oberkommando West", AppId.DLC(39988, GameCases.CoH2AppId), DLCType.Faction);
        ukf = new DLCPack("British Forces", AppId.DLC(76411, GameCases.CoH2AppId), DLCType.Faction);
        coh3 = new DLCPack("Company of Heroes 3", AppId.Game(GameCases.CoH3AppId), DLCType.Game);
    }

    #endregion

}
