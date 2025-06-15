using System;
using System.Text;

using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Remoting;

namespace Battlegrounds.Networking.Communication.Golang;

/*
 * This file merely contains the C# versions of the communication structs (+some lightweight conversions) found in the .go file:
 * communication.go
 */

/*
type IntroMessage struct {
	Type          int           // The type of intro message being sent
	LobbyName     string        // The name of the lobby/campaign to join
	LobbyUID      uint64        // The UID of the lobby/campaign to join
	LobbyPassword string        // Lobby access code field if not join/host request
	InstanceType  int           // The type of lobby to create; File type if file transfer
	PlayerUID     Steam.SteamID // Required field, specified the ID of the Steam user we're interacting with
	PlayerName    string        // The string name of the player
	IdentityKey   []byte        // Key for identification (Currently not used)
	AppVersion    []byte        // The application version checksum (Used to check if a player is using the same version as host)
	ModGuid       string        // The GUID of the mod being used
	Game          string        // The Game to target with this request (See Server/Game/CoH.go)
}
 */

/// <summary>
/// Represents an introduction message to the server.
/// </summary>
public readonly struct IntroMessage {

	/// <summary>
	/// Get or init intro message request type
	/// </summary>
	public int Type { get; init; }

	/// <summary>
	/// Get or init the name of the lobby to join
	/// </summary>
	public string LobbyName { get; init; }

    /// <summary>
    /// Get or init the UID of the lobby.
    /// </summary>
    public ulong LobbyUID { get; init; }

    /// <summary>
    /// Get or init the password to use when joining. Set as <see cref="string.Empty"/> when no password is expected.
    /// </summary>
    public string LobbyPassword { get; init; }

	/// <summary>
	/// Get or init the lobby type. For now 1 is sufficient (Match-Lobby)
	/// </summary>
	public int InstanceType { get; init; }

	/// <summary>
	/// Get or init the Steam ID of the player connecting (Must be set!)
	/// </summary>
	public ulong PlayerUID { get; init; }

	/// <summary>
	/// Get or init the display name of the player connecting.
	/// </summary>
	public string PlayerName { get; init; }
    
	/// <summary>
	/// Identity key (for verifying user)
	/// </summary>
	public byte[] IdentityKey { get; init; }

	/// <summary>
	/// Get or initialise the application verison to use/require
	/// </summary>
	public byte[] AppVersion { get; init; }

	/// <summary>
	/// Get or initialise the mod guid to use
	/// </summary>
	public string ModGuid { get; init; }

	/// <summary>
	/// Get or initialise the game to use
	/// </summary>
	public string Game { get; init; }

}

/*
type Message struct {
	Mode          byte   // Message mode (forward, broadcast, broker)
	CID           uint32 // message ID - local to sender
	Target        uint64 // destination
	Sender        uint64 // sender of message
	ContentLength uint16 // Length of json message
	Content       []byte // actual json message
}
     */

/// <summary>
/// Represents the message target mode.
/// </summary>
public enum MessageMode : byte {

	/// <summary>
	/// Message should be forwared to specified recipient. (Obsolete)
	/// </summary>
	Forward = 0,

	/// <summary>
	/// Message should be broadcast to all. (Obsolete)
	/// </summary>
	Broadcast = 1,

	/// <summary>
	/// Message is direct communication with broker.
	/// </summary>
	Broker = 2,

	/// <summary>
	/// Message is direct call to logic to be invoked by the broker.
	/// </summary>
	BrokerCall = 3,

	/// <summary>
	/// Message is an upload case
	/// </summary>
	FileUpload = 4

}

/// <summary>
/// Represents a message that can be sent or received over a <see cref="ServerConnection"/>.
/// </summary>
public readonly struct Message {

	/// <summary>
	/// Get or init the message target mode.
	/// </summary>
	public MessageMode Mode { get; init; }

	/// <summary>
	/// Get or init the unique message chain ID.
	/// </summary>
	public uint CID { get; init; }

	/// <summary>
	/// Get or init the recipient of the message.
	/// </summary>
	public ulong Target { get; init; }

	/// <summary>
	/// Get or init the sender of the message (Should be self when sending!).
	/// </summary>
	public ulong Sender { get; init; }

	/// <summary>
	/// Get the length of the content of the message.
	/// </summary>
	public ushort ContentLength => (ushort)this.Content.Length;

	/// <summary>
	/// Get or init the content of the message.
	/// </summary>
	public byte[] Content { get; init; }

}

