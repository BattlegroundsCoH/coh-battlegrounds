using Battlegrounds.Logging;

using System;

namespace Battlegrounds.ErrorHandling;

/// <summary>
/// Exception class representing a fatal application exception. Instantiating this call will force the application to shut down.
/// </summary>
public sealed class FatalAppException : Exception {

    private static readonly Logger logger = Logger.Create<FatalAppException>();

    private const string DefaultMessage = "Fatal application error occured and the application will now shut down.";

    /// <summary>
    /// Create a new <see cref="FatalAppException"/> exception instance with a default exception message.
    /// </summary>
    /// <remarks>This will make a call to <see cref="Environment.Exit(int)"/>.</remarks>
    public FatalAppException() : base(DefaultMessage) => this.Crash(DefaultMessage);

    /// <summary>
    /// Create a new <see cref="FatalAppException"/> exception instance with a specified message.
    /// </summary>
    /// <remarks>This will make a call to <see cref="Environment.Exit(int)"/>.</remarks>
    /// <param name="message">A message explaining the cause of the fatal application exception.</param>
    public FatalAppException(string message) : base(message) => this.Crash(message);

    private void Crash(string message) {
        logger.Error($"Fatal app error detected and application will now close ({message}).");
        Environment.Exit(0);
    }

}
