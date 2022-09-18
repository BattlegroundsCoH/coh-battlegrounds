using System;

namespace Battlegrounds.Networking.LobbySystem.Local;

public class LocalLobbyTeam : ILobbyTeam {

    public ILobbySlot[] Slots { get; }

    public int TeamID { get; }

    public int Capacity { get; private set; }

    public ILobbyHandle Handle { get; private set; }

    public string TeamRole { get; private set; }

    public LocalLobbyTeam(ILobbyHandle handle, int tid) {
        this.Handle = handle;
        this.TeamID = tid;
        this.Slots = new ILobbySlot[4] {
            new LocalLobbySlot(handle, 0, tid),
            new LocalLobbySlot(handle, 1, tid),
            new LocalLobbySlot(handle, 2, tid),
            new LocalLobbySlot(handle, 3, tid)
        };
        this.TeamRole = tid is LobbyConstants.TID_ALLIES ? "Team_Allies" : "Team_Axis";
    }

    public ILobbySlot? GetSlotOfMember(ulong memberID) {
        for (int i = 0; i < this.Slots.Length; i++) { 
            if (this.Slots[i].Occupant is ILobbyMember mem && mem.MemberID == memberID) {
                return this.Slots[i];
            }
        }
        return null;
    }

    public void Resize(int k) {

        // Prepare swaps
        var swaps = new bool[4];
        for (int i = 0; i < 4; i++) {
            swaps[i] = this.Slots[i].State is LobbyConstants.STATE_OCCUPIED;
        }

        // Update slots
        for (int i = 0; i < 4; i++) {
            if (i >= k) {
                if (swaps[i]) {
                    for (int j = k; j >= 0; j--) {
                        if (!swaps[j]) {
                            this.Swap(i, j);
                            swaps[j] = false;
                            break;
                        }
                    }
                }
                this.Slots[i].State = LobbyConstants.STATE_DISABLED;
            } else if (!swaps[i]) {
                this.Slots[i].State = LobbyConstants.STATE_OPEN;
            }
        }

        // Set
        this.Capacity = k;

    }

    public void Swap(int i, int j) {

        // Grab instances
        var from = this.Slots[i] as LocalLobbySlot ?? throw new Exception();
        var to = this.Slots[j] as LocalLobbySlot ?? throw new Exception();

        // Backup
        var tmpState = from.State;
        var tmpOcc = from.Occupant;

        // Do swap
        from.SetOccupant(to.Occupant);
        from.State = to.State;
        to.SetOccupant(tmpOcc);
        to.State = tmpState;

    }

    public void SetHandle(ILobbyHandle handle) => this.Handle = handle;

    public void SetRole(string roleId) => this.TeamRole = roleId;

}
