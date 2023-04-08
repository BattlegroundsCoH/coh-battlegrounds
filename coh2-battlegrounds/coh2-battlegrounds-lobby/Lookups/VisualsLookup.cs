using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Lobby.Lookups;

/// <summary>
/// Static utility class for lookup up embedded visual resources.
/// </summary>
public static class VisualsLookup {

    /// <summary>
    /// Readonly dictionary of faction icons.
    /// </summary>
    public static readonly Dictionary<string, ImageSource> FactionIcons = new() {
        [Faction.Soviet.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIET.png")),
        [Faction.America.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionAEF.png")),
        [Faction.British.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionBRIT.png")),
        [Faction.OberkommandoWest.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionOKW.png")),
        [Faction.Wehrmacht.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionWEHR.png")),
        ["?"] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIET.png")),
        [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionLOCKED.png")),
    };

    /// <summary>
    /// Readonly dictionary of highlighted versions of faction icons.
    /// </summary>
    public static readonly Dictionary<string, ImageSource> FactionHoverIcons = new() {
        [Faction.Soviet.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIETHighlighted.png")),
        [Faction.America.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionAEFHighlighted.png")),
        [Faction.British.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionBRITHighlighted.png")),
        [Faction.OberkommandoWest.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionOKWHighlighted.png")),
        [Faction.Wehrmacht.Name] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionWEHRHighlighted.png")),
        ["?"] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionSOVIETHighlighted.png")),
        [string.Empty] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/army_icons/FactionLOCKEDHighlighted.png")),
    };

    /// <summary>
    /// Readonly array of objective type icons.
    /// </summary>
    public static readonly ImageSource[] ObjectiveTypes = new[] {
        new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/objective_icons/ot_attack.png")),
        new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/objective_icons/ot_defend.png")),
        new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/objective_icons/ot_support.png")),
    };

    /// <summary>
    /// Get the Locale String for downloading
    /// </summary>
    public static readonly Func<string, string> LOCSTR_DOWNLOAD = x => BattlegroundsContext.Localize.GetString("LobbyView_DownloadGamemode", x);

    /// <summary>
    /// Get the locale string for playing
    /// </summary>
    public static readonly Func<string> LOCSTR_PLAYING = () => BattlegroundsContext.Localize.GetString("LobbyView_PLAYING");

}
