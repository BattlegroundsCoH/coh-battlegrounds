using System;
using System.IO;

namespace Battlegrounds.Compiler.Essence;

/// <summary>
/// Interface representing an archiver tool to archive or extract CoH archive data.
/// </summary>
public interface IArchiver {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="archdef"></param>
    /// <param name="relativepath"></param>
    /// <param name="output"></param>
    /// <returns>When successfully archived, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    bool Archive(string archdef, string relativepath, string output);

    /// <summary>
    /// Extract the specified archive file to the specified output directory.
    /// </summary>
    /// <remarks>
    /// This method may not be supported for all games.
    /// </remarks>
    /// <param name="arcfile">The archive file to extract.</param>
    /// <param name="outpath">The output directory to extract contents to.</param>
    /// <returns>When successfully extracted, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException"/>
    bool Extract(string arcfile, string outpath);

    /// <summary>
    /// Extract the specified archive file to the specified output directory.
    /// </summary>
    /// <remarks>
    /// This method may not be supported for all games.
    /// </remarks>
    /// <param name="arcfile">The archive file to extract.</param>
    /// <param name="outpath">The output directory to extract contents to.</param>
    /// <param name="output">The output write that receives redirected information.</param>
    /// <returns>When successfully extracted, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    /// <exception cref="NotSupportedException"/>
    bool Extract(string arcfile, string outpath, TextWriter output);

}
