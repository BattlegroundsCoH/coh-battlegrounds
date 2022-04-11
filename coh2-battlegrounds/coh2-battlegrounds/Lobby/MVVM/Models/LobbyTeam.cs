using System.Windows;

using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyTeam {

    private LobbyAPIStructs.LobbyTeam m_team;

    public LobbySlot Slot1 { get; }

    public LobbySlot Slot2 { get; }

    public LobbySlot Slot3 { get; }

    public LobbySlot Slot4 { get; }

    public LobbyAPIStructs.LobbyTeam Team => this.m_team;

    public LobbyTeam(LobbyAPI lobbyAPI, LobbyAPIStructs.LobbyTeam lobbyTeam) {
       
        // Set team isntance
        this.m_team = lobbyTeam;
        
        // Create models
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

        // Subscribe to team changes
        lobbyAPI.OnLobbyTeamUpdate += this.OnLobbyTeamUpdated;

    }

    private void OnLobbyTeamUpdated(LobbyAPIStructs.LobbyTeam obj) {

        // Only trigger on self
        if (obj.TeamID == this.Team.TeamID) {

            // Update self ref
            this.m_team = obj;

            // Update slots
            this.Slot1.OnLobbySlotUpdate(obj.Slots[0]);
            this.Slot2.OnLobbySlotUpdate(obj.Slots[1]);
            this.Slot3.OnLobbySlotUpdate(obj.Slots[2]);
            this.Slot4.OnLobbySlotUpdate(obj.Slots[3]);

            // Enter GUI thread and update
            Application.Current.Dispatcher.Invoke(() => {
                // TODO: Update something
            });

        }
    }

}
