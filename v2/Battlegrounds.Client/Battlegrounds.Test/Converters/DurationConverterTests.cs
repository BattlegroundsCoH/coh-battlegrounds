using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class DurationConverterTests {

    private DurationConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new DurationConverter();
    }

    [Test]
    public void Convert_NonTimeSpanValue_ReturnsZeroMinutes() {
        var result = _converter.Convert("not a timespan", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("0m"));
    }

    [Test]
    public void Convert_LessThanOneMinute_ReturnsSeconds() {
        var result = _converter.Convert(TimeSpan.FromSeconds(45), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("45s"));
    }

    [Test]
    public void Convert_LessThanOneHour_ReturnsMinutesAndSeconds() {
        var result = _converter.Convert(new TimeSpan(0, 5, 30), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("5m 30s"));
    }

    [Test]
    public void Convert_OneHourOrMore_ReturnsHoursAndMinutes() {
        var result = _converter.Convert(new TimeSpan(2, 15, 0), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("2h 15m"));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("1h 0m", typeof(TimeSpan), null, CultureInfo.InvariantCulture));
    }

}
