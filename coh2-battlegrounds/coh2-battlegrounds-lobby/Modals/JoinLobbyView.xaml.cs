using System.Windows;
using System.Windows.Controls;

using Battlegrounds.UI.Modals;

namespace Battlegrounds.Lobby.Modals;

/// <summary>
/// Interaction logic for JoinLobbyView.xaml
/// </summary>
public partial class JoinLobbyView : Modal {

    public JoinLobbyView() {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e) {
        if (this.DataContext is JoinLobby vm) {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }

}
