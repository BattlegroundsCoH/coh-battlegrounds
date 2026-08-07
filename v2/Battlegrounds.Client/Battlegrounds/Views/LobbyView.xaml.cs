using System.Windows.Controls;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for LobbyView.xaml
/// </summary>
public partial class LobbyView : UserControl {

    public LobbyView(LobbyViewModel viewModel) {
        InitializeComponent();
        DataContext = viewModel;
        Unloaded += OnUnloaded;
    }

    private async void OnUnloaded(object sender, System.Windows.RoutedEventArgs e) {
        Unloaded -= OnUnloaded;
        if (DataContext is LobbyViewModel viewModel) {
            await viewModel.DisposeAsync();
        }
    }
}
