using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class RelativeTimeConverterTests {

    private RelativeTimeConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new RelativeTimeConverter();
    }

    [Test]
    public void Convert_NonDateTimeValue_ReturnsUnknown() {
        var result = _converter.Convert("not a date", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Unknown"));
    }

    [Test]
    public void Convert_LessThanOneMinuteAgo_ReturnsJustNow() {
        var result = _converter.Convert(DateTime.Now.AddSeconds(-30), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Just now"));
    }

    [Test]
    public void Convert_MinutesAgo_ReturnsMinutesFormat() {
        var result = _converter.Convert(DateTime.Now.AddMinutes(-5), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("5m ago"));
    }

    [Test]
    public void Convert_HoursAgo_ReturnsHoursFormat() {
        var result = _converter.Convert(DateTime.Now.AddHours(-3), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("3h ago"));
    }

    [Test]
    public void Convert_DaysAgo_ReturnsDaysFormat() {
        var result = _converter.Convert(DateTime.Now.AddDays(-2), typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("2d ago"));
    }

    [Test]
    public void Convert_MoreThanAWeekAgo_ReturnsFormattedDate() {
        var timestamp = DateTime.Now.AddDays(-10);
        var result = _converter.Convert(timestamp, typeof(string), null, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(timestamp.ToString("MMM d, yyyy")));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("Just now", typeof(DateTime), null, CultureInfo.InvariantCulture));
    }

}
