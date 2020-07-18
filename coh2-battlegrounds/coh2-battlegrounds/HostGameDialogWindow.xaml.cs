using Battlegrounds.Online.Services;
using coh2_battlegrounds;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BattlegroundsApp
{
    /// <summary>
    /// Interaction logic for HostGameDialogWindow.xaml
    /// </summary>
    public partial class HostGameDialogWindow : Window
    {
        public HostGameDialogWindow()
        {
            InitializeComponent();
        }

        private void CancelHostGameButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HostGameButton_Click(object sender, RoutedEventArgs e)
        {
            string _lobbyName = lobbyName.Text;
            string _lobbyPassword = lobbyPassword.Text;

            ManagedLobby.Host(ServerMessageHandler.hub, _lobbyName, _lobbyPassword, ServerMessageHandler.OnServerResponse);
        }
    }
}
