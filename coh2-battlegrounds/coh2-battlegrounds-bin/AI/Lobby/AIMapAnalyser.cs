using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

using Battlegrounds.ErrorHandling;
using Battlegrounds.Game.Database;
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
            }
            return false;
        }
        public override int GetHashCode() {
            return HashCode.Combine(this.X, this.Y);
        }
        public Node? Parent { get; set; }
    }

    public record Subdivision(int Left, int Right, int Top, int Bottom, List<Node> Nodes);

    public record Edge(Node First, Node Second) {
        public bool SameEdge(Edge other) {
            return (other.First.Equals(this.First) && other.Second.Equals(this.Second))
                || (other.First.Equals(this.Second) && other.Second.Equals(this.First)); 
        }
    }

    private static readonly Random __rand = new(1944); // "Deterministic random generator"
   
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

    public bool Analyze(out TgaPixel[,] pixelmap) {

        // Grab file
        var mapFile = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, $"map_icons\\{this.m_scenario.RelativeFilename}_map.tga");

        // Zu erst : open mm
        this.m_tga = TryForget.Try(() => TgaPixelReader.ReadTarga(mapFile), null);
        if (this.m_tga is null) {
            pixelmap = new TgaPixel[0,0];
            return false;
        }

        // Grab as pixel map
        pixelmap = this.m_tga.ToPixelMap().Resize(this.m_tga.Width / 4, this.m_tga.Height / 4); // Convert to pixel map and resize so we can loop over fewer elements

        // Create radius
        var searchRadius = 3;
        var subDivisions = 8;

        // Subdivide scatter points
        this.Subdivide(pixelmap, subDivisions, searchRadius);

        // Connect internal nodes
        this.ConnectGridNodes(searchRadius);

        // Connect grids
        this.ConnectGrids(searchRadius);

        // Return success
        return this.m_edges.Count > 0;

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

                // Smooth path
                SmoothEdges(edges);

                // Remove nodes with no connection
                for (int k = 0; k < sub.Nodes.Count; k++) {
                    if (!edges.Exists(x => x.First.Equals(sub.Nodes[k]) || x.Second.Equals(sub.Nodes[k]))) {
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
        for (int k = 0; k < edges.Count; k++) {

            // Grab edge
            var e = edges[k];

            // While there exists an edge coming from the outgoing edge
            while (edges.Find(x => x.First.Equals(e.Second)) is Edge outEdge) {

                // Compute direction vectors
                var v1 = Vector2.Normalize(new Vector2(e.Second.X - e.First.X, e.Second.Y - e.First.Y)); // look at connection node
                var v2 = Vector2.Normalize(new Vector2(outEdge.Second.X - e.First.X, outEdge.Second.Y - e.First.Y)); // look at end node

                // Take the dot product
                var dot = Vector2.Dot(v1, v2);
                if (dot < 0.4) {
                    edges[k] = e with { Second = outEdge.Second };
                    edges.Remove(outEdge);
                } else {
                    break; // "valid" edge
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
                if (dis < (searchRadius * 2) && dis < nearestNeighbourDistance) {
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

                    // Smooth
                    SmoothEdges(edges);

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

    private Stack<Node> SimplifyPath(Stack<Node> path) {

        // Prepare queue
        Queue<Node> queue = new ();

        // Get first element and add to queue
        var current = path.Pop();
        queue.Enqueue(current);

        // Pop until no more path elements
        while (path.Count > 0) {

            // Get next
            var next = path.Pop();

            // Check if there's a bend
            int dx = Math.Abs(current.X - next.X);
            int dy = Math.Abs(current.Y - next.Y);
            if ((dx == 0 && dy != 0) || (dx != 0 && dy == 0)) {
                if (path.Count is 0) {
                    queue.Enqueue(next);
                }
                continue;
            }

            // Add
            queue.Enqueue(next);
            current = next;

        }

        return new(queue);

    }

    private static bool GetPath(Node start, Node end, TgaPixel[,] tgaPixels, out Stack<Node> path) { // DFS

        // Create stack
        var stack = new Stack<Node>();
        var visited = new HashSet<Node>();

        // Init
        visited.Add(start);
        stack.Push(start);

        // Flag if found
        bool found = false;

        // While node to consider
        while (stack.Count > 0) {

            // Pop
            var cur = stack.Pop();

            // Halt if destination
            if (cur.X == end.X && cur.Y == end.Y) {
                end.Parent = cur;
                found = true;
                break;
            }

            // Get neighbours
            var neighbours = GetNeighbours(cur, tgaPixels, visited);
            for (int i = 0; i < neighbours.Count; i++) {
                neighbours[i].Parent = cur;
                visited.Add(neighbours[i]);
                stack.Push(neighbours[i]);
            }

        }

        // Create stack for backtracked path
        path = new();

        // If found, backtrack
        if (found) {
            var backtrack = end;
            path.Push(end);
            while (backtrack.Parent is not null) {
                path.Push(backtrack.Parent);
                backtrack = backtrack.Parent;
            }
        }

        // Return false ==> no path found
        return found;

    }

    private static List<Node> GetNeighbours(Node current, TgaPixel[,] tgaPixels, HashSet<Node> visited) {

        // Container for neighbours
        var neighbours = new List<Node>();

        // Cap min/max x
        int minx = Math.Max(0, current.X - 1);
        int maxx = Math.Min(tgaPixels.GetLength(0), current.X + 1);

        // Cap min/max y
        int miny = Math.Max(0, current.Y - 1);
        int maxy = Math.Min(tgaPixels.GetLength(1), current.Y + 1);

        // Loop
        for (int y = miny; y < maxy; y++) {
            for (int x = minx; x < maxx; x++) {
                var p = tgaPixels[x, y];
                var d = p.DistanceTo(current.Pixel);
                if (p.WithinTolerance(current.Pixel, tolerance:8)) {
                    var n = new Node(x, y, p);
                    if (visited.Add(n)) {
                        neighbours.Add(n);
                    }
                }
            }
        }

        // Return neighbours
        return neighbours;

    }

}