/*
type BrokerMessage struct {
	Request byte
	First   byte
	Second  string
}

const BROKER_REQUEST_DISCONNECT = 0
const BROKER_REQUEST_LOBBYUPDATE = 1

const BROKER_FIRST_CAPACITY = 0
const BROKER_FIRST_MODE = 1
const BROKER_FIRST_STATUS = 2
const BROKER_FIRST_MEMBERS = 3

*/

/// <summary>
/// Represents a direct request to the broker.
/// </summary>
public readonly struct BrokerRequestMessage {

	/// <summary>
	/// Get or init the request type.
	/// </summary>
	public BrokerRequestType Request { get; init; }

	/// <summary>
	/// Get or init the first broker value to set.
	/// </summary>
	public BrokerFirstVal First { get; init; }

	/// <summary>
	/// Get or init the value of the broker value.
	/// </summary>
	public string Second { get; init; }

	/// <summary>
	/// Get the request message as byte array.
	/// </summary>
	/// <returns>Array of bytes.</returns>
	public byte[] Bytes() => GoMarshal.JsonMarshal(this);

}

/// <summary>
/// Represents the request type that can be sent to the broker.
/// </summary>
public enum BrokerRequestType : byte {

	/// <summary>
	/// Disconnect from broker.
	/// </summary>
	Disconnect = 0,

	/// <summary>
	/// Update broker lobby value.
	/// </summary>
	[Obsolete("Should use a direct broker call.")]
	Update = 1

}

/// <summary>
/// Represents the value to ask broker to update.
/// </summary>
public enum BrokerFirstVal : byte {

	/// <summary>
	/// The capacity value
	/// </summary>
	[Obsolete("Use the broker call for this instead.")]
	Capacity,

	/// <summary>
	/// The mode value
	/// </summary>
	[Obsolete("Use the broker call for this instead.")]
	Mode,

	/// <summary>
	/// The status value
	/// </summary>
	[Obsolete("Use the broker call for this instead.")]
	Status,

	/// <summary>
	/// The member value
	/// </summary>
	[Obsolete("Use the broker call for this instead.")]
	Members

}

/*
type ContentMessageArg struct {
	DotNetType string
	Value      []byte
}
type ContentMessage struct {
	MessageType  int				 // The message type
	StrMsg       string				 // Simple string message
	Who          uint64				 // Who this message revolves around
	Kick         bool				 // Kick member
	Static       bool				 // Is static
	DotNetType   string				 // Target type
	DotNetTarget string				 // Target method/object
	RemoteAction byte                // The remote action to perform (0 = call, 1 = get, 2 = set)
	Arguments    []ContentMessageArg // Encoded as string array
}
 */

