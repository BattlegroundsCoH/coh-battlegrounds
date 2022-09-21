using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem.Json;

public class JsonLobbyTeam : ILobbyTeam {

    private ILobbyHandle? m_handle;

    [JsonConverter(typeof(JsonLobbySlotArrayConverter))]
    public ILobbySlot[] Slots { get; }

    public int TeamID { get; set; }

    public int Capacity { get; set; }

    public ILobbyHandle Handle => this.m_handle ?? throw new InvalidOperationException();

    public string TeamRole { get; set; }

    [JsonConstructor]
    public JsonLobbyTeam(ILobbySlot[] Slots, int TeamID, int Capacity, string TeamRole) { 
        this.Slots = Slots;
        this.TeamID = TeamID;
        this.Capacity = Capacity;
        this.TeamRole = TeamRole;
    }

    public void SetHandle(ILobbyHandle handle) {
        this.m_handle = handle;
        for (int i = 0; i < this.Slots.Length; i++) {
            this.Slots[i].SetHandle(handle);
        }
    }

    public bool IsMember(ulong memberID) {
        return this.GetSlotOfMember(memberID) is not null;
    }

    public ILobbySlot? GetSlotOfMember(ulong memberID) {
        for (int i = 0; i < this.Slots.Length; i++) {
            if (this.Slots[i].State == 1 && this.Slots[i].Occupant?.MemberID == memberID) {
                return this.Slots[i];
            }
        }
        return null;
    }

}

public class JsonLobbyTeamConverter : JsonConverter<ILobbyTeam> {
    public override ILobbyTeam? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<JsonLobbyTeam>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, ILobbyTeam value, JsonSerializerOptions options) {
        if (value is not JsonLobbyTeam json)
            throw new InvalidOperationException();
        JsonSerializer.Serialize(writer, json, options);
    }
}

public class JsonLobbyTeamArrayConverter : JsonConverter<ILobbyTeam[]> {
    public override ILobbyTeam[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<JsonLobbyTeam[]>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, ILobbyTeam[] value, JsonSerializerOptions options) {
        if (value is not JsonLobbyTeam[] json)
            throw new InvalidOperationException();
        JsonSerializer.Serialize(writer, json, options);
    }
}
