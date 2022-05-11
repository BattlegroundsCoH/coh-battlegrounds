using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Lua;

namespace Battlegrounds.Game.Database;

/// <summary>
/// The theatre of war a scenario is taking place in.
/// </summary>
public enum ScenarioTheatre {

    /// <summary>
    /// Axis vs Soviets
    /// </summary>
    EasternFront,

    /// <summary>
    /// Axis vs UKF & USF
    /// </summary>
    WesternFront,

    /// <summary>
    /// Axis vs Allies (Germany)
    /// </summary>
    SharedFront,

}

/// <summary>
/// Represents a scenario. Implements <see cref="IJsonObject"/>. This class cannot be inherited.
/// </summary>
public sealed class Scenario {

    public const string INVALID_SGA = "INVALID SGA";

    /// <summary>
    /// 
    /// </summary>
    public readonly struct PointPosition {

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public readonly GamePosition Position;

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public readonly ushort Owner;

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public readonly string EntityBlueprint;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Owner"></param>
        /// <param name="EntityBlueprint"></param>
        [JsonConstructor]
        public PointPosition(GamePosition Position, ushort Owner, string EntityBlueprint) {
            this.Position = Position;
            this.Owner = Owner;
            this.EntityBlueprint = EntityBlueprint;
        }
    }

    /// <summary>
    /// The text-name for the <see cref="Scenario"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description text for the <see cref="Scenario"/>.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The relative filename to the "mp" scenarios folder.
    /// </summary>
    public string RelativeFilename { get; set; }

    /// <summary>
    /// Name of the sga file containing this <see cref="Scenario"/>.
    /// </summary>
    public string SgaName { get; set; }

    /// <summary>
    /// The max amount of players who can play on this map.
    /// </summary>
    public byte MaxPlayers { get; set; }

    /// <summary>
    /// The <see cref="ScenarioTheatre"/> this map takes place in.
    /// </summary>
    public ScenarioTheatre Theatre { get; set; }

    /// <summary>
    /// Can this <see cref="Scenario"/> be considered a winter map.
    /// </summary>
    public bool IsWintermap { get; set; }

    /// <summary>
    /// Get if the given scenario is visible in the lobby.
    /// </summary>
    public bool IsVisibleInLobby { get; set; }

    /// <summary>
    /// Get if the <see cref="Scenario"/> is a workshop map.
    /// </summary>
    public bool IsWorkshopMap => !(this.SgaName.ToLowerInvariant() is "mpscenarios" or "mpxp1scenarios");

    /// <summary>
    /// The <see cref="Wincondition"/> instances designed for this <see cref="Scenario"/>. Empty list means all <see cref="Wincondition"/> instances can be used.
    /// </summary>
    public List<string> Gamemodes { get; set; }

    /// <summary>
    /// Get if the scenario has a valid info or options file.
    /// </summary>
    [JsonIgnore]
    public bool HasValidInfoOrOptionsFile { get; }

    /// <summary>
    /// Get the point position information.
    /// </summary>
    public PointPosition[] Points { get; set; }

    /// <summary>
    /// Get the width and length of the world.
    /// </summary>
    public GamePosition WorldSize { get; set; }

    /// <summary>
    /// Get the width and height of the minimap
    /// </summary>
    public GamePosition MinimapSize { get; set; }

    public string ToJsonReference() => this.RelativeFilename;

    public Scenario() {
        this.SgaName = INVALID_SGA;
        this.Gamemodes = new List<string>();
        this.HasValidInfoOrOptionsFile = true; // Under the assumption it's being set
        this.Name = "Unknwon";
        this.Description = "Undefined";
        this.RelativeFilename = "INVALID_FILENAME";
        this.Points = Array.Empty<PointPosition>();
        this.WorldSize = GamePosition.Naught;
        this.MinimapSize = GamePosition.Naught;
    }

    /// <summary>
    /// New <see cref="Scenario"/> instance with data from either an infor or options file.
    /// </summary>
    /// <param name="infofile">The path to the info file.</param>
    /// <param name="optionsfile">The path to the options file</param>
    /// <exception cref="ArgumentNullException"/>
    public Scenario(string infofile, string optionsfile) {

        // Make sure infofile is not null
        if (infofile is null) {
            throw new ArgumentNullException(nameof(infofile), "Info filepath cannot be null");
        }

        // Make sure optionsfile is not null
        if (optionsfile is null) {
            throw new ArgumentNullException(nameof(optionsfile), "Options filepath cannot be null");
        }

        // Create basics
        this.Gamemodes = new();
        this.SgaName = string.Empty;
        this.RelativeFilename = Path.GetFileNameWithoutExtension(infofile);

        // Create lua reader
        LuaState scenarioState = new();
        LuaVM.DoFile(scenarioState, infofile);
        LuaVM.DoFile(scenarioState, optionsfile);

        // Get header info (and if false, bail)
        if (scenarioState._G["HeaderInfo"] is not LuaTable headerInfo) {
            throw new Exception("Invalid scenario header 'HeaderInfo' not found!");
        }

        // Read header
        this.Name = headerInfo["scenarioname"].Str();
        this.Description = headerInfo["scenariodescription"].Str();
        this.MaxPlayers = (byte)(headerInfo["maxplayers"] as LuaNumber ?? new LuaNumber(0));

        // Read battlefront
        int battlefront = headerInfo["scenario_battlefront"] is LuaNumber bf ? (int)bf : 2;
        this.Theatre = battlefront == 2 ? ScenarioTheatre.EasternFront : battlefront == 5 ? ScenarioTheatre.WesternFront : ScenarioTheatre.SharedFront;

        // Get the skins table (apperantly both are accepted...)
        if (headerInfo["default_skins"] is not LuaTable skins) {
            skins = headerInfo["default_skin"] as LuaTable ?? new();
        }

        // Read winter information
        this.IsWintermap = skins?.Contains("winter") ?? false;

        // Read world size
        this.WorldSize = ReadSize(headerInfo["mapsize"].As<LuaTable>());

        // Read world points
        this.Points = headerInfo["point_positions"].As<LuaTable>().ToArray().Map(x => (this.WorldSize, x.As<LuaTable>())).Map(ReadPoint);

        // Read is visible
        this.IsVisibleInLobby = (scenarioState._G["visible_in_lobby"] as LuaBool)?.IsTrue ?? true;

        // Read minimap size
        if (scenarioState._G["minimap_size"] is LuaTable minimap) {
            this.MinimapSize = ReadSize(minimap);
        } else {
            this.MinimapSize = new(768, 768);
        }

        // Mark as valid
        this.HasValidInfoOrOptionsFile = true;

    }

    private static GamePosition ReadSize(LuaTable? table)
        => table is null ? GamePosition.Naught : new(table[1].As<LuaNumber>().ToInt(), table[2].As<LuaNumber>().ToInt());

    private static PointPosition ReadPoint((GamePosition ws, LuaTable table) _) {
        double y = _.table["y"].As<LuaNumber>();
        double x = _.table["x"].As<LuaNumber>();
        ushort owner = (ushort)_.table["owner_id"].As<LuaNumber>().ToInt();
        string ebp = _.table["ebp_name"].Str();
        return new(GamePosition.WorldToScreenCoordinate(new(x, y), (int)_.ws.X, (int)_.ws.Y), owner, ebp);
    }

    public override string ToString() => this.Name;

}

