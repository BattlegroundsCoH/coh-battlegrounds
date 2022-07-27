using System;

using Battlegrounds.Game;

namespace Battlegrounds.AI.Lobby;

public class AIMapAnalysis {

    public enum StrategicValueType {
        Crossroads,
        Fuel,
        Munitions,
        VictoryPoint,
        Resource
    }

    public record RoadConnection(int First, int Second);

    public record StrategicValue(GamePosition Position, StrategicValueType Type, float Weight);

    public GamePosition[] Nodes { get; init; }

    public StrategicValue[] StrategicPositions { get; init; }

    public RoadConnection[] Roads { get; init; }

    public AIMapAnalysis() {
        this.Nodes = Array.Empty<GamePosition>();
        this.StrategicPositions = Array.Empty<StrategicValue>();
        this.Roads = Array.Empty<RoadConnection>();
    }

    public AIMapAnalysis(GamePosition[] nodes,  RoadConnection[] connections, StrategicValue[] strategicPositions) {
        this.Nodes = nodes;
        this.Roads = connections;
        this.StrategicPositions = strategicPositions;
    }

}
