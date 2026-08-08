using System.Globalization;
using System.Windows;

using Battlegrounds.Converters;

namespace Battlegrounds.Test.Converters;

[TestOf(typeof(AspectRatioConverter))]
public class AspectRatioConverterTests {

    private AspectRatioConverter _converter;

    [SetUp]
    public void SetUp() {
        _converter = new AspectRatioConverter();
    }

    private object Convert(object? width, object? ratio, CultureInfo? culture = null)
        => _converter.Convert(width, typeof(double), ratio, culture ?? CultureInfo.InvariantCulture);

    [Test]
    public void Convert_AtSixteenByNine_ReturnsTheMatchingHeight() {

        // Act
        var result = Convert(640d, "16:9");

        // Assert
        Assert.That(result, Is.EqualTo(360d), "A 640-wide 16:9 box is 360 tall");

    }

    [Test]
    public void Convert_AtSixteenByTen_ReturnsTheMatchingHeight() {

        // Act
        var result = Convert(640d, "16:10");

        // Assert
        Assert.That(result, Is.EqualTo(400d), "A 640-wide 16:10 box is 400 tall");

    }

    [Test]
    public void Convert_ScalesWithTheWidth() {

        // Act — the whole point: the ratio holds however wide the container makes the box
        var narrow = Convert(320d, "16:9");
        var wide = Convert(1280d, "16:9");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(narrow, Is.EqualTo(180d), "A 320-wide 16:9 box is 180 tall");
            Assert.That(wide, Is.EqualTo(720d), "A 1280-wide 16:9 box is 720 tall");
        }

    }

    [Test]
    public void Convert_InACultureWithADecimalComma_StillParsesTheRatio() {

        // Arrange — the parameter is authored in XAML, not entered by a user, so it must not be
        // reinterpreted by the machine's locale
        var german = new CultureInfo("de-DE");

        // Act
        var result = Convert(640d, "16:9", german);

        // Assert
        Assert.That(result, Is.EqualTo(360d), "The ratio should parse invariantly");

    }

    [TestCase(0d, TestName = "Convert_BeforeLayoutHasMeasured_IsUnset")]
    [TestCase(-10d, TestName = "Convert_WithANegativeWidth_IsUnset")]
    [TestCase(double.NaN, TestName = "Convert_WithAnUnsetWidth_IsUnset")]
    [TestCase(double.PositiveInfinity, TestName = "Convert_WithAnInfiniteWidth_IsUnset")]
    public void Convert_WithAnUnusableWidth_ReturnsUnset(double width) {

        // Act
        var result = Convert(width, "16:9");

        // Assert — returning 0 here would collapse the box on the first layout pass
        Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue), "An unusable width should leave the height unset");

    }

    [TestCase(null, TestName = "Convert_WithNoRatio_IsUnset")]
    [TestCase("", TestName = "Convert_WithAnEmptyRatio_IsUnset")]
    [TestCase("16", TestName = "Convert_WithAMalformedRatio_IsUnset")]
    [TestCase("16:9:4", TestName = "Convert_WithAnOverlongRatio_IsUnset")]
    [TestCase("16:banana", TestName = "Convert_WithANonNumericRatio_IsUnset")]
    [TestCase("16:0", TestName = "Convert_WithAZeroHeight_IsUnset")]
    [TestCase("0:9", TestName = "Convert_WithAZeroWidth_IsUnset")]
    public void Convert_WithAnUnusableRatio_ReturnsUnset(string? ratio) {

        // Act
        var result = Convert(640d, ratio);

        // Assert
        Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue), "An unusable ratio should leave the height unset");

    }

    [Test]
    public void Convert_WithAnUnboundWidth_ReturnsUnset() {

        // Act — what a binding supplies before its source has produced a value
        var result = Convert(DependencyProperty.UnsetValue, "16:9");

        // Assert
        Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue), "An unbound width should leave the height unset");

    }

    [Test]
    public void ConvertBack_IsNotSupported() {

        // Act & Assert
        Assert.Throws<NotSupportedException>(
            () => _converter.ConvertBack(360d, typeof(double), "16:9", CultureInfo.InvariantCulture),
            "A height does not imply a width");

    }

}
