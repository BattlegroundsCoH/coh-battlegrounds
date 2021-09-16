using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.MVVM;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyChatSpectatorModel : IViewModel {

        private LobbyHandler m_handler;

        public bool SingleInstanceOnly => false;

        public LobbyChatSpectatorModel(LobbyHandler lobbyHandler) {
            this.m_handler = lobbyHandler;
        }

    }

}
