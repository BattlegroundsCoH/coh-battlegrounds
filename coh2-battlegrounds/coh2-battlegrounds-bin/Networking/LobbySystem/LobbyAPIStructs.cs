using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem; 

/// <summary>
/// 
/// </summary>
public static class LobbyAPIStructs {

    public interface IAPIObject {

        [JsonIgnore]
        public OnlineLobbyHandle? API { get; set; }

        [MemberNotNull(nameof(API))]
        public void SetAPI(OnlineLobbyHandle api);

    }

    public class LobbyCompany : IAPIObject {
        public bool IsAuto { get; set; }
        public bool IsNone { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Army { get; set; } = string.Empty;
        public float Strength { get; set; }
        public string Specialisation { get; set; } = string.Empty;
        public OnlineLobbyHandle? API { get; set; }

        [MemberNotNull(nameof(API))]
        public void SetAPI(OnlineLobbyHandle api) {
            this.API = api;
        }

    }

    public class LobbyMember : IAPIObject {

        public ulong MemberID { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public byte Role { get; set; }
        public byte AILevel { get; set; }
        public LobbyMemberState State { get; set; }
        public LobbyCompany? Company { get; set; }
        public OnlineLobbyHandle? API { get; set; }

        [MemberNotNull(nameof(API))]
        public void SetAPI(OnlineLobbyHandle api) {
            this.API = api;
            if (this.Company is not null) {
                this.Company.API = api;
            }
        }

    }

    public class LobbySlot : IAPIObject {

        public int SlotID { get; set; }
        public int TeamID { get; set; }
        public byte State { get; set; }
        public LobbyMember? Occupant { get; set; }

        [JsonIgnore]
        public OnlineLobbyHandle? API { get; set; }

        [JsonIgnore]
        [MemberNotNullWhen(true, nameof(Occupant))]
        public bool IsOccupied => this.State == 1;

        [MemberNotNullWhen(true, nameof(API))]
        public bool IsSelf() {
            if (this.API is null) {
                return false;
            }
            if (this.Occupant is LobbyMember mem) {
                return mem.MemberID == this.API.Self.ID;
            }
            return false;
        }

        public bool IsAI() {
            if (this.Occupant is LobbyMember mem) {
                return mem.Role == 3;
            }
            return false;
        }

        public void TrySetCompany(LobbyCompany company) {
            if (this.IsOccupied) {
                this.Occupant.Company = company;
            }
        }

        [MemberNotNull(nameof(API))]
        public void SetAPI(OnlineLobbyHandle api) {
            this.API = api;
            this.Occupant?.SetAPI(api);
        }

    }

    public class LobbyTeam : IAPIObject {

        public LobbySlot[] Slots { get; set; } = new LobbySlot[4];
        public int TeamID { get; set; }
        public int Capacity { get; set; }
        public OnlineLobbyHandle? API { get; set; }

        [MemberNotNull(nameof(API))]
        public void SetAPI(OnlineLobbyHandle api) {
            this.API = api;
            for (int i = 0; i < this.Slots.Length; i++) {
                this.Slots[i].SetAPI(api);
            }
        }

        public bool IsMember(ulong memberID) {
            return this.GetSlotOfMember(memberID) is not null;
        }

        public LobbySlot? GetSlotOfMember(ulong memberID) {
            for (int i = 0; i < this.Slots.Length; i++) {
                if (this.Slots[i].State == 1 && this.Slots[i].Occupant?.MemberID == memberID) {
                    return this.Slots[i];
                }
            }
            return null;
        }

    }

    public class LobbyMessage {
        public string Timestamp { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
    }

    public class LobbyRemote {
        public ulong HostID { get; set; }
        public LobbyTeam[] Teams { get; set; } = new LobbyTeam[3];
        public Dictionary<string, string> Settings { get; set; } = new();
    }

}
