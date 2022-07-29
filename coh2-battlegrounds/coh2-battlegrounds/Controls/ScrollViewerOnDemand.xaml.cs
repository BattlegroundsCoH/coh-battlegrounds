using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Functional;

namespace BattlegroundsApp.Controls;

/// <summary>
/// Interaction logic for ScrollViewerOnDemand.xaml
/// </summary>
public partial class ScrollViewerOnDemand : UserControl {

    public ScrollViewerOnDemand() {
        InitializeComponent();
    }

    public new object Content {
        get => this.GetValue(ContentProperty);
        set {
            this.SetCurrentValue(ContentProperty, value);
            this.__ScrollBar.Content = value;
            this.__DefPresenter.Content = value;
        }
    }

    public static readonly DependencyProperty IsScrollbarVisibleProperty =
        DependencyProperty.Register(nameof(IsScrollbarVisible), typeof(bool), typeof(ScrollViewerOnDemand),
            new FrameworkPropertyMetadata(false, OnScrollbarVisiblePropertyChanged));

    private static void OnScrollbarVisiblePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs) {
        if (obj is ScrollViewerOnDemand vdm) {
            vdm.IsScrollbarVisible = (bool)eventArgs.NewValue;
        }
    }

    public bool IsScrollbarVisible {
        get => (bool)this.GetValue(IsScrollbarVisibleProperty);
        set {
            this.SetCurrentValue(IsScrollbarVisibleProperty, value);
            if (value) {
                this.__ScrollBar.Visibility = Visibility.Visible;
                this.__DefPresenter.Visibility = Visibility.Collapsed;
            } else {
                this.__ScrollBar.Visibility = Visibility.Collapsed;
                this.__DefPresenter.Visibility = Visibility.Visible;
            }
        }
    }

}
