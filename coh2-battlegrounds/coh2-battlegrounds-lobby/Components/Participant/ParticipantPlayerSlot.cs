﻿using System;
using System.Linq;
using System.Windows;

using Battlegrounds.Functional;
using Battlegrounds.Lobby.Helpers;
using Battlegrounds.Misc.Collections;
using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Lobby.Components.Participant;

public sealed class ParticipantPlayerSlot : PlayerSlot {

    public override SlotContextMenu ContextMenu { get; }

    public override Visibility IsCompanySelectorVisible
        => (this.Slot.IsSelf() && this.SelectableCompanies.Count > 1) ? Visibility.Visible : Visibility.Collapsed;

    public ParticipantPlayerSlot(ILobbySlot teamSlot, Team team) : base(teamSlot, team) {
        this.ContextMenu = new ParticipantSlotContextMenu(teamSlot.Handle ?? throw new Exception("Expected valid lobbyAPI isntance!"), this);
    }

    protected override void RefreshCompanyView() {

        // Bail if slot API handle is invalid for some reason
        if (this.Slot.Handle is null) {
            return;
        }

        // Populate
        if (this.IsSelf) {

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

            // Set self company
            if (this.m_slot.Occupant is ILobbyMember mem) {
                mem.ChangeCompany(this.SelectedCompany);
            }

            // Trigger notify if only one selectable
            if (this.SelectableCompanies.Count is 1) {
                this.NotifyProperty(new(nameof(this.SelectedCompany)));
            }

        }

    }

    protected override void OnLobbyCompanyChanged(int newValue) {
        if (this.IsSelf) {
            this.SetCompany();
        }
    }

    public override void OnLobbyCompanyChanged(ILobbyCompany company) {
        if (this.IsSelf) {
            return;
        }
        this.SelectableCompanies.ClearTo(company);
        this.SelectedCompanyIndex = 0;
        this.RefreshLHSIcon();
        this.NotifyProperty(new(nameof(this.SelectedCompany)));
    }

}
