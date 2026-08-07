using System.Globalization;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestFixture]
public class MatchButtonStateConverterTests {

    private MatchButtonStateConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new MatchButtonStateConverter();
    }

    [Test]
    public void Convert_TooFewValues_ReturnsCanStart() {
        var result = _converter.Convert([true], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("CanStart"));
    }

    [Test]
    public void Convert_Host_MatchStarting_ReturnsStarting() {
        var result = _converter.Convert([true, false, false, true], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Starting"));
    }

    [Test]
    public void Convert_Host_WaitingForMatchOver_ReturnsWaiting() {
        var result = _converter.Convert([false, true, false, true], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Waiting"));
    }

    [Test]
    public void Convert_Host_CanStartMatch_ReturnsCanStart() {
        var result = _converter.Convert([false, false, true, true], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("CanStart"));
    }

    [Test]
    public void Convert_Host_CannotStartMatch_ReturnsCannotStart() {
        var result = _converter.Convert([false, false, false, true], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("CannotStart"));
    }

    [Test]
    public void Convert_NonHost_MatchStarting_ReturnsStarting() {
        var result = _converter.Convert([true, false, false, false], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Starting"));
    }

    [Test]
    public void Convert_NonHost_WaitingForMatchOver_ReturnsWaiting() {
        var result = _converter.Convert([false, true, false, false], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Waiting"));
    }

    [Test]
    public void Convert_NonHost_Default_ReturnsReady() {
        var result = _converter.Convert([false, false, false, false], typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("Ready"));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("CanStart", [typeof(bool)], null!, CultureInfo.InvariantCulture));
    }

}

[TestFixture]
public class ButtonStateToContentConverterTests {

    private ButtonStateToContentConverter _converter = null!;

    [SetUp]
    public void SetUp() {
        _converter = new ButtonStateToContentConverter();
    }

    [TestCase("Starting", "STARTING MATCH...")]
    [TestCase("Waiting", "MATCH IN PROGRESS")]
    [TestCase("CanStart", "START MATCH")]
    [TestCase("CannotStart", "CANNOT START")]
    [TestCase("Ready", "READY")]
    [TestCase("Unready", "NOT READY")]
    [TestCase("SomeUnknownState", "START MATCH")]
    public void Convert_KnownState_ReturnsExpectedContent(string state, string expected) {
        var result = _converter.Convert(state, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Convert_NonStringValue_ReturnsStartMatch() {
        var result = _converter.Convert(42, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo("START MATCH"));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() => _converter.ConvertBack("START MATCH", typeof(string), null!, CultureInfo.InvariantCulture));
    }

}
