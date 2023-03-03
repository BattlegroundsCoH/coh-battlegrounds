using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

using Battlegrounds.ErrorHandling;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Gfx;

namespace Battlegrounds.AI.Lobby;

public class AIMapAnalyser {

    public class Node {
        public int X { get; }
        public int Y { get; }
        public TgaPixel Pixel { get; }
        public Node(int x, int y, TgaPixel pixel) {
            this.X = x;
            this.Y = y;
            this.Pixel = pixel;
        }
        public override bool Equals(object? obj) {
            if (obj is Node n) {
                return n.X == this.X && n.Y == this.Y;
            } else if (obj is GamePosition gp) {
                return gp.X == this.X && gp.Y == this.Y;
            }
            return false;
        }
        public override int GetHashCode() {
            return HashCode.Combine(this.X, this.Y);
        }
        public Node? Parent { get; set; }
    }

    public record Subdivision(int Left, int Right, int Top, int Bottom, List<Node> Nodes);

    public record Edge(Node A, Node B) {
        public bool SameEdge(Edge other) {
            return (other.A.Equals(this.A) && other.B.Equals(this.B))
                || (other.A.Equals(this.B) && other.B.Equals(this.A)); 
        }
    }

    private static readonly TgaPixel RoadPixelColour = new TgaPixel(142, 142, 124);

    private readonly Scenario m_scenario;
    private readonly List<Edge> m_edges;
    private readonly List<Node> m_nodes;
    private Subdivision[,]? m_grid;
    private TgaImage? m_tga;

    public List<Node> Nodes => this.m_nodes;

    public List<Edge> Edges => this.m_edges;

    public AIMapAnalyser(Scenario scenario) {
        this.m_scenario = scenario;
        this.m_edges = new();
        this.m_nodes = new();
    }

    public AIMapAnalysis? Analyze(out TgaPixel[,] pixelmap, int subDivisions, int searchRadius) {

        // Grab file
        var mapFile = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, $"map_icons\\{this.m_scenario.RelativeFilename}_map.tga");

        // Zu erst : open mm
        this.m_tga = TryForget.Try(() => TgaPixelReader.ReadTarga(mapFile), null);
        if (this.m_tga is null) {
            pixelmap = new TgaPixel[0,0];
            return null;
        }

        // Grab as pixel map
        pixelmap = this.m_tga.ToPixelMap().Resize(this.m_tga.Width / 4, this.m_tga.Height / 4); // Convert to pixel map and resize so we can loop over fewer elements
        int width = pixelmap.GetLength(0);
        int height = pixelmap.GetLength(1);

        // Subdivide scatter points
        this.Subdivide(pixelmap, subDivisions, searchRadius);

        // Connect internal nodes
        this.ConnectGridNodes(searchRadius);

        // Connect grids
        this.ConnectGrids(searchRadius);

        // TODO: Node smoothen -> Merge nodes within some radius into one

        // Smooth edges (Do last so we don't accidentally cut useful connections)
        SmoothEdges(this.m_edges);

