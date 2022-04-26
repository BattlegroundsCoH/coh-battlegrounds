using System.Windows.Controls;

using BattlegroundsApp.MVVM.Models;

namespace BattlegroundsApp.MVVM.Views;

/// <summary>
/// Interaction logic for LobbyBrowserView.xaml
/// </summary>
public partial class LobbyBrowserView : UserControl {

    public LobbyBrowserView() {
        InitializeComponent();
    }

    private void GameLobbyList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (this.DataContext is LobbyBrowserViewModel vm) {
            vm.RefreshJoin();
        }
    }

}
