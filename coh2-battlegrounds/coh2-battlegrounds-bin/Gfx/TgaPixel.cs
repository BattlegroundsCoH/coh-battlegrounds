using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Battlegrounds.Util;

namespace Battlegrounds.Gfx;

/// <summary>
/// Readonly struct representing a pixel in a <see cref="TgaImage"/>.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct TgaPixel {

    /// <summary>
    /// Black pixel RGB(0,0,0).
    /// </summary>
    public static readonly TgaPixel BLACK = new(0, 0, 0);

    /// <summary>
    /// White pixel RGB(255,255,255).
    /// </summary>
    public static readonly TgaPixel WHITE = new(255, 255, 255);

    /// <summary>
    /// The red channel.
    /// </summary>
    [FieldOffset(2)]
    public readonly byte R;

    /// <summary>
    /// The green channel.
    /// </summary>
    [FieldOffset(1)]
    public readonly byte G;

    /// <summary>
    /// The blue channel.
    /// </summary>
    [FieldOffset(0)]
    public readonly byte B;

    /// <summary>
    /// The alpa channel.
    /// </summary>
    [FieldOffset(3)]
    public readonly byte A;

    /// <summary>
    /// Initialise a new <see cref="TgaPixel"/> instance with RGBA channels.
    /// </summary>
    /// <param name="R">The red channel value.</param>
    /// <param name="G">The green channel value.</param>
    /// <param name="B">The blue channel value.</param>
    /// <param name="A">The alpha channel value.</param>
    public TgaPixel(byte R, byte G, byte B, byte A) {
        this.R = R;
        this.G = G;
        this.B = B;
        this.A = A;
    }

    /// <summary>
    /// Initialise a new <see cref="TgaPixel"/> instance with RGB channels.
    /// </summary>
    /// <param name="R">The red channel value.</param>
    /// <param name="G">The green channel value.</param>
    /// <param name="B">The blue channel value.</param>
    public TgaPixel(byte R, byte G, byte B) : this(R, G, B, 255) { }

    public override string ToString() => $"({this.R},{this.G},{this.B},{this.A})";

    /// <summary>
    /// Computes the euclidean distance between to pixel values in the RGB colour space.
    /// </summary>
    /// <param name="p">The other <see cref="TgaPixel"/> to get colour space distance to.</param>
    /// <returns>The euclidean distance between the two pixels in RGB colour space.</returns>
    public double DistanceTo(TgaPixel p) => Math.Sqrt((this.R - p.R).Square() + (this.G - p.G).Square() + (this.B - p.B).Square());

    /// <summary>
    /// Detmine if <paramref name="p"/> is within the same channel range on the RGB channels.
    /// </summary>
    /// <param name="p">The pixel to compare with.</param>
    /// <param name="r">The max difference on the red channel.</param>
    /// <param name="g">The max difference on the green channel.</param>
    /// <param name="b">The max difference on the blue channel.</param>
    /// <returns>if <paramref name="p"/> has all channel values within the specified tolerance <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool WithinTolerance(TgaPixel p, int r=4,int g=4, int b = 4) {
        int _r = this.R;
        int _g = this.G;
        int _b = this.B;
        return Math.Abs(_r - p.R) <= r && Math.Abs(_g - p.G) <= g && Math.Abs(_b - p.B) <= b;
    }

    /// <summary>
    /// Computes the average difference between the pixel and <paramref name="p"/>.
    /// </summary>
    /// <param name="p">The pixel to compute average channel difference to.</param>
    /// <returns>The average difference on the RGB channels.</returns>
    public float AverageDifference(TgaPixel p) {
        int r = this.R;
        int g = this.G;
        int b = this.B;
        return (Math.Abs(r - p.R) + Math.Abs(g - p.G) + Math.Abs(b - p.B)) / 3f;
    }

    public override int GetHashCode() {
        return HashCode.Combine(this.R, this.G, this.B, this.A);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is TgaPixel p) {
            return p.GetHashCode() == this.GetHashCode();
        }
        return false;
    }

    public static bool operator ==(TgaPixel left, TgaPixel right) {
        return left.Equals(right);
    }

    public static bool operator !=(TgaPixel left, TgaPixel right) {
        return !(left == right);
    }

}

/// <summary>
/// Simplified readonly struct version of <see cref="TgaPixel"/>.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public readonly struct TgaPixelRGB {

    /// <summary>
    /// The red channel.
    /// </summary>
    [FieldOffset(2)]
    public readonly byte R;

    /// <summary>
    /// The green channel.
    /// </summary>
    [FieldOffset(1)]
    public readonly byte G;
    
    /// <summary>
    /// The blue channel.
    /// </summary>
    [FieldOffset(0)]
    public readonly byte B;

    public static implicit operator TgaPixel(TgaPixelRGB rgb) => new(rgb.R, rgb.G, rgb.B);

}

/// <summary>
/// Static utility class for working with <see cref="TgaPixel"/> maps.
/// </summary>
public static class TgaPixelMap {

    /// <summary>
    /// Resize the input <paramref name="map"/> to a <i>stricly</i> smaller pixel map.
    /// </summary>
    /// <param name="map">The original map to resize.</param>
    /// <param name="w">The new width.</param>
    /// <param name="h">The new height.</param>
    /// <returns>A smaller <see cref="TgaPixel"/>[,] map.</returns>
    /// <exception cref="ArgumentException"/>
    public static TgaPixel[,] Resize(this TgaPixel[,] map, int w, int h) {

        // Grab original image size
        int ow = map.GetLength(0);
        int oh = map.GetLength(1);

        // Ensure new width is smaller than original width
        if (w > ow)
            throw new ArgumentException($"Invalid resize width '{w}'. Must be smaller than '{ow}'.", nameof(w));

        // Ensure new width is smaller than original width
        if (h > oh)
            throw new ArgumentException($"Invalid resize height '{h}'. Must be smaller than '{oh}'.", nameof(h));

        // Allocate new map
        TgaPixel[,] resized = new TgaPixel[w, h];

        // Compute stepsizes and visit matrix size (c)
        int wstep = ow / w;
        int hstep = oh / h;
        int c = wstep * hstep;

        // Loop over and resize
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

                // Store result
                resized[x, y] = new((byte)(r / c), (byte)(g / c), (byte)(b / c));

            }
        }

        return resized;

    }

}
