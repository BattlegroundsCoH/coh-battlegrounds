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

    // Regression: the converter used to do `value as string`, which returns null (and is
    // therefore treated as "nothing to show") for any non-string reference type. This made
    // bindings to non-string view-model objects (e.g. a selection view-model) permanently
    // Collapsed regardless of whether the object was actually null.
    [Test]
    public void Convert_NonNullNonStringObject_ReturnsVisible() {
        var result = _converter.Convert(new object(), typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_EmptyString_ReturnsCollapsed() {
        var result = _converter.Convert(string.Empty, typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_WhitespaceString_ReturnsCollapsed() {
        var result = _converter.Convert("   ", typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_Inverted_NonNullNonStringObject_ReturnsCollapsed() {
        _converter.IsInverted = true;
        var result = _converter.Convert(new object(), typeof(Visibility), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

}
