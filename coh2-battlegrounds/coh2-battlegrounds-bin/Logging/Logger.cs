using Battlegrounds.ErrorHandling;

using System;
using System.Diagnostics;
using System.Reflection;

namespace Battlegrounds.Logging;

/// <summary>
/// Class for writing log messages.
/// </summary>
public sealed class Logger {

    private static readonly string layout = "[{0}:{1}] {3}-{2}: {4}" + Environment.NewLine; // 0:Longdate, 1:Longtime, 2:level, 3:source, 4:message

    private readonly Type source;

    private Logger(Type source) {
        this.source = source;
    }

    /// <summary>
    /// Log the specified message to the console
    /// </summary>
    /// <param name="level">The level of log to make; Note that debug calls are ignored in release mode</param>
    /// <param name="message">The object to log. Special formatting rules may apply.</param>
    public void Log(LogLevel level, object? message) 
        => LogThis(level, LogObj(message, level));

    /// <summary>
    /// Logs the specified message; applying standard string formatting on the input message.
    /// </summary>
    /// <param name="level">The level of log to make; Note that debug calls are ignored in release mode</param>
    /// <param name="message">The message to log; accepting string formatting</param>
    /// <param name="args">String format arguments</param>
    public void Log(LogLevel level, string message, params object[] args)
        => LogThis(level, string.Format(message, args));

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The object to log. Special formatting rules may apply.</param>
    public void Info(object? message) 
        => LogThis(LogLevel.Info, LogObj(message, LogLevel.Info));

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The message to log; accepting string formatting</param>
    /// <param name="args">String format arguments</param>
    public void Info(string message, params object[] args)
        => LogThis(LogLevel.Info, string.Format(message, args));

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <remarks>
    /// Calls to this method are ignored in RELEASE mode.
    /// </remarks>
    /// <param name="message">The object to log. Special formatting rules may apply.</param>
    [Conditional("DEBUG")]
    public void Debug(object? message)
        => LogThis(LogLevel.Debug, LogObj(message, LogLevel.Debug));

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <remarks>
    /// Calls to this method are ignored in RELEASE mode.
    /// </remarks>
    /// <param name="message">The message to log; accepting string formatting</param>
    /// <param name="args">String format arguments</param>
    [Conditional("DEBUG")]
    public void Debug(string message, params object[] args)
        => LogThis(LogLevel.Debug, string.Format(message, args));

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The object to log. Special formatting rules may apply.</param>
    public void Warning(object? message)
        => LogThis(LogLevel.Warning, LogObj(message, LogLevel.Warning));

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log; accepting string formatting.</param>
    /// <param name="args">String format arguments.</param>
    public void Warning(string message, params object[] args)
        => LogThis(LogLevel.Warning, string.Format(message, args));

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The object to log. Special formatting rules may apply.</param>
    public void Error(object? message)
        => LogThis(LogLevel.Error, LogObj(message, LogLevel.Error));

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log; accepting string formatting</param>
    /// <param name="args">String format arguments</param>
    public void Error(string message, params object[] args)
        => LogThis(LogLevel.Error, string.Format(message, args));

    /// <summary>
    /// Triggers a <see cref="FatalAppException"/> instantiation and performs a formatted log event.
    /// </summary>
    /// <param name="message">The message to log; accepting string formatting.</param>
    /// <param name="args">String format arguments.</param>
    /// <returns>The created <see cref="FatalAppException"/> instance (The app will be terminating at this point!)</returns>
    public FatalAppException Fatal(string message, params object[] args) {
        string msg = string.Format(message, args);
        LogThis(LogLevel.Fatal, msg);
        return new FatalAppException(msg);
    }

    /// <summary>
    /// Logs an exception in a nice and readable format.
    /// </summary>
    /// <param name="e">The exception to log.</param>
    public void Exception(Exception e) {
        LogThis(LogLevel.Error, e.Message);
        BattlegroundsContext.Log!.Write(e.StackTrace);
        BattlegroundsContext.Log!.Write(Environment.NewLine);
    }

    // The actual method that passes the log message to the actual logger
    private void LogThis(LogLevel level, string msg) {
#if DEBUG
#else
        if (level is LogLevel.Debug) { return }
#endif
        string tolog = string.Format(layout,
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToLongTimeString(),
                level.ToString().ToUpperInvariant(),
                source.Name,
                msg);
        BattlegroundsContext.Log!.Write(tolog);
        Console.Write(tolog); // TOOD: check with global settings if this log is desired
#if DEBUG
        Trace.Write(tolog);
#endif
    }

    // Performs nice formatting of objects
    private static string LogObj(object? obj, LogLevel level) => obj switch {
        null => "<<NULL>>",
        ILogFormat fmt => fmt.GetLogString(level),
        _ => obj.ToString()!
    };

    /// <summary>
    /// Creates a logger instance for the calling class type.
    /// </summary>
    /// <remarks>
    /// This shold only be created once per class
    /// </remarks>
    /// <returns>A <see cref="Logger"/> instance tied to the calling class type.</returns>
    /// <exception cref="FatalAppException"></exception>
    public static Logger CreateLogger() {

        // Get logger type from calling frame
        StackFrame frame = new StackFrame(1);

        // Get the calling
        if (frame.GetMethod() is MethodBase method) {
            return new Logger(method.DeclaringType ?? throw new FatalAppException("Failed to initialise logger: " + frame.GetFileName()));
        }

        // Error anyway
        throw new FatalAppException("Failed to initialise logger: " + frame.GetFileName());

    }

    /// <summary>
    /// Create a logger instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the source to log for.</typeparam>
    /// <returns>A <see cref="Logger"/> instance tied to the calling class type.</returns>
    public static Logger Create<T>() => new Logger(typeof(T));

}
