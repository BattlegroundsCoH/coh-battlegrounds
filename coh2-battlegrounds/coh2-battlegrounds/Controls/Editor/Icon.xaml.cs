using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Controls.Editor;
/// <summary>
/// Interaction logic for Icon.xaml
/// </summary>
public partial class Icon : UserControl {

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
        = DependencyProperty.Register(nameof(IconName), typeof(string), typeof(Icon), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => (a as Icon).IconName = b.NewValue as string));

    /// <summary>
    /// Get or set icon name to display
    /// </summary>
    public string IconName {
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
        = DependencyProperty.Register(nameof(SymbolName), typeof(string), typeof(Icon), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => (a as Icon).SymbolName = b.NewValue as string));

    /// <summary>
    /// Get or set name of symbol to display
    /// </summary>
    public string SymbolName {
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
        = DependencyProperty.Register(nameof(MaskColour), typeof(Brush), typeof(Icon), new PropertyMetadata(Brushes.LightGray));

    /// <summary>
    /// Get the colour of the mask.
    /// </summary>
    public Brush MaskColour {
        get => this.GetValue(MaskColourProperty) as Brush;
        set {
            this.SetValue(MaskColourProperty, value);
            this.RefreshMask();
        }
    }

    /// <summary>
    /// Identifies the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(Icon));

    public bool IsSelected {
        get => (bool)this.GetValue(IsSelectedProperty);
        set {
            this.SetValue(IsSelectedProperty, value);
            this.RefreshMask();
        }
    }

    public Icon() {
        InitializeComponent();
        this.RefreshMask();
    }

    private void TrySetIcon()
        => TrySet(IconImage, ICON_SOURCE, IconName);

    private void TrySetSymbol()
        => TrySet(SymbolImage, SYMBOL_SOURCE, SymbolName);

    private static void TrySet(Image img, string source, string name) {

        // Do nothing if name is not valid
        if (string.IsNullOrEmpty(name)) {
            return;
        }

        // If resource handler is missing for some reason, bail
        if (App.ResourceHandler is null) {
            return;
        }

        // do nothing if icon is invalid
        if (!App.ResourceHandler.HasIcon(source, name)) {
            return;
        }

        // Set source
        img.Source = App.ResourceHandler.GetIcon(source, name);

    }

    private void RefreshMask() {
        bool selected = this.IsSelected;
        this.MaskRect.Fill = selected ? Brushes.Transparent : this.MaskColour;
        this.MaskRect.Opacity = selected ? 0 : 0.25; // TODO: Expose these two "magic" numbers
    }

}
