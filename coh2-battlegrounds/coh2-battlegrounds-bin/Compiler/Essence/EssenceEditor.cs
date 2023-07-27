using System;
using System.IO;

using Battlegrounds.Logging;

namespace Battlegrounds.Compiler.Essence;

/// <summary>
/// Class representing the Essence Editor that can be used to archive files.
/// </summary>
public sealed class EssenceEditor : IArchiver {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <inheritdoc/>
    public bool Extract(string arcfile, string outpath) => throw new NotSupportedException("Extracting data using the Essence Editor is not allowed.");

    /// <inheritdoc/>
    public bool Extract(string arcfile, string outpath, TextWriter output) => throw new NotSupportedException("Extracting data using the Essence Editor is not allowed.");

    /// <inheritdoc/>
    public bool Archive(string archdef, string relativepath, string output) {
        throw new NotImplementedException();
    }

}
