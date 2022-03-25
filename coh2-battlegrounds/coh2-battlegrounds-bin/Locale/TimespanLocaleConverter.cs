using System;

namespace Battlegrounds.Locale;

/// <summary>
/// Converter class for converting a <see cref="TimeSpan"/> into a localised string. This class cannot be inherited.
/// </summary>
public sealed class TimespanLocaleConverter : LocaleConverter<TimeSpan> {

    /// <summary>
    /// Get the localised string representation of the given <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="localize">The <see cref="Localize"/> instance to pull localised string data from.</param>
    /// <param name="localeObject">The <see cref="TimeSpan"/> to localise.</param>
    /// <returns>The localised string representation of the <see cref="TimeSpan"/> instance.</returns>
    public override string GetLocalisedString(Localize locale, TimeSpan localeObject) {
        if (localeObject.Hours > 0)
            return locale.GetString("TimeSpan_HHMMSS", localeObject.Hours.ToString("00"), localeObject.Minutes.ToString("00"), localeObject.Seconds.ToString("00"));
        else if (localeObject.Minutes > 0)
            return locale.GetString("TimeSpan_MMSS", localeObject.Minutes.ToString("00"), localeObject.Seconds.ToString("00"));
        else
            return locale.GetString("TimeSpan_SS", localeObject.Seconds.ToString("00"));
    }

}
