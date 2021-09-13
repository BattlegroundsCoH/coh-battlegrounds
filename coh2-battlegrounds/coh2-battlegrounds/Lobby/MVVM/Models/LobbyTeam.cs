using Battlegrounds.Networking.Lobby;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyTeam {

        public LobbySlot[] Slots { get; }

        public LobbyTeam(ILobbyTeam lobbyTeam) {

            // Define slot models
            this.Slots = new LobbySlot[4] {
                new(lobbyTeam.Slots[0]),
                new(lobbyTeam.Slots[1]),
                new(lobbyTeam.Slots[2]),
                new(lobbyTeam.Slots[3])
            };

        }

    }

}
