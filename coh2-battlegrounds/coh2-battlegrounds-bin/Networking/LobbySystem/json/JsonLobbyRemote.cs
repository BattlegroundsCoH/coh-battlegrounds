using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem.json;

public class JsonLobbyRemote {

    public ulong HostID { get; set; }

    [JsonConverter(typeof(JsonLobbyTeamArrayConverter))]
    public ILobbyTeam[] Teams { get; set; } = new ILobbyTeam[3];
    
    public Dictionary<string, string> Settings { get; set; } = new();

}
