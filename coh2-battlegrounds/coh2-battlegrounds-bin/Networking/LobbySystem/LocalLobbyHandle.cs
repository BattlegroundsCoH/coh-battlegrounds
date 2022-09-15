using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.LobbySystem.Local;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

namespace Battlegrounds.Networking.LobbySystem;

public sealed class LocalLobbyHandle : ILobbyHandle {

    private readonly LocalLobbyTeam m_allies;
    private readonly LocalLobbyTeam m_axis;
    private readonly LocalLobbyTeam m_obs;
    private readonly ILobbyPlanningHandle m_planner;

    private bool m_areRolesReversed;

    public string Title => $"{this.Self.Name}'s Lobby";

    public bool IsHost => true;

    public SteamUser Self { get; }

    public ILobbyTeam Allies => this.m_allies;

    public ILobbyTeam Axis => this.m_axis;

    public ILobbyTeam Observers => this.m_obs;

    public Dictionary<string, string> Settings { get; }

    public ILobbyPlanningHandle? PlanningHandle => this.m_planner;

    public event LobbyEventHandler<ILobbyTeam>? OnLobbyTeamUpdate;
    public event LobbyEventHandler<ILobbySlot>? OnLobbySlotUpdate;
    public event LobbyEventHandler<LobbyCompanyChangedEventArgs>? OnLobbyCompanyUpdate;
    public event LobbyEventHandler<LobbySettingsChangedEventArgs>? OnLobbySettingUpdate;

    public LocalLobbyHandle(SteamUser user) {
        
        // Set self
        this.Self = user;

        // Do settings
        this.Settings = new();

        // Create allies
        this.m_allies = new LocalLobbyTeam(this, LobbyConstants.TID_ALLIES);
        this.m_axis = new LocalLobbyTeam(this, LobbyConstants.TID_AXIS);
        this.m_obs = new LocalLobbyTeam(this, LobbyConstants.TID_OBS);

        // Set
        this.m_allies.Resize(1);
        this.m_axis.Resize(1);

        // Create self
        var self = new LocalLobbyMember(this, user.Name, user.ID, LobbyConstants.ROLE_HOST, 0, LobbyMemberState.Waiting);
        (this.m_allies.Slots[0] as LocalLobbySlot)?.SetOccupant(self);

        // Create planner
        this.m_planner = new LocalLobbyPlanner(this);

    }

    public void AddAI(int tid, int sid, int difficulty, ILobbyCompany company) {

        if ((tid is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis).Slots[sid] is LocalLobbySlot locSlot && !locSlot.IsOccupied) {

            // Create player
            var aiPlayer = new LocalLobbyMember(this, string.Empty, 0, LobbyConstants.ROLE_AI, (byte)difficulty, LobbyMemberState.Waiting, company);

            // Add
            locSlot.SetOccupant(aiPlayer);

            // Notify
            this.OnLobbySlotUpdate?.Invoke(locSlot);

        }

    }

    public uint GetPlayerCount(bool humansOnly = false)
        => this.m_allies.Slots.Concat(this.m_axis.Slots).Aggregate(0u, (a, b) => a + (b.IsOccupied && (humansOnly && !b.IsAI() || !humansOnly) ? 1u : 0u));

    public byte GetSelfTeam() {
        if (this.Allies.GetSlotOfMember(this.Self.ID) is null) {
            if (this.Axis.GetSlotOfMember(this.Self.ID) is null) {
                return LobbyConstants.TID_OBS;
            }
            return LobbyConstants.TID_AXIS;
        }
        return LobbyConstants.TID_ALLIES;
    }

    public void LockSlot(int tid, int sid) {
        if ((tid is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis).Slots[sid] is LocalLobbySlot locSlot && !locSlot.IsOccupied) {
            locSlot.State = LobbyConstants.STATE_LOCKED;
            this.OnLobbySlotUpdate?.Invoke(locSlot);
        }
    }

    public void MoveSlot(ulong mid, int tid, int sid) {

        // Grab current position
        var (u, v) = this.PosOf(mid);

        // Bail if none
        if (u is -1 && v is -1) {
            return;
        }

        // Clear old slot
        var oldTeam = u is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis;
        if (oldTeam.Slots[v] is not LocalLobbySlot b) {
            return;
        }

        // Grab occupant
        var occupant = b.Occupant;

        // Get team and move to
        var team = tid is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis;
        if (team.Slots[sid] is LocalLobbySlot a) {
            a.SetOccupant(occupant);
        }

        // Set occupant null
        b.SetOccupant(null);

        // Update visually
        this.OnLobbyTeamUpdate?.Invoke(team);
        if (team != oldTeam) {
            this.OnLobbyTeamUpdate?.Invoke(oldTeam);
        }

    }

    private (int,int) PosOf(ulong mid) {
        
        // Grab index
        int u = this.m_allies.Slots.IndexOf(x => x.IsOccupied && x.Occupant.MemberID == mid);
        if (u is not -1) {
            return (LobbyConstants.TID_ALLIES, u);
        }

        // Grab other index
        int v = this.m_axis.Slots.IndexOf(x => x.IsOccupied && x.Occupant.MemberID == mid);
        if (v is not -1) {
            return (LobbyConstants.TID_AXIS, v);
        }

        // Return none
        return (-1, -1);

    }

