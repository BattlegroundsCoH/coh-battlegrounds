using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.DataCompany;

namespace BattlegroundsApp.Utilities.Converters {

    public class StringToCompanyTypeIconConverter : IValueConverter {

        private static readonly Dictionary<string, ImageSource> Icons = new() {
            [nameof(CompanyType.Infantry)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_infantry.png")),
            [nameof(CompanyType.Armoured)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_armoured.png")),
            [nameof(CompanyType.Motorized)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_motorized.png")),
            [nameof(CompanyType.Mechanized)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_mechanized.png")),
            [nameof(CompanyType.Airborne)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_airborne.png")),
            [nameof(CompanyType.Artillery)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_artillery.png")),
            [nameof(CompanyType.TankDestroyer)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_td.png")),
            [nameof(CompanyType.Engineer)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_engineer.png")),
            [nameof(CompanyType.Unspecified)] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_unspecified.png")),
            [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/company_types/ct_unspecified.png"))
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is CompanyType t) {
                return Icons.GetValueOrDefault(t.ToString(), Icons[string.Empty]);
            } else if (value is string icoType) {
                return Icons.GetValueOrDefault(icoType, Icons[string.Empty]);
            }
            throw new ArgumentException("Invalid converter argument.", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

    }

}
