using System.Collections.Generic;
using System.Text.Json.Serialization;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Scenarios;

/// <summary>
/// Represents a scenario. This interface defines the contract for a scenario.
/// </summary>
public interface IScenario {

    /// <summary>
    /// The text-name for the <see cref="IScenario"/>.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// The description text for the <see cref="IScenario"/>.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// The relative filename to the "mp" scenarios folder.
    /// </summary>
    string RelativeFilename { get; set; }

    /// <summary>
    /// Name of the sga file containing this <see cref="IScenario"/>.
    /// </summary>
    string SgaName { get; set; }

    /// <summary>
    /// The max amount of players who can play on this map.
    /// </summary>
    byte MaxPlayers { get; set; }

    /// <summary>
    /// The <see cref="ScenarioTheatre"/> this map takes place in.
    /// </summary>
    ScenarioTheatre Theatre { get; set; }

    /// <summary>
    /// Can this <see cref="IScenario"/> be considered a winter map.
    /// </summary>
    bool IsWintermap { get; }

    /// <summary>
    /// Get if the given scenario is visible in the lobby.
    /// </summary>
    bool IsVisibleInLobby { get; set; }

    /// <summary>
    /// Get if the <see cref="IScenario"/> is a workshop map.
    /// </summary>
    bool IsWorkshopMap { get; }

    /// <summary>
    /// The winconditions designed for this <see cref="IScenario"/>. Empty list means all winconditions can be used.
    /// </summary>
    IList<string> Gamemodes { get; set; }

    /// <summary>
    /// Get if the scenario has a valid info or options file.
    /// </summary>
    [JsonIgnore]
    bool HasValidInfoOrOptionsFile { get; }

    /// <summary>
    /// Get the point position information.
    /// </summary>
    PointPosition[] Points { get; set; }

    /// <summary>
    /// Get the width and length of the playable world.
    /// </summary>
    GameSize PlayableSize { get; set; }

    /// <summary>
    /// Get or set the width and length of the terrain.
    /// </summary>
    GameSize TerrainSize { get; set; }

    /// <summary>
    /// Get or set the width and length of the terrain.
    /// </summary>
    GameSize MinimapSize { get; set; }

    /// <summary>
    /// Get the game this scenario is for.
    /// </summary>
    GameCase Game { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minimapWidth"></param>
    /// <param name="minimapHeight"></param>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    GamePosition ToMinimapPosition(double minimapWidth, double minimapHeight, GamePosition worldPos);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minimapWidth"></param>
    /// <param name="minimapHeight"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, double x, double y);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minimapWidth"></param>
    /// <param name="minimapHeight"></param>
    /// <param name="minipos"></param>
    /// <returns></returns>
    GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, GamePosition minipos);

}
