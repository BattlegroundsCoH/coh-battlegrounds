using System;
using System.Linq;
using System.Windows;

using Battlegrounds.Functional;
using Battlegrounds.Misc.Collections;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyHostSlotModel : LobbySlot {

    private bool IsControllable => this.Slot.IsSelf() || this.Slot.IsAI();

    public override Visibility IsCompanySelectorVisible 
        => (this.IsControllable && this.SelectableCompanies.Count > 1) ? Visibility.Visible : Visibility.Collapsed;

    public override LobbyContextMenu ContextMenu { get; }

    public LobbyHostSlotModel(ILobbySlot teamSlot, LobbyTeam team) : base(teamSlot, team) {
        this.ContextMenu = new LobbyHostContextMenu(teamSlot.Handle ?? throw new Exception("Expected lobby API instance but found none!"), this);
    }

    protected override void RefreshCompanyView() {

        // Populate
        if (this.IsControllable) {

            // Clear selectables
            this.SelectableCompanies.ClearTo(DummyCompany);

            // Add all companies
            (this.Slot.TeamID == 0 ? AlliedCompanies() : AxisCompanies()).Select(FromCompany).ForEach(this.SelectableCompanies.Add);

            // Remove dummy
            if (this.SelectableCompanies.Contains(DummyCompany)) {
                this.SelectableCompanies.Remove(DummyCompany);
            }

            // Notify index
            this.SelectedCompanyIndex = 0;

            // Trigger notify if only one selectable
            if (this.SelectableCompanies.Count is 1) {
                this.NotifyProperty(new(nameof(this.SelectedCompany)));
            }

        }

    }

    protected override void OnLobbyCompanyChanged(int newValue) {
        if (this.IsControllable && newValue >= 0) {
            this.SetCompany();
        }
    }

    public override void OnLobbyCompanyChanged(ILobbyCompany company) {
        if (this.IsControllable) { // if set by us, ignore this
            return;
        }
        this.SelectableCompanies.ClearTo(company);
        this.SelectedCompanyIndex = 0;
        this.RefreshLHSIcon();
        this.NotifyProperty(new(nameof(this.SelectedCompany)));
    }

}
