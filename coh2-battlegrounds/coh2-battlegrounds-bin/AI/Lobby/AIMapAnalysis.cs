using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.AI.Lobby;

public class AIMapAnalysis {

    public record RoadConnection(int First, int Second);

    public Vector2[] Nodes { get; init; }

    public RoadConnection[] Roads { get; init; }

    public AIMapAnalysis(Vector2[] nodes, RoadConnection[] connections) {
        this.Nodes = nodes;
        this.Roads = connections;
    }

}
