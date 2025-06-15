using System.Collections.Generic;

namespace Battlegrounds.Compiler;

/// <summary>
/// Interface representing an archiver tool
/// </summary>
public interface IArchiver {
    
    /// <summary>
    /// Archive the given archive definition.
    /// </summary>
    /// <param name="archiveDefinitionPath">The relative path of the archive definition</param>
    /// <returns>When archived successfully, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    bool Archive(string archiveDefinitionPath);

    /// <summary>
    /// Archive the given archive definition with additional tool argments.
    /// </summary>
    /// <param name="archiveDefinitionPath">The relative path of the archive definition</param>
    /// <param name="optionalArguments">Optional arguments to give to the compiler</param>
    /// <returns>When archived successfully, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    bool Archive(string archiveDefinitionPath, Dictionary<string, string> optionalArguments);

}
