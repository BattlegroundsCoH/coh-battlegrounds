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

    public LobbyModel Lobby { get; }

    public LobbyTeam(LobbyAPI lobbyAPI, LobbyAPIStructs.LobbyTeam lobbyTeam, LobbyModel model) {
       
        // Set team isntance
        this.m_team = lobbyTeam;

        // Set lobby instance
        this.Lobby = model;

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

    public void OnTeamMemberCompanyUpdated(int sid, LobbyAPIStructs.LobbyCompany company) {
        
        // Try set in slot
        this.Team.Slots[sid].TrySetCompany(company);
        
        // Enter GUI thread and update
        Application.Current.Dispatcher.Invoke(() => {

            // Trigger update
            switch (sid) {
                case 0:
                    this.Slot1.OnLobbyCompanyChanged(company); break;
                case 1:
                    this.Slot2.OnLobbyCompanyChanged(company); break;
                case 2:
                    this.Slot3.OnLobbyCompanyChanged(company); break;
                case 3:
                    this.Slot4.OnLobbyCompanyChanged(company); break;
                default:
                    break;
            }

            // Recheck playability
            if (this.Lobby is LobbyHostModel hostModel) {
                hostModel.RefreshPlayability();
            }

        });

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
                if (this.Lobby is LobbyHostModel hostModel) {
                    hostModel.RefreshPlayability();
                }
            });

        }
    }

    public (bool, bool) CanPlay() {

        // Grab slots
        var slots = this.Team.Slots;

        // flag
        bool flag1 = false; // any valid player
        bool flag2 = true; // No invalid players

        // Loop over slots and check if any valid
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i].IsOccupied) {
                if (slots[i].Occupant is not LobbyAPIStructs.LobbyMember mem) {
                    continue; // Some werid err
                }
                flag1 |= mem.State is LobbyAPIStructs.LobbyMemberState.Waiting;
                if (mem.Company?.IsNone ?? true) {
                    flag2 = false;
                } else {
                    flag2 &= true;
                }
            }
        }

        // Return flags
        return (flag1, flag2);
    
    }

}
