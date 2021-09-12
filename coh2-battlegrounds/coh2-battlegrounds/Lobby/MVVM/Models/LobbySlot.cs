using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Networking.Lobby;

namespace BattlegroundsApp.Lobby.MVVM.Models {
    
    public class LobbySlot : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        
        public ILobbyTeamSlot NetworkInterface { get; set; }
        
        public string LeftDisplayString { get; set; }

        public bool IsSelf { get; set; }

        public bool IsOpen { get; set; }

        public bool IsLocked { get; set; }

        public Visibility LeftIconVisibility => this.IsOpen ? Visibility.Hidden : Visibility.Visible;

        public Visibility CompanyVisibility => !(this.IsOpen || this.IsLocked) ? Visibility.Visible : Visibility.Hidden;

        public LobbySlot() {

        }

    }

}
