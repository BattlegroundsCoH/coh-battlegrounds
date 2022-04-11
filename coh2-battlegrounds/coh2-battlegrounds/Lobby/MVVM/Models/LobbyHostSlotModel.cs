using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models;
public class LobbyHostSlotModel : LobbySlot {

    public override Visibility IsCompanySelectorVisible => this.Slot.IsSelf() || this.Slot.IsAI() ? Visibility.Visible : Visibility.Collapsed;

    public override LobbyContextMenu ContextMenu { get; }

    public LobbyHostSlotModel(LobbyAPIStructs.LobbySlot teamSlot, LobbyTeam team) : base(teamSlot, team) {
        this.ContextMenu = new LobbyHostContextMenu(teamSlot.API ?? throw new Exception("Expected lobby API instance but found none!"), this);
    }

    protected override void OnLobbyCompanyChanged(int newValue) {
        if (this.Slot.API is null) {
            return;
        }
        if (this.Slot.IsSelf() || this.Slot.IsAI()) {
            this.SetLeftIcon(FactionIcons[this.SelectedCompany.Army], FactionHoverIcons[this.SelectedCompany.Army]);
            this.Slot.API.SetCompany(this.Slot.TeamID, this.Slot.SlotID, this.SelectedCompany);
        }
    }

}
