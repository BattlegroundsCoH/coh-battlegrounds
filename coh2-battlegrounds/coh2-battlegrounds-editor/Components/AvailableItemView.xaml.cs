using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Battlegrounds.Editor.Components;

/// <summary>
/// Interaction logic for AvailableItemView.xaml
/// </summary>
public partial class AvailableItemView : UserControl {

    public static readonly Brush VIEW_DEFAULT = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334252"));
    public static readonly Brush VIEW_HOVER = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#536375"));

    public AvailableItemView() {
        InitializeComponent();
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        this.Background = VIEW_HOVER;
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        this.Background = VIEW_DEFAULT;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        if (this.DataContext is AvailableItem vm) {
            vm.Move?.Invoke(this, vm, e);
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e) {
        if (this.DataContext is AvailableItem vm) {
            vm.AddClick?.Invoke(this, vm);
        }
    }

}
