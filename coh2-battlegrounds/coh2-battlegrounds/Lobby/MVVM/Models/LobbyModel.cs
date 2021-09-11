using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Networking.Lobby;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyModel : IViewModel {

        private LobbyHandler m_handler;

        public LobbyButtonModel EditCompany { get; }

        public LobbyButtonModel ExitLobby { get; }

        public LobbyButtonModel StartMatch { get; }

        public ImageSource SelectedMatchScenario { get; }

        public bool SingleInstanceOnly => false;

        public LobbyModel(LobbyHandler handler) {

            // Set handler
            this.m_handler = handler;

            // Create buttons
            this.EditCompany = new LobbyButtonModel() {
                Click = new RelayCommand(this.EditSelfCompany),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_EditCompany")
            };

            // Create buttons
            this.ExitLobby = new LobbyButtonModel() {
                Click = new RelayCommand(this.LeaveLobby),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_LeaveLobby")
            };

        }

        private void EditSelfCompany() {

        }

        private void LeaveLobby() {

        }

    }

}
