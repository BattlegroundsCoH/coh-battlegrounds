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
    }

}
