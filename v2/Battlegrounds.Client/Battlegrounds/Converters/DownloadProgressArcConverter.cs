using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Battlegrounds.Converters;

/// <summary>
/// Converts a <see cref="float"/> progress value (0–1) into a donut-arc <see cref="Geometry"/>
/// suitable for painting a doughnut-style download indicator.
/// The geometry assumes a 40×40 canvas (centre 20,20; outer radius 16; inner radius 10).
/// Returns <see cref="Geometry.Empty"/> for values ≤ 0.
/// </summary>
public sealed class DownloadProgressArcConverter : IValueConverter {

    private const double Cx = 20.0;
    private const double Cy = 20.0;
    private const double OuterR = 16.0;
    private const double InnerR = 10.0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not float progress || progress <= 0f)
            return Geometry.Empty;

        if (progress >= 1.0f)
            return BuildFullRing();

        return BuildPartialArc(progress);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    // Full donut ring (two concentric circles, EvenOdd fill rule punchces the hole)
    private static Geometry BuildFullRing() {
        var pg = new PathGeometry { FillRule = FillRule.EvenOdd };

        pg.Figures.Add(BuildCircle(OuterR, SweepDirection.Clockwise));
        pg.Figures.Add(BuildCircle(InnerR, SweepDirection.Clockwise));

        return pg;
    }

    // Split a full circle into two 180° arcs so WPF doesn't discard the degenerate arc
    private static PathFigure BuildCircle(double radius, SweepDirection direction) {
        var top    = new Point(Cx, Cy - radius);
        var bottom = new Point(Cx, Cy + radius);
        var size   = new Size(radius, radius);

        var figure = new PathFigure { StartPoint = top, IsClosed = true };
        figure.Segments.Add(new ArcSegment(bottom, size, 0, isLargeArc: true, direction, isStroked: true));
        figure.Segments.Add(new ArcSegment(top,    size, 0, isLargeArc: true, direction, isStroked: true));
        return figure;
    }

    // Partial donut arc from 12-o'clock clockwise to the given progress fraction
    private static Geometry BuildPartialArc(float progress) {
        double angle = progress * 2.0 * Math.PI;
        bool isLargeArc = angle > Math.PI;

        double endXOuter = Cx + OuterR * Math.Sin(angle);
        double endYOuter = Cy - OuterR * Math.Cos(angle);
        double endXInner = Cx + InnerR * Math.Sin(angle);
        double endYInner = Cy - InnerR * Math.Cos(angle);

        var outerSize = new Size(OuterR, OuterR);
        var innerSize = new Size(InnerR, InnerR);

        var figure = new PathFigure { StartPoint = new Point(Cx, Cy - OuterR), IsClosed = true };
        figure.Segments.Add(new ArcSegment(new Point(endXOuter, endYOuter), outerSize, 0, isLargeArc, SweepDirection.Clockwise,        isStroked: true));
        figure.Segments.Add(new LineSegment( new Point(endXInner, endYInner),                                                            isStroked: true));
        figure.Segments.Add(new ArcSegment(new Point(Cx, Cy - InnerR),         innerSize, 0, isLargeArc, SweepDirection.Counterclockwise, isStroked: true));

        var pg = new PathGeometry();
        pg.Figures.Add(figure);
        return pg;
    }

}
