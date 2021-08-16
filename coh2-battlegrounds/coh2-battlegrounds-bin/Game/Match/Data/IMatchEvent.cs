using System;
using System.Globalization;

namespace Battlegrounds.Game.Match.Data {

    /// <summary>
    /// Interface for an ingame event.
    /// </summary>
    public interface IMatchEvent {

        /// <summary>
        /// Get the default number format in use by the game.
        /// </summary>
        public static readonly IFormatProvider NumberFormat = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// Get the single uppercase letter used to identify event type.
        /// </summary>
        public char Identifier { get; }

        /// <summary>
        /// Get the unique identifier for the event.
        /// </summary>
        public uint Uid { get; }

    }

}
