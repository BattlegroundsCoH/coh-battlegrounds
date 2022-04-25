using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.Server;

/*
type Lobby struct {
	    Host         *LobbyMember            `json:"-"`
	    Participants map[uint64]*LobbyMember `json:"-"`
	    Mutex        *sync.RWMutex           `json:"-"`
	    Name         string
	    Mode         string // gamemode, map etc.
	    Status       string // playing, setting up etc.
	    Password     string `json:"-"` // Makes json serializer skip it
	    Capacity     int
	    Occupants    int    // The amount of registed players/ais
	    LobbyType    int    // if skirmish or campaign etc.
	    UID          uint64 // unique identifier
}
 */

/// <summary>
/// API representation of a server lobby.
/// </summary>
public struct ServerLobby {

    /// <summary>
    /// Get (or set) the actual name of the server lobby.
    /// </summary>
    [JsonPropertyName("LobbyName")]
    public string Name { get; set; }

    /// <summary>
    /// Get (or set) the numeric type 
    /// </summary>
    [JsonPropertyName("LobbyType")]
    public int Type { get; set; }

    /// <summary>
    /// Get (or set) the current amount of active members in the lobby (includes AI).
    /// </summary>
    [JsonPropertyName("Occupants")]
    public int Members { get; set; }

    /// <summary>
    /// Get (or set) the current capacity of the lobby.
    /// </summary>
    [JsonPropertyName("LobbyCapacity")]
    public int Capacity { get; set; }

    /// <summary>
    /// Get (or set) the current state of the lobby.
    /// </summary>
    [JsonPropertyName("LobbyPlayState")]
    public string State { get; set; }

    /// <summary>
    /// Get the translated version of <see cref="State"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="State"/> raw if <see cref="StringTranslator"/> is not defined.
    /// <br/>
    /// This is not saved in json.
    /// </remarks>
    [JsonIgnore]
    public string TranslatedState => this.StringTranslator?.Invoke(this.State, nameof(State)) ?? this.State;

    /// <summary>
    /// Get (or set) the current mode played by the lobby.
    /// </summary>
    [JsonPropertyName("LobbyPlaymode")]
    public string Mode { get; set; }

    /// <summary>
    /// Get the translated version of <see cref="Mode"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Mode"/> raw if <see cref="StringTranslator"/> is not defined.
    /// <br/>
    /// This is not saved in json.
    /// </remarks>
    [JsonIgnore]
    public string TranslatedMode => this.StringTranslator?.Invoke(this.Mode, nameof(Mode)) ?? this.Mode;

    /// <summary>
    /// Get (or set) the GUID associated with the lobby.
    /// </summary>
    [JsonPropertyName("LobbyId")]
    public ulong UID { get; set; }

    /// <summary>
    /// Get (or set) if the lobby is password-protected.
    /// </summary>
    [JsonPropertyName("HasPassword")]
    public bool HasPassword { get; set; }

    /// <summary>
    /// Get the string describing the filled slots of the lobby.
    /// </summary>
    /// <remarks>
    /// This is not serialised in Json.
    /// </remarks>
    [JsonIgnore]
    public string CapacityString => $"{this.Members}/{this.Capacity}";

    /// <summary>
    /// Get or set the translator functionality that will translate the server-stored value.
    /// </summary>
    /// <remarks>
    /// This is not serialised in Json and must be defined by the local machine.
    /// </remarks>
    [JsonIgnore]
    public ServerAPIResponseStringTranslator StringTranslator { get; set; }

}
