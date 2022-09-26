using System;
using System.Globalization;
using System.Windows.Data;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;

namespace BattlegroundsApp.Utilities.Converters;

public class FactionToAllianceConverter : IValueConverter {

    public LocaleKey Alliance { get; set; }
    public LocaleKeyToStringConverter Convertor { get; set; }

    public FactionToAllianceConverter() {
        Convertor = new LocaleKeyToStringConverter();
        Alliance = new LocaleKey("FactionToAllianceConverter_Unkown");
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        if (value is Faction faction) {

            Alliance = faction.IsAllied ? new LocaleKey("FactionToAllianceConverter_Allies") : new LocaleKey("FactionToAllianceConverter_Axis");

            return Convertor.Convert(Alliance, typeof(String), null, null);

        }

        return Convertor.Convert(Alliance, typeof(String), null, null); ;

    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }

}
