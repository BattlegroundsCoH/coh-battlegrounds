using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Game.Match.Play;
using Battlegrounds.Game.Match.Startup;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Interface for an object capable of starting a Company of Heroes 2 Battlegrounds match.
/// </summary>
public interface IMatchStarter {

    /// <summary>
    /// Get if the match has started.
    /// </summary>
    bool HasStarted { get; }

    /// <summary>
    /// Get if the match startup has been cancelled.
    /// </summary>
    bool IsCancelled { get; }

    /// <summary>
    /// Get the created <see cref="IPlayStrategy"/> object created.
    /// </summary>
    /// <remarks>
    /// Only valid if <see cref="IsCancelled"/> is <see langword="false"/> and <see cref="HasStarted"/> is <see langword="true"/>.
    /// </remarks>
    IPlayStrategy PlayObject { get; }

    /// <summary>
    /// Begin the startup process.
    /// </summary>
    /// <param name="startupStrategy">The given <see cref="IStartupStrategy"/> to use when starting the match.</param>
    void Startup(IStartupStrategy startupStrategy);

}
