using System.Linq;
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
        = DependencyProperty.Register(nameof(IconName), typeof(object), typeof(GameIcon), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GameIcon>(i => i.IconName = b.NewValue)));

    /// <summary>
    /// Get or set icon name to display
    /// </summary>
    public object? IconName {
        get => this.GetValue(IconNameProperty);
        set {
            this.SetValue(IconNameProperty, value);
            this.TrySetIcon();
        }
    }

    /// <summary>
    /// Identifies the <see cref="SymbolName"/> property.
    /// </summary>
    public static readonly DependencyProperty SymbolNameProperty
        = DependencyProperty.Register(nameof(SymbolName), typeof(object), typeof(GameIcon), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => a.Cast<GameIcon>(i => i.SymbolName = b.NewValue)));

    /// <summary>
    /// Get or set name of symbol to display
    /// </summary>
    public object? SymbolName {
        get => this.GetValue(SymbolNameProperty);
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
        => TrySet(IconImage, ResolveResource(ICON_SOURCE, IconName));

    private void TrySetSymbol()
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
