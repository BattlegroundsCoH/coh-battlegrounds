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

using BattlegroundsApp.MVVM.Models;

namespace BattlegroundsApp.MVVM.Views {

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

}
