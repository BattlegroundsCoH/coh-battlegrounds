using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class LockStateConverterTests {

    private LockStateConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new LockStateConverter();
    }

    [Test]
    public void Convert_True_ReturnsUnlockSlot() {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Unlock Slot"));
    }

    [Test]
    public void Convert_False_ReturnsLockSlot() {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Lock Slot"));
    }

    [Test]
    public void Convert_NonBoolValue_ThrowsInvalidCastException() {
        Assert.Throws<InvalidCastException>(() => _converter.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("Lock Slot", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

}
