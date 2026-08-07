using System.Globalization;
using System.Windows;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class InvertedBooleanToVisibilityConverterTests {

    private InvertedBooleanToVisibilityConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new InvertedBooleanToVisibilityConverter();
    }

    [Test]
    public void Convert_True_ReturnsCollapsed() {
        var result = _converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_False_ReturnsVisible() {
        var result = _converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_NonBoolValue_ReturnsVisible() {
        var result = _converter.Convert("not a bool", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void ConvertBack_Collapsed_ReturnsFalse() {
        var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void ConvertBack_Visible_ReturnsTrue() {
        var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void ConvertBack_NonVisibilityValue_ReturnsTrue() {
        var result = _converter.ConvertBack("not a visibility", typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(true));
    }

}
