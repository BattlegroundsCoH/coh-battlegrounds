using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.ErrorHandling;
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

    public record Edge(Node First, Node Second) {
        public bool SameEdge(Edge other) {
            return (other.First.Equals(this.First) && other.Second.Equals(this.Second))
                || (other.First.Equals(this.Second) && other.Second.Equals(this.First)); 
        }
    }

    private static readonly Random __rand = new(1944); // "Deterministic random generator"
   
    private readonly string m_mmfile;
    private readonly List<Edge> m_edges;
    private readonly List<Node> m_nodes;
    private TgaImage? m_tga;

    public List<Node> Nodes => this.m_nodes;

    public List<Edge> Edges => this.m_edges;

    public AIMapAnalyser(string mmfile) {
        this.m_mmfile = mmfile;
        this.m_edges = new();
        this.m_nodes = new();
    }

    public bool Analyze(out TgaPixel[,] pixelmap) {

        // Zu erst : open mm
        this.m_tga = TryForget.Try(() => TgaPixelReader.ReadTarga(this.m_mmfile), null);
        if (this.m_tga is null) {
            pixelmap = new TgaPixel[0,0];
            return false;
        }

        // Grab as pixel map
        pixelmap = this.m_tga.ToPixelMap().Resize(this.m_tga.Width / 4, this.m_tga.Height / 4);
        int w = pixelmap.GetLength(0);
        int h = pixelmap.GetLength(1);

        // Calculate scatter points
        int sPointCount = 512;
        HashSet<Node> scatterPoints = new();

        // Make cutoffs (so we don't consider edges too much)
        int cutW = w / 8;
        int cutH = h / 8;

        // Define "road" colours
        var r1 = new TgaPixel(151, 151, 130);
        var r2 = new TgaPixel(148, 148, 128);
        var r3 = new TgaPixel(156, 156, 136);
        //var abv = new TgaPixel((byte)((151 + 148 + 156) / 3.0), (byte)((151 + 148 + 156) / 3.0), (byte)((130 + 128 + 136) / 3.0));
        var abv = new TgaPixel(142, 142, 124);

        // Generate scatter points
        while (scatterPoints.Count < sPointCount) {

            // Grab X/Y
            int x = __rand.Next(cutW, w - cutW);
            int y = __rand.Next(cutH, h - cutH);

            // Ignore black
            var p = pixelmap[x, y];
            //var d = p.DistanceTo(abv);
            if (!p.WithinTolerance(abv, 8)) {
                continue;
            }

            // Add
            scatterPoints.Add(new(x, y, p));

        }

        // Grab points
        this.m_nodes.AddRange(scatterPoints);

        // Try get paths
        for (int i = 0; i < this.m_nodes.Count; i++) {

            for (int j = i + 1; j < this.m_nodes.Count; j++) {

                if (GetPath(this.m_nodes[i], this.m_nodes[j], pixelmap, out var p)) {
                    p = SimplifyPath(p);
                    var k = p.Pop();
                    while (p.Count > 0) {
                        var next = p.Pop();
                        var e = new Edge(k, next);
                        if (!this.m_edges.Exists(x => x.SameEdge(e)))
                            this.m_edges.Add(new(k, next));

                        k = next;
                    }
                }

            }

        }

        // Return success
        return true;

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
