using System.Globalization;
using System.Windows.Media;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class DownloadProgressArcConverterTests {

    private DownloadProgressArcConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new DownloadProgressArcConverter();
    }

    [Test]
    public void Convert_NonFloatValue_ReturnsEmptyGeometry() {
        var result = _converter.Convert("not a float", typeof(Geometry), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Geometry.Empty));
    }

    [Test]
    public void Convert_ZeroProgress_ReturnsEmptyGeometry() {
        var result = _converter.Convert(0f, typeof(Geometry), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Geometry.Empty));
    }

    [Test]
    public void Convert_NegativeProgress_ReturnsEmptyGeometry() {
        var result = _converter.Convert(-0.5f, typeof(Geometry), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Geometry.Empty));
    }

    [Test]
    public void Convert_FullProgress_ReturnsNonEmptyGeometry() {
        var result = _converter.Convert(1.0f, typeof(Geometry), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.EqualTo(Geometry.Empty));
    }

    [Test]
    public void Convert_OverfullProgress_ReturnsNonEmptyGeometry() {
        var result = _converter.Convert(1.5f, typeof(Geometry), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.EqualTo(Geometry.Empty));
    }

    [Test]
    public void Convert_PartialProgress_ReturnsNonEmptyGeometry() {
        var result = _converter.Convert(0.35f, typeof(Geometry), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.EqualTo(Geometry.Empty));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(Geometry.Empty, typeof(float), null, CultureInfo.InvariantCulture));
    }

}
