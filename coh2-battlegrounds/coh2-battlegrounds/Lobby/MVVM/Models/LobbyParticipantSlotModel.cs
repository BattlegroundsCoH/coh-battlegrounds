using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models;
public class LobbyParticipantSlotModel : LobbySlot {

    public override LobbyContextMenu ContextMenu { get; }

    public override Visibility IsCompanySelectorVisible => this.Slot.IsSelf() ? Visibility.Visible : Visibility.Collapsed;

    public LobbyParticipantSlotModel(LobbyAPIStructs.LobbySlot teamSlot, LobbyTeam team) : base(teamSlot, team) {
        this.ContextMenu = new LobbyParticipantContextMenu(teamSlot.API ?? throw new Exception("Expected valid lobbyAPI isntance!"));
    }

    protected override void OnLobbyCompanyChanged(int newValue) {
        if (this.Slot.API is null) {
            return;
        }
        //throw new NotImplementedException();
    }
}
