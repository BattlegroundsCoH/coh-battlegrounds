using System.Collections.Generic;

namespace Battlegrounds.Compiler;

/// <summary>
/// 
/// </summary>
public interface IArchiver {
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="archiveDefinitionPath"></param>
    /// <returns></returns>
    bool Archive(string archiveDefinitionPath);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="archiveDefinitionPath"></param>
    /// <param name="optionalArguments"></param>
    /// <returns></returns>
    bool Archive(string archiveDefinitionPath, Dictionary<string, string> optionalArguments);

}
