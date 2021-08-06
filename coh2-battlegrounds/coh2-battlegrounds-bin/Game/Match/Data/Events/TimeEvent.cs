using System;

namespace Battlegrounds.Game.Match.Data.Events {

    /// <summary>
    /// <see cref="IMatchEvent"/> implementation for an event with a timestamp.
    /// </summary>
    public class TimeEvent : IMatchEvent {

        public char Identifier => 'A';

        public uint Uid => this.UnderlyingEvent.Uid;

        /// <summary>
        /// Get the underlying event that occured at this timestamp.
        /// </summary>
        public IMatchEvent UnderlyingEvent { get; }

        /// <summary>
        /// Get the timestamp where the event occured.
        /// </summary>
        public TimeSpan Timestamp { get; }

        /// <summary>
        /// Create a new <see cref="TimeEvent"/> with specified timestamp and the event that occured at specified time.
        /// </summary>
        /// <param name="timeSpan">The time of which this event occured.</param>
        /// <param name="encapsulatedEvent">The event that occured.</param>
        public TimeEvent(TimeSpan timeSpan, IMatchEvent encapsulatedEvent) {
            this.Timestamp = timeSpan;
            this.UnderlyingEvent = encapsulatedEvent;
        }

    }

}
