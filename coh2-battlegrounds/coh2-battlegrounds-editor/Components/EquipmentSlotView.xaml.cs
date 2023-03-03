using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Battlegrounds.Editor.Components;

/// <summary>
/// Interaction logic for EquipmentSlotView.xaml
/// </summary>
public partial class EquipmentSlotView : UserControl {

    public static readonly Brush VIEW_DEFAULT = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334252"));
    public static readonly Brush VIEW_HOVER = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#536375"));

    public EquipmentSlotView() {
        InitializeComponent();
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        this.Background = VIEW_HOVER;
        this.PortraitElement.IsSelected = true;
        this.EquipButton.Visibility = Visibility.Visible;
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        this.Background = VIEW_DEFAULT;
        this.PortraitElement.IsSelected = false;
        this.EquipButton.Visibility = Visibility.Collapsed;
    }

}
