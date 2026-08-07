using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class BoolToItemTypeTests {

    private BoolToItemType _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new BoolToItemType();
    }

    [Test]
    public void Convert_True_ReturnsTeamWeapon() {
        var result = _converter.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Team Weapon"));
    }

    [Test]
    public void Convert_False_ReturnsWeaponPickup() {
        var result = _converter.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Weapon Pickup"));
    }

    [Test]
    public void Convert_NonBoolValue_ReturnsWeaponPickup() {
        var result = _converter.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Weapon Pickup"));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("Team Weapon", typeof(bool), null!, CultureInfo.InvariantCulture));
    }

}
