using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
