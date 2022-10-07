using System;
using System.Globalization;
using System.Windows.Data;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;

namespace Battlegrounds.UI.Converters.Locale;

public class FactionToAlliance : IValueConverter {

    public LocaleKey Alliance { get; set; }
    public LocaleKeyString Convertor { get; set; }

    public FactionToAlliance() {
        Convertor = new LocaleKeyString();
        Alliance = new LocaleKey("FactionToAllianceConverter_Unkown");
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        if (value is Faction faction) {

            Alliance = faction.IsAllied ? new LocaleKey("FactionToAllianceConverter_Allies") : new LocaleKey("FactionToAllianceConverter_Axis");

            return Convertor.Convert(Alliance, typeof(string), null, null);

        }

        return Convertor.Convert(Alliance, typeof(string), null, null); ;

    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }

}
