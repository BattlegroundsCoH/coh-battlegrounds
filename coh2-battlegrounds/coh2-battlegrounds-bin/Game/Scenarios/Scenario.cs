using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Scripting.Lua.Interpreter;
using Battlegrounds.Util;

namespace Battlegrounds.Game.Scenarios;

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
    public bool IsWorkshopMap => SgaName.ToLower() is not ("mpscenarios" or "mpxp1scenarios");

    /// <summary>
    /// The <see cref="Wincondition"/> instances designed for this <see cref="Scenario"/>. Empty list means all <see cref="Wincondition"/> instances can be used.
    /// </summary>
    public List<string> Gamemodes { get; set; }

    /// <summary>
    /// Get if the scenario has a valid info or options file.
    /// </summary>
    [JsonIgnore]
    public bool HasValidInfoOrOptionsFile { get; private set; }

    /// <summary>
    /// Get the point position information.
    /// </summary>
    public PointPosition[] Points { get; set; }

    /// <summary>
    /// Get the width and length of the playable world.
    /// </summary>
    public GameSize PlayableSize { get; set; }

    /// <summary>
    /// Get or set the width and length of the terrain.
    /// </summary>
    public GameSize TerrainSize { get; set; }

    /// <summary>
    /// Get or set the width and length of the terrain.
    /// </summary>
    public GameSize MinimapSize { get; set; }

    /// <summary>
    /// Get or set the game this scenario is for.
    /// </summary>
    public GameCase Game { get; set; }

    public Scenario() {
        SgaName = INVALID_SGA;
        Gamemodes = new List<string>();
        HasValidInfoOrOptionsFile = true; // Under the assumption it's being set
        Name = "Unknwon";
        Description = "Undefined";
        RelativeFilename = "INVALID_FILENAME";
        Points = Array.Empty<PointPosition>();
        PlayableSize = GameSize.Naught;
        TerrainSize = GameSize.Naught;
        MinimapSize = GameSize.Naught;
        Game = GameCase.CompanyOfHeroes2;
    }

    /// <summary>
    /// New <see cref="Scenario"/> instance with data from either an infor or options file.
    /// </summary>
    /// <param name="laofile">The path to the lao file containing terrain information.</param>
    /// <param name="infofile">The path to the info file.</param>
    /// <param name="optionsfile">The path to the options file</param>
    /// <exception cref="ArgumentNullException"/>
    public static Scenario? ReadScenario(string? laofile, string? infofile, string? optionsfile, string sganame) {

        // Make sure lao file is not null
        if (string.IsNullOrEmpty(laofile)) {
            return null;
        }

        // Make sure info file is not null
        if (string.IsNullOrEmpty(infofile)) {
            return null;
        }

        // Make sure options file is not null
        if (string.IsNullOrEmpty(optionsfile)) {
            return null;
        }

        // Create lua reader
        LuaState scenarioState = new();
        LuaVM.DoFile(scenarioState, infofile);
        LuaVM.DoFile(scenarioState, optionsfile);

        // Get header info (and if false, bail)
        if (scenarioState._G["HeaderInfo"] is not LuaTable headerInfo) {
            return null;
        }

        // Create scenario with basics
        var scen = new Scenario {
            // Create basics
            Gamemodes = new(),
            SgaName = sganame,
            RelativeFilename = Path.GetFileNameWithoutExtension(infofile),
            Name = headerInfo["scenarioname"].Str(),
            Description = headerInfo["scenariodescription"].Str(),
            MaxPlayers = (byte)(headerInfo["maxplayers"] as LuaNumber ?? new LuaNumber(0))
        };

        // Read battlefront
        int battlefront = headerInfo["scenario_battlefront"] is LuaNumber bf ? (int)bf : 2;
        scen.Theatre = battlefront == 2 ? ScenarioTheatre.EasternFront : battlefront == 5 ? ScenarioTheatre.WesternFront : ScenarioTheatre.SharedFront;

        // Get the skins table (apperantly both are accepted...)
        if (headerInfo["default_skins"] is not LuaTable skins) {
            skins = headerInfo["default_skin"] as LuaTable ?? new();
        }

        // Read winter information
        scen.IsWintermap = skins?.Contains("winter") ?? false;

        // Read world size
        scen.PlayableSize = ReadSize(headerInfo["mapsize"].As<LuaTable>());

        // Read world points
        scen.Points = headerInfo["point_positions"].As<LuaTable>().ToArray().Map(x => x.As<LuaTable>()).Map(ReadPoint);

        // Read is visible
        scen.IsVisibleInLobby = (scenarioState._G["visible_in_lobby"] as LuaBool)?.IsTrue ?? true;

        // Read minimap size
        if (scenarioState._G["minimap_size"] is LuaTable minimap) {
            scen.MinimapSize = ReadSize(minimap);
        } else {
            scen.MinimapSize = new(768, 768);
        }

        // Mark as valid
        scen.HasValidInfoOrOptionsFile = true;

        try {

            // Try open
            using var fs = File.OpenRead(laofile);
            using var br = new BinaryReader(fs);

            // Skip first 12 bytes
            br.Skip(12);

            // Read with
            int terrainWidth = br.ReadInt32();
            int terrainLength = br.ReadInt32();

            // Store
            scen.TerrainSize = new(terrainWidth, terrainLength);

        } catch (Exception e) {
            Trace.WriteLine($"Error while reading scenario information: {e}", nameof(Scenario));
        }

        // Return
        return scen;

    }

    private static GameSize ReadSize(LuaTable? table)
        => table is null ? GameSize.Naught : new(table[1].As<LuaNumber>().ToInt(), table[2].As<LuaNumber>().ToInt());

    private static PointPosition ReadPoint(LuaTable table) {
        double y = table["y"].As<LuaNumber>();
        double x = table["x"].As<LuaNumber>();
        ushort owner = (ushort)table["owner_id"].As<LuaNumber>().ToInt();
        string ebp = table["ebp_name"].Str();
        return new(new(x, y), owner, ebp);
    }
    public GamePosition ToMinimapPosition(double minimapWidth, double minimapHeight, GamePosition worldPos) {

        // Bring into standard coordinate system
        double x = worldPos.X + PlayableSize.Width * .5;
        double y = worldPos.Y + PlayableSize.Length * .5;

        // Calculate u,v coords
        double u = x / PlayableSize.Width;
        double v = y / PlayableSize.Length;

        // Return position
        return new(u * minimapWidth, v * minimapHeight);

    }

    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, double x, double y) {

        // Calculate u,v coords
        double u = x / minimapWidth;
        double v = y / minimapHeight;

        // Get into world coords
        double _x = u * PlayableSize.Width;
        double _y = v * PlayableSize.Length;

        // Return position in CoH2 world coordinates
        return new(_x - PlayableSize.Width * .5, _y - PlayableSize.Length * .5);

    }

    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, GamePosition minipos)
        => FromMinimapPosition(minimapWidth, minimapHeight, minipos.X, minipos.Y);

    /// <inheritdoc/>
    public override string ToString() => Name;

}