/// <summary>
/// Represents an argument in a <see cref="ContentMessage"/>.
/// </summary>
public readonly struct ContentMessageArg {

	/// <summary>
	/// Get or init the .NET type string name.
	/// </summary>
	public string DotNetType { get;init; }

	/// <summary>
	/// Get or init the binary value of the argument.
	/// </summary>
	public byte[] Value { get;init; }

	/// <summary>
	/// Encode argument as a <see cref="ContentMessageArg"/> instance.
	/// </summary>
	/// <param name="val">The argument to encode.</param>
	/// <returns>The encoded object as a <see cref="ContentMessageArg"/> instance.</returns>
	/// <exception cref="ArgumentException"></exception>
	public static ContentMessageArg Encode(object? val) {
		byte[] valBytes = val switch {
			null => Array.Empty<byte>(),
			string s => Encoding.UTF8.GetBytes(s),
			byte b => new byte[] { b },
			ushort v => BitConverter.GetBytes(v),
			uint v => BitConverter.GetBytes(v),
			ulong v => BitConverter.GetBytes(v),
			short v => BitConverter.GetBytes(v),
			int v => BitConverter.GetBytes(v),
			long v => BitConverter.GetBytes(v),
			float v => BitConverter.GetBytes(v),
			double v => BitConverter.GetBytes(v),
			char v => BitConverter.GetBytes(v),
			bool v => BitConverter.GetBytes(v),
			IObjectID s => Encoding.ASCII.GetBytes(s.ToString()),
			_ => throw new ArgumentException($"Invalid primitive type. ({val.GetType().FullName})", nameof(val))
		};
		return new ContentMessageArg() {
			DotNetType = val is null ? typeof(void).Name : val.GetType().Name,
			Value = valBytes
		};
        }

	/// <summary>
	/// Decode a <see cref="ContentMessageArg"/> to its .NET value.
	/// </summary>
	/// <param name="val">The content message argument to decode.</param>
	/// <returns>The .NET value of the argument if type is valid; Otherwise <see langword="null"/>.</returns>
	public static object? Decode(ContentMessageArg val) {
		return val.DotNetType switch {
			nameof(String) => Encoding.UTF8.GetString(val.Value),
			nameof(Byte) => val.Value[0],
			nameof(UInt16) => BitConverter.ToUInt16(val.Value),
			nameof(UInt32) => BitConverter.ToUInt32(val.Value),
			nameof(UInt64) => BitConverter.ToUInt64(val.Value),
			nameof(Int16) => BitConverter.ToInt16(val.Value),
			nameof(Int32) => BitConverter.ToInt32(val.Value),
			nameof(Int64) => BitConverter.ToInt64(val.Value),
			nameof(Single) => BitConverter.ToSingle(val.Value),
			nameof(Double) => BitConverter.ToDouble(val.Value),
			nameof(Char) => BitConverter.ToChar(val.Value),
			nameof(Boolean) => val.Value[0] != 0,
			nameof(StringID) => new StringID(Encoding.ASCII.GetString(val.Value)),
			nameof(ObjectKey) => new ObjectKey(Encoding.ASCII.GetString(val.Value)),
			_ => null
		};
	}
}

/// <summary>
/// Represents the message type of a <see cref="ContentMessage"/>.
/// </summary>
public enum ContentMessgeType : int {

	/// <summary>
	/// Message is error
	/// </summary>
	Error,

	/// <summary>
	/// Message is OK
	/// </summary>
	OK,

	/// <summary>
	/// Message is a disconnect
	/// </summary>
	Disconnect,

	/// <summary>
	/// Message is a join
	/// </summary>
	Join,

	/// <summary>
	/// Message is a remote broker call.
	/// </summary>
	RemoteCall,

	/// <summary>
	/// Message is a response to a remote broker call. 
	/// </summary>
	RemoteCallResponse,

	/// <summary>
	/// Message is a request to get a remote ID.
	/// </summary>
	GetRemoteID,

	/// <summary>
	/// Message is a response to a request to get a remote ID.
	/// </summary>
	GetRemoteIDResponse,

	/// <summary>
	/// Execute a query message.
	/// </summary>
	[Obsolete("Queries are no longer valid.")]
	ExecQuery,

	/// <summary>
	/// Response to query execute message.
	/// </summary>
	[Obsolete("Queries are no longer valid.")]
	ExecQueryResponse,

	/// <summary>
	/// Message is to send a chat message
	/// </summary>
	SendChatMessage,

}

/// <summary>
/// Represents a message containing content.
/// </summary>
public readonly struct ContentMessage {

	/// <summary>
	/// Get or init the content message type.
	/// </summary>
	public ContentMessgeType MessageType { get; init; }

	/// <summary>
	/// Get or init the raw string message.
	/// </summary>
	public string StrMsg { get; init; }

	/// <summary>
	/// Get or init who this message is concerning.
	/// </summary>
	public ulong Who { get; init; }

	/// <summary>
	/// Get or init if the kick flag of the message is set.
	/// </summary>
	public bool Kick { get; init; }

	/// <summary>
	/// Get or init if the static flag of the message is set.
	/// </summary>
	public bool Static { get; init; }

	/// <summary>
	/// Get or init the .NET type associated with the message.
	/// </summary>
	public string DotNetType { get; init; }

	/// <summary>
	/// Get or init the .NET target of the meesage.
	/// </summary>
	public string DotNetTarget { get; init; }

	/// <summary>
	/// Get or init the remote action associated with this message (get, set, or call)
	/// </summary>
	public byte RemoteAction { get; init; }

	/// <summary>
	/// Get or init the additionally associated arguments with the message.
	/// </summary>
	public ContentMessageArg[] Arguments { get; init; }

	/// <summary>
	/// Get or init a raw array of bytes.
	/// </summary>
	public byte[] Raw { get; init; }

}

