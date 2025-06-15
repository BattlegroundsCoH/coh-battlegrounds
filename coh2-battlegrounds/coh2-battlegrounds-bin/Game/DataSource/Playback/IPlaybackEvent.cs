using System;

namespace Battlegrounds.Game.DataSource.Playback;

/// <summary>
/// Defines an interface for a playback event in a match.
/// </summary>
public interface IPlaybackEvent {

    /// <summary>
    /// Gets the attached message for this playback event.
    /// </summary>
    string AttachedMessage { get; }

    /// <summary>
    /// Gets the player ID associated with this playback event.
    /// </summary>
    byte PlayerID { get; }

    /// <summary>
    /// Gets the timestamp for this playback event.
    /// </summary>
    TimeSpan Timestamp { get; }

    /// <summary>
    /// Gets the type of this playback event.
    /// </summary>
    byte Type { get; }

    /// <summary>
    /// Gets the game event type associated with this playback event.
    /// </summary>
    GameEventType EventType { get; }

}
