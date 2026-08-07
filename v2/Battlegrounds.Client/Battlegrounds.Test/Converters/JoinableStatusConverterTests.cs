using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class JoinableStatusConverterTests {

    private JoinableStatusConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new JoinableStatusConverter();
    }

    [Test]
    public void Convert_True_ReturnsOpen() {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Open"));
    }

    [Test]
    public void Convert_False_ReturnsFull() {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Full"));
    }

    [Test]
    public void Convert_NonBoolValue_ReturnsUnknown() {
        var result = _converter.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Unknown"));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("Open", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

}
