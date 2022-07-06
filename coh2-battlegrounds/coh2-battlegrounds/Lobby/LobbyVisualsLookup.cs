using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Lobby;

/// <summary>
/// Static utility class for lookup up embedded visual resources.
/// </summary>
public static class LobbyVisualsLookup {

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
        new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/campaign/obj_main.png")),
        new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/campaign/obj_secondary.png")),
        new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/campaign/obj_star.png")),
    };

}
