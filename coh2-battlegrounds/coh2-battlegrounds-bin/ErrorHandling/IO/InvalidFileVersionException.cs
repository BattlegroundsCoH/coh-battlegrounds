using System;

namespace Battlegrounds.ErrorHandling.IO;

/// <summary>
/// Represents errors where an invalid file version was detected.
/// </summary>
public class InvalidFileVersionException : Exception {

    /// <summary>
    /// Get array of accepted version representations.
    /// </summary>
    public object[] ValidVersions { get; }

    public InvalidFileVersionException() : base("Invalid file version") { 
        this.ValidVersions = Array.Empty<object>();
    }

    public InvalidFileVersionException(string message) : base(message) {
        this.ValidVersions = Array.Empty<object>();
    }

    public InvalidFileVersionException(string message, Exception innerException) : base(message, innerException) {
        this.ValidVersions = Array.Empty<object>();
    }

    public InvalidFileVersionException(params object[] args) : base($"Invalid file version. Supported versions: {string.Join(", ", args)}") {
        this.ValidVersions = args;
    }

    public InvalidFileVersionException(string message, params object[] args) : base(message) {
        this.ValidVersions = args;
    }

    public InvalidFileVersionException(string message, Exception innerException, params object[] args) : base(message, innerException) {
        this.ValidVersions = args;
    }

}
