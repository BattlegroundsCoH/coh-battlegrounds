using Battlegrounds.AI;
using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Lobby.Components.Host;

/// <summary>
/// 
/// </summary>
public sealed class HostSlotContextMenu : SlotContextMenu {

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction KickPlayer { get; }

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction LockSlot { get; }

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction UnlockSlot { get; }

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction AddEasyAI { get; }

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction AddStandardAI { get; }

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction AddHardAI { get; }

    /// <summary>
    /// 
    /// </summary>
    public override LobbyContextAction AddExpertAI { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="slot"></param>
    public HostSlotContextMenu(ILobbyHandle handle, PlayerSlot slot) : base(handle, slot) {

        // Define captured check
        var ai_check = () => this.Slot.Slot.State != 3 && !this.Slot.Slot.IsOccupied;

        // Player actions
        this.KickPlayer = new(LOCSTR_KICKPLAYER(), new(this.KickOccupant), () => !this.Slot.IsSelf && this.Slot.Slot.IsOccupied, VisibleIfEnabled);

        // Slot state
        this.LockSlot = new(LOCSTR_LOCK_SLOT(), new(this.LockSlotAction), () => this.Slot.Slot.State == 0, VisibleIfEnabled);
        this.UnlockSlot = new(LOCSTR_UNLOCK_SLOT(), new(this.UnlockSlotAction), () => this.Slot.Slot.State == 2, VisibleIfEnabled);

        // AI stuff
        this.AddEasyAI = new(LOCSTR_EASYAI(), new(() => this.AddAIPlayer(AIDifficulty.AI_Easy)), ai_check, VisibleIfEnabled);
        this.AddStandardAI = new(LOCSTR_STANDARDAI(), new(() => this.AddAIPlayer(AIDifficulty.AI_Standard)), ai_check, VisibleIfEnabled);
        this.AddHardAI = new(LOCSTR_HARDAI(), new(() => this.AddAIPlayer(AIDifficulty.AI_Hard)), ai_check, VisibleIfEnabled);
        this.AddExpertAI = new(LOCSTR_EXPERTAI(), new(() => this.AddAIPlayer(AIDifficulty.AI_Expert)), ai_check, VisibleIfEnabled);

    }

    private void AddAIPlayer(AIDifficulty difficulty) {

        // Get first available company
        var company = this.Slot.SelectableCompanies[0];

        // Add
        this.Handle.AddAI(this.TeamId, this.SlotId, (int)difficulty, company);

    }

    private void KickOccupant()
        => this.Handle.RemoveOccupant(this.TeamId, this.SlotId);

    private void LockSlotAction()
        => this.Handle.LockSlot(this.TeamId, this.SlotId);

    private void UnlockSlotAction()
        => this.Handle.UnlockSlot(this.TeamId, this.SlotId);

}
