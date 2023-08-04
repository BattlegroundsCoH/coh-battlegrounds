using System.Collections.Generic;

namespace Battlegrounds.Game.DataSource.Gamedata.CoH3;

/// <summary>
/// 
/// </summary>
/// <param name="Type"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="Version"></param>
/// <param name="Chunks"></param>
/// <param name="Body"></param>
public record Chunk(ChunkyType Type, string Name, string Description, int Version, IList<IChunk> Chunks, byte[] Body) : IChunk;
