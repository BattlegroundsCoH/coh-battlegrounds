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
    /// 
    /// </summary>
    /// <param name="jsonOutputFile"></param>
    void DumpJson(string jsonOutputFile);

}
