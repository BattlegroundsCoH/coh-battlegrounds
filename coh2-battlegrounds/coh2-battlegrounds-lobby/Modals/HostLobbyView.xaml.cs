using System.Windows;
using System.Windows.Controls;

using Battlegrounds.UI.Modals;

namespace Battlegrounds.Lobby.Modals;

/// <summary>
/// Interaction logic for HostLobbyView.xaml
/// </summary>
public partial class HostLobbyView : Modal {

    public HostLobbyView() {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e) {
        if (this.DataContext is HostLobby vm) {
            vm.LobbyPassword = ((PasswordBox)sender).Password;
        }
    }

}
