using System.Windows.Controls;

namespace Battlegrounds.Lobby.Pages;

/// <summary>
/// Interaction logic for LobbyBrowserView.xaml
/// </summary>
public partial class LobbyBrowserView : UserControl {

    public LobbyBrowserView() {
        InitializeComponent();
    }

    private void GameLobbyList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (this.DataContext is LobbyBrowser vm) {
            vm.RefreshJoin();
        }
    }

}
