using System;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Util;

namespace BattlegroundsApp.Utilities;

public static class Vectors {

    public static Vector FromTransform(TranslateTransform t) => new(t.X, t.Y);

    public static Vector FromPoint(Point p) => new(p.X, p.Y);

    public static Point ToPoint(this Vector v) => new(v.X, v.Y);

    public static Vector Interpolate(Vector x, Vector y, double t) => x * t + y * (1 - t);

    public static double Distance(Vector x, Vector y)
        => Math.Sqrt((y.X - x.X).Square() + (y.Y  - x.Y).Square());

}
