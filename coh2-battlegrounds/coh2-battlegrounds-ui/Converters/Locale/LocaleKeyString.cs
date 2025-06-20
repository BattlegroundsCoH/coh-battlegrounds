﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Battlegrounds.Locale;

namespace Battlegrounds.UI.Converters.Locale;

public sealed class LocaleKeyString : IValueConverter {

    public static readonly DependencyProperty LocalizerProperty = DependencyProperty.Register(nameof(Localizer), typeof(Localize), typeof(LocaleKeyString));

    public Localize? Localizer { get; set; }

    public object? Convert(object value, Type targetType, object? parameter, CultureInfo? culture) {
        if (value is string s) {
            return s;
        } else if (value is LocaleKey key) {
            if (this.Localizer is not null) {
                if (this.Localizer.GetString(key) is string localeString && localeString != key.LocaleID) {
                    return localeString;
                }
            }
            return BattlegroundsContext.Localize.GetString(key);
        } else if (value is Enum e) {
            // TODO: Implement
        }
        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null; // Not allowed/possible


}
