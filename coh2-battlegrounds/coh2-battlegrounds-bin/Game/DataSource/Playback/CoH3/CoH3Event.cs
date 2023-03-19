using System;

namespace Battlegrounds.Game.DataSource.Playback.CoH3;

/// <summary>
/// Class representing an event in a CoH3 match.
/// </summary>
public sealed class CoH3Event : IPlaybackEvent {
    
    ///<inheritdoc/>
    public string AttachedMessage { get; }

    ///<inheritdoc/>
    public byte PlayerID { get; }

    ///<inheritdoc/>
    public TimeSpan Timestamp { get; }

    ///<inheritdoc/>
    public byte Type { get; }

    ///<inheritdoc/>
    public GameEventType EventType { get; }

    /// <summary>
    /// Instantiate a new <see cref="CoH3Event"/> instacne.
    /// </summary>
    /// <param name="timestamp">The timestamp of the event.</param>
    /// <param name="type">The event type.</param>
    /// <param name="playerId">The player ID type.</param>
    /// <param name="eventType">The game even type.</param>
    /// <param name="attachedMessage">The attached string message.</param>
    public CoH3Event(TimeSpan timestamp, byte type, byte playerId, GameEventType eventType, string attachedMessage) {
        this.Timestamp = timestamp;
        this.PlayerID = playerId;
        this.EventType = eventType;
        this.AttachedMessage = attachedMessage;
        this.Type = type;
    }

}