    public void RemoveOccupant(int tid, int sid) {
        var t = tid is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis;
        if (t.Slots[sid] is LocalLobbySlot locSlot && locSlot.IsOccupied) {
            locSlot.SetOccupant(null);
            this.OnLobbyTeamUpdate?.Invoke(t);
        }
    }

    public void SetCompany(int tid, int sid, ILobbyCompany company) {
        if ((tid is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis).Slots[sid] is LocalLobbySlot locSlot && locSlot.IsOccupied) {
            locSlot.Occupant.ChangeCompany(company);
            this.OnLobbyCompanyUpdate?.Invoke(new(tid, sid, company));
        }
    }

    public void SetLobbySetting(string setting, string value) {
        this.Settings[setting] = value;
        this.OnLobbySettingUpdate?.Invoke(new(setting, value));
    }

    public bool SetTeamsCapacity(int newCapacity) {

        // Bail if resize is not possible
        if (!this.CanResize(newCapacity)) {
            return false;
        }

        // Grab teams (safe cast)
        if (this.m_allies is not LocalLobbyTeam allies || this.m_axis is not LocalLobbyTeam axis) {
            return false;
        }

        // Trigger resize
        allies.Resize(newCapacity);
        axis.Resize(newCapacity);

        // Update size
        this.OnLobbyTeamUpdate?.Invoke(allies);
        this.OnLobbyTeamUpdate?.Invoke(axis);

        // Return true
        return true;

    }

    private bool CanResize(int newCap) 
        => this.m_allies.Slots.Count(x => x.IsOccupied) <= newCap && this.m_axis.Slots.Count(x => x.IsOccupied) <= newCap;

    public void UnlockSlot(int tid, int sid) {
        if ((tid is LobbyConstants.TID_ALLIES ? this.m_allies : this.m_axis).Slots[sid] is LocalLobbySlot locSlot) {
            locSlot.State = LobbyConstants.STATE_OPEN;
            this.OnLobbySlotUpdate?.Invoke(locSlot);
        }
    }

    public void SetTeamRoles(string team1, string team2) {
        
        // Set roles
        this.m_allies.SetRole(team1);
        this.m_axis.SetRole(team2);
        
        // Update visually and all that
        this.OnLobbyTeamUpdate?.Invoke(this.m_allies);
        this.OnLobbyTeamUpdate?.Invoke(this.m_axis);

    }

    public void SwapTeamRoles() {
        
        // Grab flag
        this.m_areRolesReversed = !this.m_areRolesReversed;

        // Set new roles
        var tmp = this.m_allies.TeamRole;
        this.m_allies.SetRole(this.m_axis.TeamRole);
        this.m_axis.SetRole(tmp);

        // Notify changes
        this.OnLobbyTeamUpdate?.Invoke(this.m_allies);
        this.OnLobbyTeamUpdate?.Invoke(this.m_axis);

    }

    public bool AreTeamRolesSwapped() {
        return this.m_areRolesReversed;
    }

    public bool TeamHasMember(byte tid, ulong memberId) => tid switch {
        0 => this.m_allies.GetSlotOfMember(memberId) is not null,
        1 => this.m_axis.GetSlotOfMember(memberId) is not null,
        2 => this.m_obs.GetSlotOfMember(memberId) is not null,
        _ => false
    };

    #region Nop calls

    public void CloseHandle() {
        // Do nothing
    }

    public void CancelMatch() {
        // Do Nothing
    }

    public void HaltMatch() {
        // Do Nothing
    }

    public void LaunchMatch() {
        // Do Nothing
    }

    public void MemberState(ulong mid, int tid, int sid, LobbyMemberState state) {
        // Do Nothing (Not relevant in this case)
    }

    public void NotifyError(string errorType, string errorMessage) {
        // Do Nothing
    }

    public void NotifyMatch(string infoType, string infoMessage) {
        // Do Nothing
    }

    public void ReleaseGamemode() {
        // Do Nothing
    }

    public void ReleaseResults() {
        // Do Nothing
    }

    public void RequestCompanyFile(params ulong[] members) {
        // Do Nothing
    }

    public void SendChatMessage(int filter, ulong senderID, string message) {
        // Do Nothing
    }

    public void SetLobbyState(LobbyState state) {
        // Do Nothing
    }

    public void RespondPoll(string pollId, bool pollVote) {
        // Do nothing
    }

    public bool StartMatch(double cancelTime) => true;

    public UploadResult UploadCompanyFile(byte[] contents, ulong companyOwner, UploadProgressCallbackHandler? callbackHandler) => UploadResult.UPLOAD_SUCCESS;

    public UploadResult UploadGamemodeFile(byte[] contents, UploadProgressCallbackHandler? callbackHandler) => UploadResult.UPLOAD_SUCCESS;

    public LobbyPollResults ConductPoll(string pollType, double pollTime = 3) => new(1, 0, false);

    public void Subscribe(string to, LobbyEventHandler<ContentMessage> eventHandler) {}

    public void NotifyScreen(string screen) {
        // Do nothing
    }

    #endregion

}