        // Return success
        if (this.m_edges.Count is 0) {
            
            // Return null -> nothing of value
            return null;

        } else {

            // Grab all nodes as game positons
            var gNodes = this.Nodes.ToArray().Map(x => this.m_scenario.FromMinimapPosition(width, height, x.X, x.Y));

            // Grab all edges
            var gEdges = this.Edges.ToArray().Map(e => {
                int i = this.Nodes.FindIndex(x => e.A.X == x.X && e.A.Y == x.Y);
                int j = this.Nodes.FindIndex(x => e.B.X == x.X && e.B.Y == x.Y);
                return new AIMapAnalysis.RoadConnection(i, j);
            });

            // Grab all important crossroads
            var gChokePoints = gNodes.Mapi((i, x) => {
                int eCount = gEdges.Filter(y => y.First == i || y.Second == i).Length;
                if (eCount > 4) {
                    float w = Math.Max(eCount - 2, 5);
                    return new AIMapAnalysis.StrategicValue(x, AIMapAnalysis.StrategicValueType.Crossroads, w / 5.0f);
                } else {
                    return null;
                }
            }).NotNull();

            // Grab strategic points
            var gStratPoints = this.m_scenario.Points
                .Filter(x => x.EntityBlueprint is "victory_point" or "territory_munitions_point_mp" or "territory_fuel_point_mp" or "territory_point_mp")
                .Map(x => new AIMapAnalysis.StrategicValue(x.Position, x.EntityBlueprint switch {
                    "victory_point" => AIMapAnalysis.StrategicValueType.VictoryPoint,
                    "territory_fuel_point_mp" => AIMapAnalysis.StrategicValueType.Fuel,
                    "territory_munitions_point_mp" => AIMapAnalysis.StrategicValueType.Munitions,
                    "territory_point_mp" => AIMapAnalysis.StrategicValueType.Resource,
                    _ => AIMapAnalysis.StrategicValueType.Crossroads,
                }, x.EntityBlueprint switch {
                    "victory_point" => 0.9f,
                    "territory_fuel_point_mp" => .8f,
                    "territory_munitions_point_mp" => .6f,
                    "territory_point_mp" => .4f,
                    _ => .1f
                }));

            // Return analysis
            return new() {
                Nodes = gNodes,
                Roads = gEdges,
                StrategicPositions = gChokePoints.Concat(gStratPoints),
            };

        }

    }

    [MemberNotNull(nameof(m_grid))]
    private void Subdivide(TgaPixel[,] pixelmap, int subdivisions, int searchRadius) {

        // Calculate row and column sizes
        int w = pixelmap.GetLength(0);
        int h = pixelmap.GetLength(1);
        int columnSize = w / subdivisions;
        int rowSize = h / subdivisions;

        // Create grid
        this.m_grid = new Subdivision[subdivisions, subdivisions];

        // Create weight matrix
        var convmatrix = CreateConvolutionMatrix(searchRadius);

        // Begin subdiviions
        for (int i = 0; i < subdivisions; i++) {
            
            // Calculate offset in actual Y-coordinate
            int yOffset = i * rowSize;

            for (int j = 0; j < subdivisions; j++) {

                // Calculate offset in actual X-coordinate
                int xOffset = j * columnSize;

                // Calculate div caps
                int yCap = yOffset + rowSize;
                int xCap = xOffset + columnSize;

                // Create sub
                this.m_grid[j, i] = new(xOffset, xCap, yOffset, yCap, new());

                // Loop over pixel map
                for (int y = yOffset; y < yCap; y += searchRadius) {
                    for (int x = xOffset; x < xCap; x += searchRadius) {

                        // Grab pixels
                        if (Convolution(x, x + searchRadius, y, y + searchRadius, pixelmap, convmatrix, out Node? node)) {
                            this.m_grid[j, i].Nodes.Add(node);
                        }
                        
                    }
                }

            }
        }

    }

    private static float[,] CreateConvolutionMatrix(int size) {
        int centre = size / 2;
        float[,] matrix = new float[size, size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float di = (centre - i);
                float dj = (centre - j);
                matrix[i, j] = 0.1f + (float)Math.Sqrt(di * di + dj * dj);
            }
        }
        return matrix;
    }

    private bool Convolution(int x0, int x1, int y0, int y1, TgaPixel[,] pixels, float[,] C, [NotNullWhen(true)] out Node? node) {

        // Init node
        node = null;

        // Calculate deltas
        int dy = y1 - y0;
        int dx = x1 - x0;

        // Initialise difference table
        Span<float> tol = stackalloc float[C.Length]; 
        for (int y = 0; y < dy; y++) {
            for (int x = 0; x < dx; x++) {
                tol[y * dx + x] = float.PositiveInfinity;
            }
        }

        // calculate differences
        for (int y = y0; y < y1 && y < pixels.GetLength(1); y++) {
            for (int x = x0; x < x1 && x < pixels.GetLength(0); x++) {

                int _x = x - x0;
                int _y = y - y0;
                int i = _y * dx + _x;
                float d = pixels[x, y].AverageDifference(RoadPixelColour);
                if (d <= 5) {
                    tol[i] = d* C[_x, _y];
                }

            }
        }

        // Find best
        int miny = -1;
        int minx = -1;
        for (int y = 0; y < dy; y++) {
            for (int x = 0; x < dx; x++) {
                int i = y * dx + x;
                float _this = tol[i];
                if (_this != float.PositiveInfinity && ((minx is -1 && miny is -1) || _this < tol[minx + dx * miny])) {
                    minx = x;
                    miny = y;
                }
            }

        }

        // Return
        if (minx is -1 && miny is -1) {
            return false;
        } else {
            node = new(x0 + minx, y1 + miny, pixels[x0 + minx, y0 + miny]);
            return true;
        }

    }

    private void ConnectGridNodes(int searchRadius) {

        // Bail if grid not found
        if (this.m_grid is null)
            return;

        // Loop over all grids
        for (int i = 0; i < this.m_grid.GetLength(1); i++) {
            for (int j = 0; j < this.m_grid.GetLength(0); j++) {

                // Grab subdiv
                var sub = this.m_grid[j, i];
                var edges = FindEdges(sub.Nodes, searchRadius);

                // Remove nodes with no connection
                for (int k = 0; k < sub.Nodes.Count; k++) {
                    if (!edges.Exists(x => x.A.Equals(sub.Nodes[k]) || x.B.Equals(sub.Nodes[k]))) {
                        sub.Nodes.RemoveAt(k--);
                    }
                }

                // Add edges and nodes
                this.m_edges.AddRange(edges);
                this.m_nodes.AddRange(sub.Nodes);

            }
        }

    }

    private static void SmoothEdges(List<Edge> edges) {

        // Loop over edges end smooth them out
        for (int i = 0; i < edges.Count; i++) {

            // Grab edge
            var e = edges[i];

            // Pick edges from remaining
            for (int j = i + 1; j < edges.Count; j++) {

                // Is connect to e?
                if (!edges[j].A.Equals(e.B))
                    continue;

                // Compute direction vectors
                var v1 = Vector2.Normalize(new Vector2(e.B.X - e.A.X, e.B.Y - e.A.Y)); // look at connection node
                var v2 = Vector2.Normalize(new Vector2(edges[j].B.X - e.A.X, edges[j].B.Y - e.A.Y)); // look at end node

                // Take the dot product
                var dot = Vector2.Dot(v1, v2);
                if (dot <= 0.8) {
                    edges[i] = e with { B = edges[j].B };
                    edges.RemoveAt(j--);
                }

            }

        }

    }

    private static List<Edge> FindEdges(List<Node> nodes, int searchRadius) {

        // Create edge container
        var edges = new List<Edge>();

        // Try create edges between all nodes internally
        for (int u = 0; u < nodes.Count; u++) {

            // Prepare nearest neighbour
            int nearestNeighbour = -1;
            double nearestNeighbourDistance = double.MaxValue;

            // Loop through remaining nodes
            for (int v = u + 1; v < nodes.Count; v++) {

                // Grab distance
                var dx = nodes[u].X - nodes[v].X;
                var dy = nodes[u].Y - nodes[v].Y;
                var dis = Math.Sqrt(dx * dx + dy * dy);
                if (dis <= (searchRadius * 2) && dis < nearestNeighbourDistance) {
                    nearestNeighbour = v;
                    nearestNeighbourDistance = dis;
                }

            }

            // If a nearby neighbour is found, form and edge
            if (nearestNeighbour != -1)
                edges.Add(new(nodes[u], nodes[nearestNeighbour]));

        }

        // Return
        return edges;

    }

    private void ConnectGrids(int searchRadius) {

        // Bail if grid not found
        if (this.m_grid is null)
            return;

        // Loop over all grids
        for (int i = 0; i < this.m_grid.GetLength(1); i++) {
            for (int j = 0; j < this.m_grid.GetLength(0); j++) {

                // Grab subdiv
                var sub = this.m_grid[j, i];

                // Grab all 
                var neighbours = GetNeighbouringSubdivisions(this.m_grid, j, i);

                // Loop over neighbours and make connections
                foreach(var neighbour in neighbours) {

                    // Merge nodes
                    var nodes = sub.Nodes.Concat(neighbour.Nodes).ToList();

                    // Find edges
                    var edges = FindEdges(nodes, searchRadius * 2).Where(x => !this.m_edges.Contains(x)).ToList();

                    // Add new found connecting edges
                    this.m_edges.AddRange(edges);

                }

            }

        }

    }

    private static List<Subdivision> GetNeighbouringSubdivisions(Subdivision[,] grid, int u, int v) {
        var divs = new List<Subdivision>();
        for (int i = Math.Max(0, v - 1); i < grid.GetLength(1) && i <= v + 1; i++) {
            for (int j = Math.Max(0, u - 1); j < grid.GetLength(0) && j <= u + 1; j++) {
                if (i == v && j == u)
                    continue;
                divs.Add(grid[j, i]);
            }
        }
        return divs;
    }

}
