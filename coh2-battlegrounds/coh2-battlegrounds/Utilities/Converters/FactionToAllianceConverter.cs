using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Utilities.Converters {
    public class FactionToAllianceConverter : IValueConverter {

        public string Alliance { get; set; }

        public FactionToAllianceConverter() {
            Alliance = "Unknown";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is Faction faction) {

                if (faction.IsAllied) {

                    Alliance = "Alies";
                    return Alliance;

                } else {

                    Alliance = "Axis";
                    return Alliance;

                }

            }

            return string.Empty;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
