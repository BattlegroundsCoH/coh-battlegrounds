using System.Globalization;
using System.Windows;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class ZeroToVisibilityConverterTests {

    private ZeroToVisibilityConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new ZeroToVisibilityConverter();
    }

    [Test]
    public void Convert_Zero_ReturnsCollapsed() {
        var result = _converter.Convert(0, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_ZeroFloat_ReturnsCollapsed() {
        var result = _converter.Convert(0.0f, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_Null_ReturnsCollapsed() {
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_NonZero_ReturnsVisible() {
        var result = _converter.Convert(1, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(Visibility.Visible, typeof(int), null, CultureInfo.InvariantCulture));
    }

}
