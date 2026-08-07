using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class InverseBooleanConverterTests {

    private InverseBooleanConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new InverseBooleanConverter();
    }

    [Test]
    public void Convert_True_ReturnsFalse() {
        var result = _converter.Convert(true, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void Convert_False_ReturnsTrue() {
        var result = _converter.Convert(false, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void Convert_NonBoolValue_ReturnsValueUnchanged() {
        var result = _converter.Convert("not a bool", typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("not a bool"));
    }

    [Test]
    public void ConvertBack_True_ReturnsFalse() {
        var result = _converter.ConvertBack(true, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void ConvertBack_False_ReturnsTrue() {
        var result = _converter.ConvertBack(false, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(true));
    }

}
