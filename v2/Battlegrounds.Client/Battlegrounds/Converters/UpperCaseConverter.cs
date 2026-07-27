using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

/// <summary>
/// Uppercases a bound value for display.
/// </summary>
/// <remarks>
/// The design system sets headings, labels and button captions in uppercase. Static strings
/// are authored that way directly in XAML; this exists for the bound ones — faction names,
/// game titles, company names in a section heading — where the source value is mixed case
/// and only the presentation should shout.
/// <para>
/// <c>TextBlock</c> has no <c>CharacterCasing</c> property (only <c>TextBox</c> does), so
/// there is no built-in equivalent.
/// </para>
/// <para>
/// Casing uses the supplied culture rather than the invariant culture, because the app is
/// localised (English, German, French, Polish) and casing is language-sensitive.
/// </para>
/// </remarks>
public sealed class UpperCaseConverter : IValueConverter {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString()?.ToUpper(culture);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(UpperCaseConverter)} is one-way; casing cannot be reversed.");

}
