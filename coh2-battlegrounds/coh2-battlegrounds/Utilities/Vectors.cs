using System.Windows;
using System.Windows.Media;

namespace BattlegroundsApp.Utilities;

public static class Vectors {

    public static Vector FromTransform(TranslateTransform t) => new(t.X, t.Y);

    public static Vector FromPoint(Point p) => new(p.X, p.Y);

    public static Point ToPoint(this Vector v) => new(v.X, v.Y);

    public static Vector Interpolate(Vector x, Vector y, double t) => x * t + y * (1 - t); 

}
