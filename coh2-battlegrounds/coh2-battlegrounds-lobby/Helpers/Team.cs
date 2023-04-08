using System.Windows;

using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Components.Host;
using Battlegrounds.Lobby.Components.Participant;
using Battlegrounds.Lobby.Pages;
using Battlegrounds.Lobby.Pages.Host;
using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Lobby.Helpers;

public sealed class Team {

    private ILobbyTeam m_team;

    public PlayerSlot Slot1 { get; }

    public PlayerSlot Slot2 { get; }

    public PlayerSlot Slot3 { get; }

    public PlayerSlot Slot4 { get; }

    public ILobbyTeam Model => this.m_team;

    public BaseLobby Lobby { get; }

    public string Title => BattlegroundsContext.Localize.GetString(this.m_team.TeamRole);

    public Team(ILobbyHandle lobbyAPI, ILobbyTeam lobbyTeam, BaseLobby model) {

        // Set team isntance
        this.m_team = lobbyTeam;

        // Set lobby instance
        this.Lobby = model;

        // Create models
        if (lobbyAPI.IsHost) {
            this.Slot1 = new HostPlayerSlot(lobbyTeam.Slots[0], this);
            this.Slot2 = new HostPlayerSlot(lobbyTeam.Slots[1], this);
            this.Slot3 = new HostPlayerSlot(lobbyTeam.Slots[2], this);
            this.Slot4 = new HostPlayerSlot(lobbyTeam.Slots[3], this);
        } else {
            this.Slot1 = new ParticipantPlayerSlot(lobbyTeam.Slots[0], this);
            this.Slot2 = new ParticipantPlayerSlot(lobbyTeam.Slots[1], this);
            this.Slot3 = new ParticipantPlayerSlot(lobbyTeam.Slots[2], this);
            this.Slot4 = new ParticipantPlayerSlot(lobbyTeam.Slots[3], this);
        }

        // Subscribe to team changes
        lobbyAPI.OnLobbyTeamUpdate += this.OnLobbyTeamUpdated;

    }

    public void OnTeamMemberCompanyUpdated(int sid, ILobbyCompany company) {

        // Try set in slot
        this.Model.Slots[sid].TrySetCompany(company);

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
            if (this.Lobby is HostLobby hostModel) {
                hostModel.RefreshPlayability();
            }

        });

    }

    private void OnLobbyTeamUpdated(ILobbyTeam obj) {

        // Only trigger on self
        if (obj.TeamID == this.Model.TeamID) {

            // Update self ref
            this.m_team = obj;

            // Update slots
            this.Slot1.OnLobbySlotUpdate(obj.Slots[0]);
            this.Slot2.OnLobbySlotUpdate(obj.Slots[1]);
            this.Slot3.OnLobbySlotUpdate(obj.Slots[2]);
            this.Slot4.OnLobbySlotUpdate(obj.Slots[3]);

            // Enter GUI thread and update
            Application.Current.Dispatcher.Invoke(() => {
                if (this.Lobby is HostLobby hostModel) {
                    hostModel.RefreshPlayability();
                }
                this.Lobby.NotifyProperty(obj.TeamID is LobbyConstants.TID_ALLIES ? nameof(HostLobby.Allies) : nameof(HostLobby.Axis));
            });

        }
    }

    public (bool, bool) CanPlay() {

        // Grab slots
        var slots = this.Model.Slots;

        // flag
        bool flag1 = false; // any valid player
        bool flag2 = true; // No invalid players

        // Loop over slots and check if any valid
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i].IsOccupied) {
                if (slots[i].Occupant is not ILobbyMember mem) {
                    continue; // Some werid err
                }
                flag1 |= (mem.State is LobbyMemberState.Waiting or LobbyMemberState.Planning); // Could do something here forcing planning players to press ready
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
