namespace Battlegrounds.Logging;

/// <summary>
/// Interface for objects that require custom log formatting.
/// </summary>
public interface ILogFormat {

    /// <summary>
    /// Get the log string representation of the object.
    /// </summary>
    /// <param name="level">The level of logging being invoked.</param>
    /// <returns>A logging <see cref="string"/> representation of the object.</returns>
    public string GetLogString(LogLevel level);

}
