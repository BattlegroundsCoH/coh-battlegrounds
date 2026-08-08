using System.Globalization;
using System.Windows;

using Battlegrounds.Converters;
using Battlegrounds.ViewModels.LobbyHelpers;

namespace Battlegrounds.Tests.Converters;

[TestFixture]
public class ValidCompanyVisibilityConverterTests {

    private static readonly ValidCompanyVisibilityConverter converter = new ValidCompanyVisibilityConverter();

    [Test]
    public void Convert_NullValue_ReturnsCollapsed() {
        var result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_PickableCompanyIsNoneTrue_ReturnsCollapsed() {
        var company = new PickableCompany(IsNone: true, GenerateRandom: false, Company: null, ShowCompanyPreviewCommand: null!);
        var result = converter.Convert(company, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_PickableCompanyIsNoneFalse_ReturnsVisible() {
        var company = new PickableCompany(IsNone: false, GenerateRandom: false, Company: null, ShowCompanyPreviewCommand: null!);
        var result = converter.Convert(company, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_PickableCompanyGenerateRandomTrue_ReturnsCollapsed() {
        var company = new PickableCompany(IsNone: false, GenerateRandom: true, Company: null, ShowCompanyPreviewCommand: null!);
        var result = converter.Convert(company, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    public void Convert_NonPickableCompanyValue_ReturnsVisible() {
        var result = converter.Convert("some other value", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void ConvertBack_AnyValue_ThrowsNotImplementedException() {
        Assert.Throws<NotImplementedException>(() =>
            converter.ConvertBack(Visibility.Visible, typeof(object), null!, CultureInfo.InvariantCulture));
    }
}
