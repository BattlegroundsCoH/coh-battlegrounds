using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Networking.Server;

/* (Outdated as of 26th of April 2022)
type LobbyPublic struct {
	UID                 uint64
	Name                string
	Settings            map[string]string
	IsPasswrodProtected bool
	IsObserversAllowed  bool
	Status              string
	Capacity            byte
	Occupants           byte
	Teams               [][]LobbyPublicTeamSlot
	Game                string
	AppVersion          []byte
}
type LobbyPublicTeamSlot struct {
	DisplayName string
	Army        string
	Difficulty  byte
	State       byte
}
 */

/// <summary>
/// API representation of a server lobby.
/// </summary>
public readonly struct ServerLobby {

	/// <summary>
	/// 
	/// </summary>
	public ulong UID { get; }

	/// <summary>
	/// 
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// 
	/// </summary>
	public Dictionary<string, string> Settings { get; }

	/// <summary>
	/// 
	/// </summary>
	public bool IsPasswrodProtected { get; }

	/// <summary>
	/// 
	/// </summary>
	public bool IsObserversAllowed { get; }

	/// <summary>
	/// 
	/// </summary>
	public string Status { get; }

	/// <summary>
	/// 
	/// </summary>
	public byte Capacity { get; }

	/// <summary>
	/// 
	/// </summary>
	public byte Occupants { get; }

	/// <summary>
	/// 
	/// </summary>
	public ServerSlot[][] Teams { get; }

	/// <summary>
	/// 
	/// </summary>
	public string CapacityString => $"{this.Occupants}/{this.Capacity}";

	/// <summary>
	/// 
	/// </summary>
	public string Mode => GameModeOption is "" 
		? $"{this.Settings[LobbyConstants.SETTING_MAP]}, {this.Settings[LobbyConstants.SETTING_GAMEMODE]}"
		: $"{this.Settings[LobbyConstants.SETTING_MAP]}, {this.Settings[LobbyConstants.SETTING_GAMEMODE]} ({GameModeOption})";

	/// <summary>
	/// 
	/// </summary>
	public string GameModeOption => this.Settings.GetValueOrDefault(LobbyConstants.SETTING_GAMEMODEOPTION, string.Empty);

	/// <summary>
	/// 
	/// </summary>
	public string Game { get; }

	/// <summary>
	/// 
	/// </summary>
	public byte[] AppVersion { get; }

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public GameCase GetGame() {
		if (Enum.TryParse(Game, true, out GameCase result)) {
			return result;
		}
		return GameCase.CompanyOfHeroes2;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="UID"></param>
    /// <param name="Name"></param>
    /// <param name="Settings"></param>
    /// <param name="IsPasswrodProtected"></param>
    /// <param name="IsObserversAllowed"></param>
    /// <param name="Status"></param>
    /// <param name="Capacity"></param>
    /// <param name="Occupants"></param>
    /// <param name="Teams"></param>
    /// <param name="Game"></param>
	/// <param name="AppVersion"></param>
    [JsonConstructor]
	public ServerLobby(ulong UID, string Name, Dictionary<string, string> Settings, bool IsPasswrodProtected, bool IsObserversAllowed, string Status,
		byte Capacity, byte Occupants, ServerSlot[][] Teams, string Game, byte[] AppVersion) { 
		this.UID = UID;
		this.Name = Name;
		this.IsPasswrodProtected = IsPasswrodProtected;
		this.IsObserversAllowed = IsObserversAllowed;
		this.Status = Status;
		this.Capacity = Capacity;
		this.Occupants = Occupants;

		// Create default settings
		if (Settings is null || Settings.Count is 0) {
			this.Settings = new() {
				[LobbyConstants.SETTING_MAP] = "None",
				[LobbyConstants.SETTING_GAMEMODEOPTION] = "",
				[LobbyConstants.SETTING_GAMEMODE] = "None"
			};
        } else {
			this.Settings = Settings;
		}

		// Create default teams
		if (Teams is null || Teams.Length is 0) {
			this.Teams = new ServerSlot[3][] {
				new ServerSlot[] { ServerSlot.None, ServerSlot.None, ServerSlot.None, ServerSlot.None  },
				new ServerSlot[] { ServerSlot.None, ServerSlot.None, ServerSlot.None, ServerSlot.None },
                Array.Empty<ServerSlot>()
            };
        } else {
			this.Teams = Teams;
        }

		this.Game = Game;
		this.AppVersion = AppVersion;

	}

}

/// <summary>
/// 
/// </summary>
public readonly struct ServerSlot {

	/// <summary>
	/// 
	/// </summary>
	public static readonly ServerSlot None = new("", "", 0, 0);

	/// <summary>
	/// 
	/// </summary>
	public string DisplayName { get; }

	/// <summary>
	/// 
	/// </summary>
	public string Army { get; }

	/// <summary>
	/// 
	/// </summary>
	public byte Difficulty { get; }

	/// <summary>
	/// 
	/// </summary>
	public byte State { get; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="DisplayName"></param>
	/// <param name="Army"></param>
	/// <param name="Difficulty"></param>
	/// <param name="State"></param>
	[JsonConstructor]
	public ServerSlot(string DisplayName, string Army, byte Difficulty, byte State) {
		this.DisplayName = DisplayName;
		this.Army = Army;
		this.Difficulty = Difficulty;
		this.State = State;
	}

}
