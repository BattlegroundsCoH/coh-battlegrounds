using System.Globalization;

using Battlegrounds.Converters;
using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class GamemodeNameConverterTests {

    private GamemodeNameConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new GamemodeNameConverter();
    }

    [Test]
    public void Convert_KnownGamemodeSetting_ReturnsDisplayName() {
        var result = _converter.Convert(LobbySetting.SETTING_GAMEMODE, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Gamemode"));
    }

    [Test]
    public void Convert_KnownVictoryPointsSetting_ReturnsDisplayName() {
        var result = _converter.Convert(LobbySetting.SETTING_VICTORY_POINTS, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Victory Points"));
    }

    [Test]
    public void Convert_UnknownStringValue_ReturnsValueUnchanged() {
        var result = _converter.Convert("some_unknown_setting", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("some_unknown_setting"));
    }

    [Test]
    public void Convert_NonStringValue_ReturnsValueUnchanged() {
        object value = 42;
        var result = _converter.Convert(value, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(value));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("Gamemode", typeof(string), null!, CultureInfo.InvariantCulture));
    }

}
