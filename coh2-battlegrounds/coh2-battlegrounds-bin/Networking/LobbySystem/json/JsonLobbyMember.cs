using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem.json;

public class JsonLobbyMember : ILobbyMember {

    private ILobbyHandle? m_handle;

    public ulong MemberID { get; set; }
    
    public string DisplayName { get; }
    
    public byte Role { get; }
    
    public byte AILevel { get; }
    
    public LobbyMemberState State { get; }

    [JsonConverter(typeof(JsonLobbyCompanyConverter))]
    public ILobbyCompany? Company { get; }

    [JsonIgnore]
    public ILobbyHandle Handle => this.m_handle ?? throw new InvalidOperationException();

    [JsonConstructor]
    public JsonLobbyMember(ulong MemberID, string DisplayName, byte Role, byte AILevel, LobbyMemberState State, ILobbyCompany? Company) { 
        this.MemberID = MemberID;
        this.DisplayName = DisplayName;
        this.Role = Role;
        this.AILevel = AILevel;
        this.State = State;
        this.Company = Company;
    }

    public void SetHandle(ILobbyHandle handle) { 
        this.m_handle = handle; 
        this.Company?.SetHandle(handle);
    }

}

public class JsonLobbyMemberConverter : JsonConverter<ILobbyMember> {
    public override ILobbyMember? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<JsonLobbyMember>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, ILobbyMember value, JsonSerializerOptions options) {
        if (value is not JsonLobbyMember json)
            throw new InvalidOperationException();
        JsonSerializer.Serialize(writer, json, options);
    }
}
