using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class ReadyStateConverterTests {

    private ReadyStateConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new ReadyStateConverter();
    }

    [Test]
    public void Convert_True_ReturnsReady() {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Ready"));
    }

    [Test]
    public void Convert_False_ReturnsUnready() {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Unready"));
    }

    [Test]
    public void Convert_NonBoolValue_ReturnsUnready() {
        var result = _converter.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Unready"));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("Ready", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

}
