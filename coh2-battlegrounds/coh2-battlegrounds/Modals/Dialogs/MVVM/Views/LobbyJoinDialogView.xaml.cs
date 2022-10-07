using Battlegrounds.UI.Modals;

using BattlegroundsApp.Modals.Dialogs.MVVM.Models;

using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Views;
/// <summary>
/// Interaction logic for LobbyJoinDialogView.xaml
/// </summary>
public partial class LobbyJoinDialogView : Modal {

    public LobbyJoinDialogView() {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e) {
        if (this.DataContext is LobbyJoinDialogViewModel vm) {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }

}
