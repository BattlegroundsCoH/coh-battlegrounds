using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Controls;

public class BusyDots : Control {

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(BusyDots),
            new FrameworkPropertyMetadata(false));

    public bool IsActive {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    static BusyDots() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BusyDots), new FrameworkPropertyMetadata(typeof(BusyDots)));
    }

}
