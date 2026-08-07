using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class LockImageConverterTests {

    private LockImageConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new LockImageConverter();
    }

    [Test]
    public void Convert_True_ReturnsUnlockedImage() {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("pack://siteoforigin:,,,/Assets/Misc/unlocked.png"));
    }

    [Test]
    public void Convert_False_ReturnsLockedImage() {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("pack://siteoforigin:,,,/Assets/Misc/locked.png"));
    }

    [Test]
    public void Convert_NonBoolValue_ThrowsInvalidCastException() {
        Assert.Throws<InvalidCastException>(() => _converter.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("locked.png", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

}
