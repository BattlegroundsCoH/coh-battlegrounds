using System;
using System.Collections.Generic;
using System.ComponentModel;
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
/// Interaction logic for Portrait.xaml
/// </summary>
public partial class Portrait : UserControl, INotifyPropertyChanged {

    /// <summary>
    /// Clip dimensions (Currently only for storage purposes...)
    /// </summary>
    public static readonly Rect PortraitClip = new Rect(28, 16, 92, 140);

    /// <summary>
    /// Source to extract portraits from
    /// </summary>
    public const string PORTRAIT_SOURCE = "portrait_icons"; // TODO: Improved...

    /// <summary>
    /// Source to extract symbols from
    /// </summary>
    public const string SYMBOL_SOURCE = "symbol_icons";

    /// <summary>
    /// Identifies the <see cref="PortraitName"/> property.
    /// </summary>
    public static readonly DependencyProperty PortraitNameProperty
        = DependencyProperty.Register(nameof(PortraitName), typeof(string), typeof(Portrait), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => (a as Portrait).PortraitName = b.NewValue as string));

    /// <summary>
    /// Get or set portrait name to display
    /// </summary>
    public string PortraitName {
        get => this.GetValue(PortraitNameProperty) as string;
        set {
            this.SetValue(PortraitNameProperty, value);
            this.TrySetPortrait();
            this.PropertyChanged?.Invoke(this, new(nameof(PortraitName)));
        }
    }

    /// <summary>
    /// Identifies the <see cref="SymbolName"/> property.
    /// </summary>
    public static readonly DependencyProperty SymbolNameProperty
        = DependencyProperty.Register(nameof(SymbolName), typeof(string), typeof(Portrait), new FrameworkPropertyMetadata(
            string.Empty,
            FrameworkPropertyMetadataOptions.AffectsRender,
            (a, b) => (a as Portrait).SymbolName = b.NewValue as string));

    /// <summary>
    /// Get or set name of symbol to display
    /// </summary>
    public string SymbolName {
        get => this.GetValue(SymbolNameProperty) as string;
        set {
            this.SetValue(SymbolNameProperty, value);
            this.TrySetIcon();
            this.PropertyChanged?.Invoke(this, new(nameof(PortraitName)));
        }
    }

    /// <summary>
    /// Identifies the <see cref="PortraitSourceProperty"/> property.
    /// </summary>
    public static readonly DependencyProperty PortraitSourceProperty = DependencyProperty.Register(nameof(PortraitSource), typeof(ImageSource), typeof(Portrait));

    /// <summary>
    /// Get portrait image source
    /// </summary>
    public ImageSource PortraitSource => (ImageSource)this.GetValue(PortraitSourceProperty);

    /// <summary>
    /// Identifies the <see cref="SymbolSourceProperty"/> property.
    /// </summary>
    public static readonly DependencyProperty SymbolSourceProperty = DependencyProperty.Register(nameof(SymbolSource), typeof(ImageSource), typeof(Portrait));

    /// <summary>
    /// Get symbol image source
    /// </summary>
    public ImageSource SymbolSource => (ImageSource)this.GetValue(SymbolSourceProperty);

    /// <summary>
    /// Identifies the <see cref="MaskOpacity"/> property.
    /// </summary>
    public static readonly DependencyProperty MaskOpacityProperty = DependencyProperty.Register(nameof(MaskOpacity), typeof(double), typeof(Portrait), new(1.0));

    /// <summary>
    /// Get the opacity of the mask
    /// </summary>
    public double MaskOpacity => (double)this.GetValue(MaskOpacityProperty);

    /// <summary>
    /// Identifies the <see cref="MaskColour"/> property.
    /// </summary>
    public static readonly DependencyProperty MaskColourProperty = DependencyProperty.Register(nameof(MaskColour), typeof(Brush), typeof(Portrait));

    /// <summary>
    /// Get the colour of the mask.
    /// </summary>
    public Brush MaskColour => this.GetValue(MaskColourProperty) as Brush;

    /// <summary>
    /// Get or set if hover highlight effect is enabled (TODO: Something else for this class...)
    /// </summary>
    public bool HoverHighlight { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public Portrait() {
        this.InitializeComponent();
    }

    private void TrySetPortrait()
        => this.TrySet(PortraitSourceProperty, PORTRAIT_SOURCE, PortraitName, nameof(PortraitSource));

    private void TrySetIcon()
        => this.TrySet(SymbolSourceProperty, SYMBOL_SOURCE, SymbolName, nameof(SymbolSource));

    private void TrySet(DependencyProperty dp, string source, string name, string target) {

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
        this.SetValue(dp, App.ResourceHandler.GetIcon(source, name));
        this.PropertyChanged?.Invoke(this, new(target));

    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        if (this.HoverHighlight) {
            this.SetValue(MaskOpacityProperty, 0.075);
            this.PropertyChanged?.Invoke(this, new(nameof(this.MaskColour)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.MaskOpacity)));
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        if (this.HoverHighlight) {
            this.SetValue(MaskOpacityProperty, 0.6);
            this.PropertyChanged?.Invoke(this, new(nameof(this.MaskColour)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.MaskOpacity)));
        }
    }

}
