using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Views;

public partial class HomeView : UserControl {

    public HomeView(ViewModels.HomeViewModel viewModel) {
        DataContext = viewModel;
        InitializeComponent();
    }

    private void HomeView_Initialized(object? sender, EventArgs e) {
        if (DataContext is ViewModels.HomeViewModel vm) {
            vm.OnViewActivated();
        }
    }

    /// <summary>
    /// Runs the news card's automatic refresh only while the dashboard is actually on screen.
    /// </summary>
    /// <remarks>Visibility rather than <c>Loaded</c>/<c>Unloaded</c>, because the nav area is
    /// <i>collapsed</i> — not unloaded — while a lobby or match view is open, so an unload hook would
    /// leave the dashboard polling from behind it. This fires for both cases.</remarks>
    private void HomeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if (DataContext is not ViewModels.HomeViewModel vm) {
            return;
        }
        if (IsVisible) {
            vm.StartNewsAutoRefresh();
        } else {
            vm.StopNewsAutoRefresh();
        }
    }

}
