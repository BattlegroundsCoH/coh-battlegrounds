using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyTeam {

    public LobbySlot Slot1 { get; set; }

    public LobbySlot Slot2 { get; set; }

    public LobbySlot Slot3 { get; set; }

    public LobbySlot Slot4 { get; set; }

    public LobbyAPIStructs.LobbyTeam Team { get; }

    public LobbyTeam(LobbyAPI lobbyAPI, LobbyAPIStructs.LobbyTeam lobbyTeam) {
        this.Team = lobbyTeam;
        if (lobbyAPI.IsHost) {
            this.Slot1 = new LobbyHostSlotModel(lobbyTeam.Slots[0], this);
            this.Slot2 = new LobbyHostSlotModel(lobbyTeam.Slots[1], this);
            this.Slot3 = new LobbyHostSlotModel(lobbyTeam.Slots[2], this);
            this.Slot4 = new LobbyHostSlotModel(lobbyTeam.Slots[3], this);
        } else {
            this.Slot1 = new LobbyParticipantSlotModel(lobbyTeam.Slots[0], this);
            this.Slot2 = new LobbyParticipantSlotModel(lobbyTeam.Slots[1], this);
            this.Slot3 = new LobbyParticipantSlotModel(lobbyTeam.Slots[2], this);
            this.Slot4 = new LobbyParticipantSlotModel(lobbyTeam.Slots[3], this);
        }
    }

}
