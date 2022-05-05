using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem.json;

public class JsonLobbyPoll : ILobbyPoll {
    
    public Dictionary<ulong, bool> Responses { get; }
    
    public uint ResponseId { get; }
    
    public string PollId { get; }

    [JsonConstructor]
    public JsonLobbyPoll(Dictionary<ulong, bool> Responses, uint ResponseId, string PollId) { 
        this.Responses = Responses;
        this.ResponseId = ResponseId;
        this.PollId = PollId;
    }

}

public class JsonLobbyPollConverter : JsonConverter<ILobbyPoll> {
    public override ILobbyPoll? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<JsonLobbyPoll>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, ILobbyPoll value, JsonSerializerOptions options) {
        if (value is not JsonLobbyPoll json)
            throw new InvalidOperationException();
        JsonSerializer.Serialize(writer, json, options);
    }
}
