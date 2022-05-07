using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyParticipantContextMenu : LobbyContextMenu {

    public override LobbyContextAction KickPlayer { get; }

    public override LobbyContextAction LockSlot { get; }

    public override LobbyContextAction UnlockSlot { get; }

    public override LobbyContextAction AddEasyAI { get; }

    public override LobbyContextAction AddStandardAI { get; }

    public override LobbyContextAction AddHardAI { get; }

    public override LobbyContextAction AddExpertAI { get; }

    public LobbyParticipantContextMenu(ILobbyHandle handle, LobbySlot slot) : base(handle, slot) {

        // Player actions
        this.KickPlayer = new("", new(() => { }), NeverTrue, NeverVisible);

        // Slot state
        this.LockSlot = new("", new(() => { }), NeverTrue, NeverVisible);
        this.UnlockSlot = new("", new(() => { }), NeverTrue, NeverVisible);

        // AI stuff
        this.AddEasyAI = new("", new(() => { }), NeverTrue, NeverVisible);
        this.AddStandardAI = new("", new(() => { }), NeverTrue, NeverVisible);
        this.AddHardAI = new("", new(() => { }), NeverTrue, NeverVisible);
        this.AddExpertAI = new("", new(() => { }), NeverTrue, NeverVisible);

    }

}
