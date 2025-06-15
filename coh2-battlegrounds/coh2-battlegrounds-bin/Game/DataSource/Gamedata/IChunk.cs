using System.Collections.Generic;

namespace Battlegrounds.Game.DataSource.Gamedata;

/// <summary>
/// 
/// </summary>
public interface IChunk {

    /// <summary>
    /// 
    /// </summary>
    ChunkyType Type { get; }

    /// <summary>
    /// 
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 
    /// </summary>
    int Version { get; }

    /// <summary>
    /// 
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 
    /// </summary>
    public IList<IChunk> Chunks { get; }

    /// <summary>
    /// 
    /// </summary>
    public byte[] Body { get; }

}
