using System.Globalization;
using System.Windows;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class EmptyStringToVisibilityConverterTests {

    private EmptyStringToVisibilityConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new EmptyStringToVisibilityConverter();
    }

    [Test]
    public void Convert_Null_ReturnsVisible() {
        var result = _converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_EmptyString_ReturnsVisible() {
        var result = _converter.Convert(string.Empty, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_NonEmptyString_ReturnsCollapsed() {
        var result = _converter.Convert("hello", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_WhitespaceOnlyString_ReturnsCollapsed() {
        // Only IsNullOrEmpty is checked, not IsNullOrWhiteSpace, so whitespace is treated as content.
        var result = _converter.Convert("   ", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(Visibility.Visible, typeof(string), null!, CultureInfo.InvariantCulture));
    }

}
