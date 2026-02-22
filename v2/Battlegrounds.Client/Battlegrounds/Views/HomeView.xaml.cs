using System.Windows.Controls;

namespace Battlegrounds.Views;

public partial class HomeView : UserControl {
    
    public HomeView(ViewModels.HomeViewModel viewModel) {
        InitializeComponent();
        DataContext = viewModel;
    }

}
