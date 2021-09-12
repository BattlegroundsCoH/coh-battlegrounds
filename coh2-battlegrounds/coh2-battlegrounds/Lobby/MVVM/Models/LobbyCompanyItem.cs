using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Lobby.MVVM.Models {
    
    public class LobbyCompanyItem {
    
        public string Name { get; set; }

        public Faction Army { get; set; }

        public string Type { get; set; }

        public LobbyCompanyItem() {

        }

    }

}
