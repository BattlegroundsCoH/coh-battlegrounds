using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Util;

namespace Battlegrounds.Gfx;

[StructLayout(LayoutKind.Explicit)]
public readonly struct TgaPixel {

    public static readonly TgaPixel BLACK = new(0, 0, 0);
    public static readonly TgaPixel WHITE = new(255, 255, 255);

    [FieldOffset(2)]
    public readonly byte R;

    [FieldOffset(1)]
    public readonly byte G;

    [FieldOffset(0)]
    public readonly byte B;

    [FieldOffset(3)]
    public readonly byte A;

    public TgaPixel(byte R, byte G, byte B, byte A) {
        this.R = R;
        this.G = G;
        this.B = B;
        this.A = A;
    }
    public TgaPixel(byte R, byte G, byte B) : this(R, G, B, 255) { }
    public override string ToString() => $"({this.R},{this.G},{this.B},{this.A})";
    public double DistanceTo(TgaPixel p) => Math.Sqrt((this.R - p.R).Square() + (this.G - p.G).Square() + (this.B - p.B).Square());
    public bool WithinTolerance(TgaPixel p, int r=4,int g=4, int b = 4) {
        return Math.Abs(p.R - this.R) <= r && Math.Abs(p.G - this.G) <= g && Math.Abs(p.B - this.B) <= b;
    }
    public float AverageDifference(TgaPixel p) {
        int r = this.R;
        int g = this.G;
        int b = this.B;
        return (Math.Abs(r - p.R) + Math.Abs(g - p.G) + Math.Abs(b - p.B)) / 3f;
    }
    public bool WithinTolerance(TgaPixel p, int tolerance) => this.WithinTolerance(p, tolerance, tolerance, tolerance);
    public override int GetHashCode() {
        return HashCode.Combine(this.R, this.G, this.B, this.A);
    }
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is TgaPixel p) {
            return p.GetHashCode() == this.GetHashCode();
        }
        return false;
    }
}

[StructLayout(LayoutKind.Explicit)]
public readonly struct TgaPixelRGB {
    [FieldOffset(2)]
    public readonly byte R;
    [FieldOffset(1)]
    public readonly byte G;
    [FieldOffset(0)]
    public readonly byte B;
    public static implicit operator TgaPixel(TgaPixelRGB rgb) => new(rgb.R, rgb.G, rgb.B);
}

public static class TgaPixelMap {

    public static TgaPixel[,] Resize(this TgaPixel[,] map, int w, int h) {

        TgaPixel[,] resized = new TgaPixel[w, h];

        int wstep = map.GetLength(0) / w;
        int hstep = map.GetLength(1) / h;
        int c = wstep * hstep;

        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {

                int r = 0;
                int g = 0;
                int b = 0;
                // Ignoring alpha channel for now
                for (int cy = 0; cy < hstep; cy++) {
                    for (int cx = 0; cx < wstep; cx++) {
                        var p = map[x * wstep + cx, y * hstep + cy];
                        r += p.R;
                        g += p.G;
                        b += p.B;
                    }
                }

                r /= c;
                g /= c;
                b /= c;
                resized[x, y] = new((byte)r, (byte)g, (byte)b);

            }
        }

        return resized;

    }

}
