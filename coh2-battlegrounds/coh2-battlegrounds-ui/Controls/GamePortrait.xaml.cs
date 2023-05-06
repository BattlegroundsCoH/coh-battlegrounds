using System.Linq;
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
        = DependencyProperty.Register(nameof(PortraitName), typeof(object), typeof(GamePortrait), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GamePortrait>(p => p.PortraitName = b.NewValue)));

    /// <summary>
    /// Get or set portrait name to display
    /// </summary>
    public object? PortraitName {
        get => this.GetValue(PortraitNameProperty);
        set {
            this.SetValue(PortraitNameProperty, value);
            this.TrySetPortrait();
        }
    }

    /// <summary>
    /// Identifies the <see cref="SymbolName"/> property.
    /// </summary>
    public static readonly DependencyProperty SymbolNameProperty
        = DependencyProperty.Register(nameof(SymbolName), typeof(object), typeof(GamePortrait), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GamePortrait>(p => p.SymbolName = b.NewValue)));

    /// <summary>
    /// Get or set name of symbol to display
    /// </summary>
    public object? SymbolName {
        get => this.GetValue(SymbolNameProperty);
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
        => TrySet(PortraitImage, ResolveResource(PORTRAIT_SOURCE, PortraitName));

    private void TrySetIcon()
        => TrySet(SymbolImage, ResolveResource(SYMBOL_SOURCE, SymbolName));

    private static IIconSource? ResolveResource(string container, object? identifier) {
        if (identifier is IIconSource iconSource) {
            return iconSource;
        }
        if (identifier is string str) {
            return new DefaultIconSource(container, str);
        }
        return null;
    }

    private static void TrySet(Image img, IIconSource? iconSource) {

        // Do nothing if name is not valid
        if (iconSource is null) {
            return;
        }

        // do nothing if icon is invalid
        if (!ResourceHandler.HasIcon(iconSource)) {
            return;
        }

        // Set source
        img.Source = ResourceHandler.GetIcon(iconSource);

    }

    private void RefreshMask() {
        bool selected = this.IsSelected;
        this.MaskRect.Fill = selected ? Brushes.Transparent : this.MaskColour;
        this.MaskRect.Opacity = selected ? 0 : 0.25; // TODO: Expose these two "magic" numbers
    }


}
