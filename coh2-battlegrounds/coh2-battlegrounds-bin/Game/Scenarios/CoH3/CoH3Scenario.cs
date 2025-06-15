using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Scripting.Lua.Interpreter;

namespace Battlegrounds.Game.Scenarios.CoH3;

/// <summary>
/// Represents a scenario for Company of Heroes 3. This class cannot be inherited.
/// </summary>
public sealed class CoH3Scenario : IScenario {

    /// <inheritdoc/>
    public string Name { get; set; }

    /// <inheritdoc/>
    public string Description { get; set; }

    /// <inheritdoc/>
    public string RelativeFilename { get; set; }

    /// <inheritdoc/>
    public string SgaName { get; set; }

    /// <inheritdoc/>
    public byte MaxPlayers { get; set; }

    /// <inheritdoc/>
    public ScenarioTheatre Theatre { get; set; }

    /// <inheritdoc/>
    public bool IsWintermap => false;

    /// <inheritdoc/>
    public bool IsDesertMap => Theatre is ScenarioTheatre.AfricanFront;

    /// <inheritdoc/>
    public bool IsVisibleInLobby { get; set; }

    /// <inheritdoc/>
    public bool IsWorkshopMap => false;

    /// <inheritdoc/>
    public IList<string> Gamemodes { get; set; }

    /// <inheritdoc/>
    public bool HasValidInfoOrOptionsFile => false;

    /// <inheritdoc/>
    public PointPosition[] Points { get; set; }

    /// <inheritdoc/>
    public GameSize PlayableSize { get; set; }

    /// <inheritdoc/>
    public GameSize TerrainSize { get; set; }

    /// <inheritdoc/>
    public GameSize MinimapSize { get; set; }

    /// <inheritdoc/>
    public GameCase Game => GameCase.CompanyOfHeroes3;

    public static CoH3Scenario? ReadScenario(string? info, string sga) {

        // Make sure info file is not null
        if (string.IsNullOrEmpty(info)) {
            return null;
        }
        
        // Create lua reader
        LuaState scenarioState = new();
        LuaVM.DoFile(scenarioState, info);

        // Get header info (and if false, bail)
        if (scenarioState._G["HeaderInfo"] is not LuaTable headerInfo) {
            return null;
        }

        // Create scenario with basics
        var scen = new CoH3Scenario {
            // Create basics
            Gamemodes = new List<string>(),
            SgaName = sga,
            RelativeFilename = Path.GetFileNameWithoutExtension(info),
            Name = headerInfo["scenarioname"].Str(),
            Description = headerInfo["ScenarioDescription"].Str(),
            MaxPlayers = (byte)ReadPlayerCount(headerInfo["slots"].As<LuaTable>()),
            Theatre = headerInfo.GetIfExists("audio_environment", out LuaValue audioEnv)
                ? (audioEnv.Equals(new LuaString("desert_africa")) ? ScenarioTheatre.AfricanFront : ScenarioTheatre.ItalianFront)
                : ScenarioTheatre.ItalianFront,
            IsVisibleInLobby = headerInfo["visible_in_lobby"].As<LuaBool>().IsTrue,
            // Read world size
            PlayableSize = ReadSize(headerInfo["mapsize"].As<LuaTable>()),
            // Read world points
            Points = headerInfo["point_positions"].As<LuaTable>().ToArray().Map(x => x.As<LuaTable>()).Map(ReadPoint)
        };

        // Return parsed scenario
        return scen;

    }

    private static int ReadPlayerCount(LuaTable slots)
        => slots.ToArray().Map(x => x.As<LuaTable>()).Map(x => x["status"].As<LuaNumber>().ToInt() == 0).Count(x => x);

    private static GameSize ReadSize(LuaTable? table)
        => table is null ? GameSize.Naught : new(table[1].As<LuaNumber>().ToInt(), table[2].As<LuaNumber>().ToInt());

    private static PointPosition ReadPoint(LuaTable table) {
        double y = table["y"].As<LuaNumber>();
        double x = table["x"].As<LuaNumber>();
        ushort owner = (ushort)table["owner_id"].As<LuaNumber>().ToInt();
        string ebp = table["ebp_name"].Str();
        return new(new(x, y), owner, ebp);
    }

    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, double x, double y) {
        throw new NotImplementedException();
    }

    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, GamePosition minipos) {
        throw new NotImplementedException();
    }

    public GamePosition ToMinimapPosition(double minimapWidth, double minimapHeight, GamePosition worldPos) {
        throw new NotImplementedException();
    }

}
