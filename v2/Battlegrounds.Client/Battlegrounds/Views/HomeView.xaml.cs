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

}