/*
 
	type RemoteCallMessage struct {
		Method    string
		Arguments map[string]string
	}

 */

/// <summary>
/// Represents a message that invoke methods on a remote machine.
/// </summary>
public readonly struct RemoteCallMessage {
	
	/// <summary>
	/// Get or initialise the method to invoke.
	/// </summary>
	public string Method { get; init; }
	
	/// <summary>
	/// Get or initialise the arguments to give in string form.
	/// </summary>
	public string[] Arguments { get; init; }

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T">First tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public T Decode<T>()
		=> T<T>(0);

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2) Decode<T1, T2>()
		=> (T<T1>(0), T<T2>(1));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3) Decode<T1, T2, T3>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4) Decode<T1, T2, T3, T4>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <typeparam name="T5">Fifth tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4, T5) Decode<T1, T2, T3, T4, T5>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3), T<T5>(4));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <typeparam name="T5">Fifth tuple type.</typeparam>
	/// <typeparam name="T6">Sixth tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4, T5, T6) Decode<T1, T2, T3, T4, T5, T6>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3), T<T5>(4), T<T6>(5));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <typeparam name="T5">Fifth tuple type.</typeparam>
	/// <typeparam name="T6">Sixth tuple type.</typeparam>
	/// <typeparam name="T7">Seventh tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4, T5, T6, T7) Decode<T1, T2, T3, T4, T5, T6, T7>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3), T<T5>(4), T<T6>(5), T<T7>(6));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <typeparam name="T5">Fifth tuple type.</typeparam>
	/// <typeparam name="T6">Sixth tuple type.</typeparam>
	/// <typeparam name="T7">Seventh tuple type.</typeparam>
	/// <typeparam name="T8">Eigth tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4, T5, T6, T7, T8) Decode<T1, T2, T3, T4, T5, T6, T7, T8>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3), T<T5>(4), T<T6>(5), T<T7>(6), T<T8>(7));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <typeparam name="T5">Fifth tuple type.</typeparam>
	/// <typeparam name="T6">Sixth tuple type.</typeparam>
	/// <typeparam name="T7">Seventh tuple type.</typeparam>
	/// <typeparam name="T8">Eigth tuple type.</typeparam>
	/// <typeparam name="T9">Ninth tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9) Decode<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3), T<T5>(4), T<T6>(5), T<T7>(6), T<T8>(7), T<T9>(8));

	/// <summary>
	/// Decode the remote call message to specified arguments.
	/// </summary>
	/// <typeparam name="T1">First tuple type.</typeparam>
	/// <typeparam name="T2">Second tuple type.</typeparam>
	/// <typeparam name="T3">Third tuple type.</typeparam>
	/// <typeparam name="T4">Fourth tuple type.</typeparam>
	/// <typeparam name="T5">Fifth tuple type.</typeparam>
	/// <typeparam name="T6">Sixth tuple type.</typeparam>
	/// <typeparam name="T7">Seventh tuple type.</typeparam>
	/// <typeparam name="T8">Eigth tuple type.</typeparam>
	/// <typeparam name="T9">Ninth tuple type.</typeparam>
	/// <typeparam name="T10">Tenth tuple type.</typeparam>
	/// <returns>Decoded argument tuple.</returns>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) Decode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
		=> (T<T1>(0), T<T2>(1), T<T3>(2), T<T4>(3), T<T5>(4), T<T6>(5), T<T7>(6), T<T8>(7), T<T9>(8), T<T10>(9));

	private T T<T>(int i)
		=> (T)Convert.ChangeType(this.Arguments[i], typeof(T));

}

/// <summary>
/// Represents the stat of the upload.
/// </summary>
public enum UploadState : byte {

	/// <summary>
	/// The initial upload
	/// </summary>
	Init = 0,

	/// <summary>
	/// Mid-upload
	/// </summary>
	Chunk = 1,

	/// <summary>
	/// End of upload
	/// </summary>
	Terminate = 2
    
}

/// <summary>
/// Represents an upload call message.
/// </summary>
/// <param name="FileType">The type of file to upload.</param>
/// <param name="UploadState">The state of the upload.</param>
/// <param name="Content">The content of the upload.</param>
public record struct UploadCallMessage(byte FileType, UploadState UploadState, byte[] Content);
