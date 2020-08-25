using System;
using System.Collections.Generic;
using System.Text;

namespace BattlegroundsApp
{
    public class Lobby
    {
        public string _lobbyName { get; set; }
        public string _lobbyPlayers { get; set; }
        public bool _lobbyPasswordProtected { get; set; }
        public string _lobbyGuid { get; set; }
    }
}
