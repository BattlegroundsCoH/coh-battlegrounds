using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class UpperCaseConverterTests {

    private UpperCaseConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new UpperCaseConverter();
    }

    [Test]
    public void Convert_LowerCaseString_ReturnsUpperCase() {
        var result = _converter.Convert("hello world", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("HELLO WORLD"));
    }

    [Test]
    public void Convert_Null_ReturnsNull() {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_NonStringValue_UsesToString() {
        var result = _converter.Convert(42, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("42"));
    }

    [Test]
    public void ConvertBack_ThrowsNotSupportedException() {
        Assert.Throws<NotSupportedException>(() => _converter.ConvertBack("HELLO", typeof(string), null, CultureInfo.InvariantCulture));
    }

}
