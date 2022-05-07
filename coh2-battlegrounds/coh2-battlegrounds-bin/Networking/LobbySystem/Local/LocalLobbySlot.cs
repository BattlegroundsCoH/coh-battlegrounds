using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem.Local;

public class LocalLobbySlot : ILobbySlot {

    private ILobbyMember? m_occupant;

    public int SlotID { get; }

    public int TeamID { get; }

    public byte State { get; set; }

    public ILobbyMember? Occupant => this.m_occupant;

    [MemberNotNullWhen(true, nameof(Occupant))]
    public bool IsOccupied => this.m_occupant is not null;

    public ILobbyHandle Handle { get; private set; }

    public LocalLobbySlot(ILobbyHandle handle, int sid, int tid) {
        this.Handle = handle;
        this.SlotID = sid;
        this.TeamID = tid;
    }

    public bool IsAI() => this.IsOccupied && this.Occupant.Role is LobbyConstants.ROLE_AI;

    public bool IsAI(AIDifficulty min, AIDifficulty max) {
        if (this.Occupant is ILobbyMember mem) {
            return mem.Role == 3 && (byte)min <= mem.AILevel && mem.AILevel <= (byte)max;
        }
        return false;
    }

    public bool IsSelf() => this.IsOccupied && this.Occupant.MemberID == this.Handle.Self.ID;

    public void SetHandle(ILobbyHandle handle) => this.Handle = handle;

    public void TrySetCompany(ILobbyCompany company) {
        if (this.IsOccupied) {
            this.Occupant.ChangeCompany(company);
        }
    }

    public void SetOccupant(ILobbyMember? occupant) {
        this.m_occupant = occupant;
        this.State = occupant is null ? LobbyConstants.STATE_OPEN : LobbyConstants.STATE_OCCUPIED;
    }

}
