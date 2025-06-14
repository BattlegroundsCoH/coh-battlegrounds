using System.Globalization;
using System.Windows;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class NullVisibilityConverterTests {

    private NullVisibilityConverter _converter;

    [SetUp]
    public void SetUp() {
        _converter = new NullVisibilityConverter();
    }

    [Test]
    public void Convert_NullValue_ReturnsCollapsed() {
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_NonNullValue_ReturnsVisible() {
        var result = _converter.Convert("Test", typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_Inverted_NullValue_ReturnsVisible() {
        _converter.IsInverted = true;
        var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_Inverted_NonNullValue_ReturnsCollapsed() {
        _converter.IsInverted = true;
        var result = _converter.Convert("Test", typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(Visibility.Visible, typeof(object), null, CultureInfo.InvariantCulture));
    }

}
