using System;
using System.Collections.Generic;
using System.Text;

using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Gameplay;

/// <summary>
/// 
/// </summary>
public class Player {

    /// <summary>
    /// 
    /// </summary>
    public uint ID { get; }

    public ulong SteamID { get; }

    /// <summary>
    /// 
    /// </summary>
    public uint TeamID { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Profile { get; }

    /// <summary>
    /// 
    /// </summary>
    public Faction Army { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsAIPlayer { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public ServerItem[] Skins { get; } = new ServerItem[3];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tID"></param>
    /// <param name="name"></param>
    /// <param name="faction"></param>
    /// <param name="aiprofile"></param>
    public Player(uint id, ulong sid, uint tID, string name, Faction faction, string aiprofile) {
        this.ID = id;
        this.SteamID = sid;
        this.TeamID = tID;
        this.Name = name;
        this.Army = faction;
        this.Profile = aiprofile;
        this.IsAIPlayer = false;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{Name} ({Army.Name})";

}
