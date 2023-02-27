namespace Battlegrounds.Game.DataSource.Gamedata;

/// <summary>
/// The type of an <see cref="IChunk"/>.
/// </summary>
public enum ChunkyType {

    /// <summary>
    /// Unknown (Invalid)
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Folder -> Has subtypes
    /// </summary>
    FOLD,

    /// <summary>
    /// Data chunk -> Contains raw data
    /// </summary>
    DATA,

}
