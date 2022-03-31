using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// 
    /// </summary>
    public static class LobbyAPIStructs {

        public interface IAPIObject {

            [JsonIgnore]
            public LobbyAPI API { get; set; }

            [MemberNotNull(nameof(API))]
            public void SetAPI(LobbyAPI api);

        }

        public class LobbyCompany : IAPIObject {
            public bool IsAuto { get; set; }
            public bool IsNone { get; set; }
            public string Name { get; set; }
            public string Army { get; set; }
            public float Strength { get; set; }
            public string Specialisation { get; set; }
            public LobbyAPI API { get; set; }

            [MemberNotNull(nameof(API))]
            public void SetAPI(LobbyAPI api) {
                this.API = api;
            }

        }

        public class LobbyMember : IAPIObject {

            public ulong MemberID { get; set; }
            public string DisplayName { get; set; }
            public int Role { get; set; }
            public int AILevel { get; set; }
            public LobbyCompany? Company { get; set; }
            public LobbyAPI API { get; set; }

            [MemberNotNull(nameof(API))]
            public void SetAPI(LobbyAPI api) {
                this.API = api;
                this.Company.API = api;
            }

        }

        public class LobbySlot : IAPIObject {

            public int SlotID { get; set; }
            public int TeamID { get; set; }
            public byte State { get; set; }
            public LobbyMember? Occupant { get; set; }
            [JsonIgnore]
            public LobbyAPI? API { get; set; }

            [JsonIgnore]
            [MemberNotNullWhen(true, nameof(Occupant))]
            public bool IsOccupied => this.State == 1;

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

            [MemberNotNull(nameof(API))]
            public void SetAPI(LobbyAPI api) {
                this.API = api;
                this.Occupant?.SetAPI(api);
            }

        }

        public class LobbyTeam : IAPIObject {

            public LobbySlot[] Slots { get; set; }
            public int TeamID { get; set; }
            public int Capacity { get; set; }
            public LobbyAPI? API { get; set; }

            [MemberNotNull(nameof(API))]
            public void SetAPI(LobbyAPI api) {
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
            public string Timestamp { get; set; }
            public string Timezone { get; set; }
            public string Sender { get; set; }
            public string Message { get; set; }
            public string Channel { get; set; }
            public string Colour { get; set; }
        }

        public class LobbyRemote {
            public ulong HostID { get; set; }
            public LobbyTeam[] Teams { get; set; }
            public Dictionary<string, string> Settings { get; set; }
        }

        public enum LobbyState {
            None = 0,
            InLobby = 1,
            Starting = 2,
            Playing = 3
        }

    }

}
