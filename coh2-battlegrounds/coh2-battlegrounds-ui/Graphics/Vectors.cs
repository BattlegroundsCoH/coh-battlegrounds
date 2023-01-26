using System;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Util;

namespace Battlegrounds.UI.Graphics;

/// <summary>
/// Static utiltiy class for additional <see cref="Vector"/> and <see cref="Point"/> functionalities.
/// </summary>
public static class Vectors {

    /// <summary>
    /// Get the 2D position <see cref="Vector"/> from a <see cref="TranslateTransform"/>.
    /// </summary>
    /// <param name="t">The transform to extract position from.</param>
    /// <returns>A <see cref="Vector"/> composed of the X and Y translation values of <paramref name="t"/>.</returns>
    public static Vector FromTransform(TranslateTransform t) => new(t.X, t.Y);

    /// <summary>
    /// Get a <see cref="Vector"/> from a <see cref="Point"/>.
    /// </summary>
    /// <param name="p">The point to get <see cref="Vector"/> represention of.</param>
    /// <returns>The <see cref="Vector"/> representing <paramref name="p"/>.</returns>
    public static Vector FromPoint(Point p) => new(p.X, p.Y);

    /// <summary>
    /// Get a <see cref="Point"/> from a <see cref="Vector"/>.
    /// </summary>
    /// <param name="v">The vector to get a <see cref="Point"/> represention of.</param>
    /// <returns>The <see cref="Point"/> representing <paramref name="v"/>.</returns>
    public static Point ToPoint(this Vector v) => new(v.X, v.Y);

    /// <summary>
    /// Interpolate a <see cref="Vector"/> between to other <see cref="Vector"/> instances.
    /// </summary>
    /// <param name="x">The first vector.</param>
    /// <param name="y">The second vector.</param>
    /// <param name="t">The interpolation variable (0.5 => halfway between <paramref name="x"/> and <paramref name="y"/>).</param>
    /// <returns>An interpolated vector at time <paramref name="t"/> between <paramref name="x"/> and <paramref name="y"/>.</returns>
    public static Vector Interpolate(Vector x, Vector y, double t) => x * t + y * (1 - t);

    /// <summary>
    /// Calculates the distance between <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// <param name="x">First vector.</param>
    /// <param name="y">Second vector.</param>
    /// <returns>The euclidean distance between <paramref name="x"/> and <paramref name="y"/>.</returns>
    public static double Distance(Vector x, Vector y)
        => Math.Sqrt((y.X - x.X).Square() + (y.Y - x.Y).Square());

}
