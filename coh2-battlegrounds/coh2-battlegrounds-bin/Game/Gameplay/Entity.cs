using System;

using Battlegrounds.Game.Database;

namespace Battlegrounds.Game.Gameplay;

/// <summary>
/// 
/// </summary>
[Obsolete("Not in use - may be removed later.")]
public class Entity {

    /// <summary>
    /// 
    /// </summary>
    public ushort EntityID { get; }

    /// <summary>
    /// 
    /// </summary>
    public Player PlayerOwner { get; }

    /// <summary>
    /// 
    /// </summary>
    public Blueprint Blueprint { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eId"></param>
    /// <param name="owner"></param>
    /// <param name="bp"></param>
    public Entity(ushort eId, Player owner, Blueprint bp) {
        this.EntityID = eId;
        this.PlayerOwner = owner;
        this.Blueprint = bp;
    }

}
