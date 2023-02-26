namespace Battlegrounds.Logging;

/// <summary>
/// Enum representing different levels of logging.
/// </summary>
public enum LogLevel {

    /// <summary>
    /// Info log message.
    /// </summary>
    Info,

    /// <summary>
    /// Debug log message. This message is suppressed in release mode.
    /// </summary>
    Debug,

    /// <summary>
    /// Warning message (Not fatal, but something did not go as expected).
    /// </summary>
    Warning,

    /// <summary>
    /// Error message (Something fatal happened)
    /// </summary>
    Error,

    /// <summary>
    /// Fatal message (Something fatal happened, application cannot continue)
    /// </summary>
    Fatal,

}
