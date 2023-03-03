using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Battlegrounds.Editor.Components;

public record SquadSlotViewClickEventArgs(object Sender);

/// <summary>
/// Interaction logic for SquadSlotView.xaml
/// </summary>
public partial class SquadSlotView : UserControl {

    private Brush VIEW_DEFAULT => this.DataContext switch {
        SquadSlot ssvm => ssvm.PhaseBackground,
        _ => (SolidColorBrush)Application.Current.FindResource("BackgroundLightBlueBrush")
    };

    private Brush VIEW_HOVER => this.DataContext switch {
        SquadSlot ssvm => ssvm.PhaseBackgroundHover,
        _ => (SolidColorBrush)Application.Current.FindResource("BackgroundLightGrayBrush")
    };

    public SquadSlotView() {
        InitializeComponent();
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        this.Background = VIEW_HOVER;
        this.PortraitElement.IsSelected = true;
        this.RemoveButton.Visibility = Visibility.Visible;
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        this.Background = VIEW_DEFAULT;
        this.PortraitElement.IsSelected = false;
        this.RemoveButton.Visibility = Visibility.Collapsed;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e) {
        base.OnMouseDown(e);
        if (this.DataContext is SquadSlot vm) {
            vm.Click?.Invoke(this, vm);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e) {
        if (this.DataContext is SquadSlot vm) {
            vm.RemoveClick?.Invoke(this, vm);
        }
    }

}
