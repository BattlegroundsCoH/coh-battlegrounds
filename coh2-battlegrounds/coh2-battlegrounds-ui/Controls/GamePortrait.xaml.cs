﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Functional;
using Battlegrounds.Resources;

namespace Battlegrounds.UI.Controls;

/// <summary>
/// Interaction logic for GamePortrait.xaml
/// </summary>
public partial class GamePortrait : UserControl {

    /// <summary>
    /// Clip dimensions (Currently only for storage purposes...)
    /// </summary>
    public static readonly Rect PortraitClip = new Rect(28, 16, 92, 140);

    /// <summary>
    /// Source to extract portraits from
    /// </summary>
    public const string PORTRAIT_SOURCE = "portraits";

    /// <summary>
    /// Source to extract symbols from
    /// </summary>
    public const string SYMBOL_SOURCE = "symbol_icons";

    /// <summary>
    /// Identifies the <see cref="PortraitName"/> property.
    /// </summary>
    public static readonly DependencyProperty PortraitNameProperty
        = DependencyProperty.Register(nameof(PortraitName), typeof(string), typeof(GamePortrait), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GamePortrait>(p => p.PortraitName = b.NewValue as string)));

    /// <summary>
    /// Get or set portrait name to display
    /// </summary>
    public string? PortraitName {
        get => this.GetValue(PortraitNameProperty) as string;
        set {
            this.SetValue(PortraitNameProperty, value);
            this.TrySetPortrait();
        }
    }

    /// <summary>
    /// Identifies the <see cref="SymbolName"/> property.
    /// </summary>
    public static readonly DependencyProperty SymbolNameProperty
        = DependencyProperty.Register(nameof(SymbolName), typeof(string), typeof(GamePortrait), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GamePortrait>(p => p.SymbolName = b.NewValue as string)));

    /// <summary>
    /// Get or set name of symbol to display
    /// </summary>
    public string? SymbolName {
        get => this.GetValue(SymbolNameProperty) as string;
        set {
            this.SetValue(SymbolNameProperty, value);
            this.TrySetIcon();
        }
    }

    /// <summary>
    /// Identifies the <see cref="MaskColour"/> property.
    /// </summary>
    public static readonly DependencyProperty MaskColourProperty
        = DependencyProperty.Register(nameof(MaskColour), typeof(Brush), typeof(GamePortrait), new PropertyMetadata(Brushes.LightGray));

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
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(GamePortrait),
            new PropertyMetadata(false, (a, b) => a.Cast<GamePortrait>(p => p.IsSelected = (bool)b.NewValue)));

    public bool IsSelected {
        get => (bool)this.GetValue(IsSelectedProperty);
        set {
            this.SetValue(IsSelectedProperty, value);
            this.RefreshMask();
        }
    }

    public GamePortrait() {
        InitializeComponent();
    }

    private void TrySetPortrait()
        => TrySet(PortraitImage, PORTRAIT_SOURCE, PortraitName ?? string.Empty);

    private void TrySetIcon()
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