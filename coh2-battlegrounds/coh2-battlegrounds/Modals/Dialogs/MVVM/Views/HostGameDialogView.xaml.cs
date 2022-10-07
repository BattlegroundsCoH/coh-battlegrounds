using Battlegrounds.UI.Modals;

using BattlegroundsApp.Modals.Dialogs.MVVM.Models;

using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Views;

/// <summary>
/// Interaction logic for HostGameDialogView.xaml
/// </summary>
public partial class HostGameDialogView : Modal {

    public HostGameDialogView() {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e) {
        if (this.DataContext is HostGameDialogViewModel vm) {
            vm.LobbyPassword = ((PasswordBox)sender).Password;
        }
    }

}
