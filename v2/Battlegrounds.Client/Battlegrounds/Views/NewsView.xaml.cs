using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Views;

public partial class NewsView : UserControl {

    public NewsView(ViewModels.NewsViewModel viewModel) {
        DataContext = viewModel;
        InitializeComponent();
    }

    private void NewsView_Initialized(object? sender, EventArgs e) {
        if (DataContext is ViewModels.NewsViewModel vm) {
            vm.OnViewActivated();
        }
    }

    /// <summary>
    /// Runs the automatic refresh only while the page is actually on screen.
    /// </summary>
    private void NewsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if (DataContext is not ViewModels.NewsViewModel vm) {
            return;
        }
        if (IsVisible) {
            vm.StartAutoRefresh();
        } else {
            vm.StopAutoRefresh();
        }
    }

}
