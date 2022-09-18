using System;

using Battlegrounds.Game;

namespace Battlegrounds.AI.Lobby;

/// <summary>
/// Class representing the analytical results of a call to <see cref="AIMapAnalyser.Analyze(out Battlegrounds.Gfx.TgaPixel[,], int, int)"/>.
/// </summary>
public class AIMapAnalysis {

    /// <summary>
    /// Enum representing the different strategic values of an enum.
    /// </summary>
    public enum StrategicValueType {

        /// <summary>
        /// Crossroads, T-section or otherwise significant road node.
        /// </summary>
        Crossroads,

        /// <summary>
        /// Fuel point.
        /// </summary>
        Fuel,

        /// <summary>
        /// Munitions point.
        /// </summary>
        Munitions,

        /// <summary>
        /// Victory point.
        /// </summary>
        VictoryPoint,

        /// <summary>
        /// Ordinary resource point.
        /// </summary>
        Resource

    }

    /// <summary>
    /// Record representing a road segment connecting two nodes identified by index.
    /// </summary>
    /// <param name="First">The first road segment.</param>
    /// <param name="Second">The second road segment.</param>
    public record RoadConnection(int First, int Second);

    /// <summary>
    /// Record representing a point of significant value.
    /// </summary>
    /// <param name="Position">The position of strategic value.</param>
    /// <param name="Type">The type of stratgic value.</param>
    /// <param name="Weight">How much should the AI "favour" this point.</param>
    public record StrategicValue(GamePosition Position, StrategicValueType Type, float Weight);

    /// <summary>
    /// Get or initialise road nodes.
    /// </summary>
    public GamePosition[] Nodes { get; init; }

    /// <summary>
    /// Get or initialise strategic points.
    /// </summary>
    public StrategicValue[] StrategicPositions { get; init; }

    /// <summary>
    /// Get or initialise road segments.
    /// </summary>
    public RoadConnection[] Roads { get; init; }

    /// <summary>
    /// Initialise a new empty <see cref="AIMapAnalysis"/> instance.
    /// </summary>
    public AIMapAnalysis() {
        this.Nodes = Array.Empty<GamePosition>();
        this.StrategicPositions = Array.Empty<StrategicValue>();
        this.Roads = Array.Empty<RoadConnection>();
    }

    /// <summary>
    /// Initialise a new <see cref="AIMapAnalysis"/> instance populated with data.
    /// </summary>
    /// <param name="nodes">The road nodes.</param>
    /// <param name="connections">The road connections.</param>
    /// <param name="strategicPositions">The strategic value points.</param>
    public AIMapAnalysis(GamePosition[] nodes,  RoadConnection[] connections, StrategicValue[] strategicPositions) {
        this.Nodes = nodes;
        this.Roads = connections;
        this.StrategicPositions = strategicPositions;
    }

}
