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
        [Faction.Soviet] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/soviet.png")),
        [Faction.Wehrmacht] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/german.png")),
        [Faction.America] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/aef.png")),
        [Faction.OberkommandoWest] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/west_german.png")),
        [Faction.British] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/british.png")),
        [Faction.BritishAfrica] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/british_africa.png")),
        [Faction.AfrikaKorps] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/afrika_korps.png")),
        [Faction.Germans] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/german_ugly.png")),
        [Faction.Americans] = new BitmapImage(new Uri("pack://application:,,,/Battlegrounds;component/Resources/ingame/american.png"))
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        // Return flag for faction directly
        if (value is Faction faction) {
            return Flags[faction];
        }

        // Get faction from string (if string)
        faction = value is string fstr 
            ? Faction.FromName(fstr, Faction.TryGetGameFromFactionName(fstr)) 
            : throw new ArgumentException("Expected string parameter, but found none.", nameof(value));

        // Return flags
        return Flags[faction];

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

    public static ImageSource GetIcon(Faction faction) => Flags[faction];

}
