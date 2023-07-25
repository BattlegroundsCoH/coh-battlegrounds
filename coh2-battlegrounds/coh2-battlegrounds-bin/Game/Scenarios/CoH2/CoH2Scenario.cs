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

namespace Battlegrounds.Game.Scenarios.CoH2;

/// <summary>
/// Represents a scenario. This class cannot be inherited.
/// </summary>
public sealed class CoH2Scenario : IScenario {

    /// <summary>
    /// Name of an invalid sga
    /// </summary>
    public const string INVALID_SGA = "INVALID SGA";

    /// <summary>
    /// The text-name for the <see cref="CoH2Scenario"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description text for the <see cref="CoH2Scenario"/>.
    /// </summary>
    public string Description { get; set; }

    /// <inheritdoc/>
    public string RelativeFilename { get; set; }

    /// <summary>
    /// Name of the sga file containing this <see cref="CoH2Scenario"/>.
    /// </summary>
    public string SgaName { get; set; }

    /// <inheritdoc/>
    public byte MaxPlayers { get; set; }

    /// <inheritdoc/>
    public ScenarioTheatre Theatre { get; set; }

    /// <summary>
    /// Can this <see cref="CoH2Scenario"/> be considered a winter map.
    /// </summary>
    public bool IsWintermap { get; set; }

    /// <inheritdoc/>
    public bool IsVisibleInLobby { get; set; }

    /// <summary>
    /// Get if the <see cref="CoH2Scenario"/> is a workshop map.
    /// </summary>
    public bool IsWorkshopMap => SgaName.ToLower() is not ("mpscenarios" or "mpxp1scenarios");

    /// <inheritdoc/>
    public IList<string> Gamemodes { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool HasValidInfoOrOptionsFile { get; private set; }

    /// <inheritdoc/>
    public PointPosition[] Points { get; set; }

    /// <inheritdoc/>
    public GameSize PlayableSize { get; set; }

    /// <inheritdoc/>
    public GameSize TerrainSize { get; set; }

    /// <inheritdoc/>
    public GameSize MinimapSize { get; set; }

    /// <inheritdoc/>
    public GameCase Game => GameCase.CompanyOfHeroes2;

    /// <summary>
    /// Create a new, unitialised scenario
    /// </summary>
    public CoH2Scenario() {
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
    }

    /// <summary>
    /// New <see cref="CoH2Scenario"/> instance with data from either an infor or options file.
    /// </summary>
    /// <param name="laofile">The path to the lao file containing terrain information.</param>
    /// <param name="infofile">The path to the info file.</param>
    /// <param name="optionsfile">The path to the options file</param>
    /// <param name="sganame">The name of the sga file</param>
    /// <exception cref="ArgumentNullException"/>
    public static CoH2Scenario? ReadScenario(string? laofile, string? infofile, string? optionsfile, string sganame) {

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
        var scen = new CoH2Scenario {
            // Create basics
            Gamemodes = new List<string>(),
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
            Trace.WriteLine($"Error while reading scenario information: {e}", nameof(CoH2Scenario));
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, GamePosition minipos)
        => FromMinimapPosition(minimapWidth, minimapHeight, minipos.X, minipos.Y);

    /// <inheritdoc/>
    public override string ToString() => Name;

}
