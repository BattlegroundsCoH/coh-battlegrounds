﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Functional;
using Battlegrounds.Resources;

namespace Battlegrounds.UI.Controls;
/// <summary>
/// Interaction logic for GameIcon.xaml
/// </summary>
public partial class GameIcon : UserControl {

    /// <summary>
    /// Source to extract portraits from
    /// </summary>
    public const string ICON_SOURCE = "ability_icons";

    /// <summary>
    /// Source to extract symbols from
    /// </summary>
    public const string SYMBOL_SOURCE = "symbol_icons";

    /// <summary>
    /// Identifies the <see cref="IconName"/> property.
    /// </summary>
    public static readonly DependencyProperty IconNameProperty
        = DependencyProperty.Register(nameof(IconName), typeof(string), typeof(GameIcon), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GameIcon>(i => i.IconName = b.NewValue as string)));

    /// <summary>
    /// Get or set icon name to display
    /// </summary>
    public string? IconName {
        get => this.GetValue(IconNameProperty) as string;
        set {
            this.SetValue(IconNameProperty, value);
            this.TrySetIcon();
        }
    }

    /// <summary>
    /// Identifies the <see cref="SymbolName"/> property.
    /// </summary>
    public static readonly DependencyProperty SymbolNameProperty
        = DependencyProperty.Register(nameof(SymbolName), typeof(string), typeof(GameIcon), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GameIcon>(i => i.SymbolName = b.NewValue as string)));

    /// <summary>
    /// Get or set name of symbol to display
    /// </summary>
    public string? SymbolName {
        get => this.GetValue(SymbolNameProperty) as string;
        set {
            this.SetValue(SymbolNameProperty, value);
            this.TrySetSymbol();
        }
    }

    /// <summary>
    /// Identifies the <see cref="MaskColour"/> property.
    /// </summary>
    public static readonly DependencyProperty MaskColourProperty
        = DependencyProperty.Register(nameof(MaskColour), typeof(Brush), typeof(GameIcon), new PropertyMetadata(Brushes.LightGray));

    /// <summary>
    /// Get the colour of the mask.
    /// </summary>
    public Brush? MaskColour {
        get => this.GetValue(MaskColourProperty) as Brush;
        set {
            this.SetValue(MaskColourProperty, value);
            this.RefreshMask();
        }
    }

    /// <summary>
    /// Identifies the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(GameIcon));

    /// <summary>
    /// 
    /// </summary>
    public bool IsSelected {
        get => (bool)this.GetValue(IsSelectedProperty);
        set {
            this.SetValue(IsSelectedProperty, value);
            this.RefreshMask();
        }
    }

    public GameIcon() {
        this.InitializeComponent();
        this.RefreshMask();
    }

    private void TrySetIcon()
        => TrySet(IconImage, ICON_SOURCE, IconName ?? string.Empty);

    private void TrySetSymbol()
        => TrySet(SymbolImage, SYMBOL_SOURCE, SymbolName ?? string.Empty);

    private static void TrySet(Image img, string source, string name) {

        // Do nothing if name is not valid
        if (string.IsNullOrEmpty(name)) {
            return;
        }

        // do nothing if icon is invalid
        if (!ResourceHandler.HasIcon(source, name)) {
            return;
        }

        // Set source
        img.Source = ResourceHandler.GetIcon(source, name);

    }

    private void RefreshMask() {
        bool selected = this.IsSelected;
        this.MaskRect.Fill = selected ? Brushes.Transparent : this.MaskColour;
        this.MaskRect.Opacity = selected ? 0 : 0.25; // TODO: Expose these two "magic" numbers
    }

}