using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.UI.Converters.Icons;

public sealed class StringToFactionIcon : IValueConverter {

    private static readonly Dictionary<Faction, ImageSource> Flags = new() {
        [Faction.Soviet] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/soviet.png")),
        [Faction.Wehrmacht] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/german.png")),
        [Faction.America] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/aef.png")),
        [Faction.OberkommandoWest] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/west_german.png")),
        [Faction.British] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/british.png"))
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        // Return flag for faction directly
        if (value is Faction faction) {
            return Flags[faction];
        }

        // Get faction from string (if string)
        faction = value is string fstr ? Faction.FromName(fstr) : throw new ArgumentException("Expected string parameter, but found none.", nameof(value));

        // Return flags
        return Flags[faction];

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

    public static ImageSource GetIcon(Faction faction) => Flags[faction];

}
