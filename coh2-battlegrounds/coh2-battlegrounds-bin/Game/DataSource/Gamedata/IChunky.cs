using System.IO;

namespace Battlegrounds.Game.DataSource.Gamedata;

/// <summary>
/// 
/// </summary>
public interface IChunky {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    bool Load(BinaryReader reader);

    /// <summary>
    /// Walks the <see cref="IChunky"/> and the sub chunks, following the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Ordered string path to walk of the file.</param>
    /// <returns>The <see cref="IChunk"/> at the end of the path or <see langword="null"/>.</returns>
    IChunk? Walk(params string[] path);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsonOutputFile"></param>
    void DumpJson(string jsonOutputFile);

}
