using System;
using System.Diagnostics;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

    /// <summary>
    /// Enum representing the type of object that is captured.
    /// </summary>
    public enum CaptureEventType {

        /// <summary>
        /// The captured object is currently unknown.
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// The captured type was a vehicle.
        /// </summary>
        VEHICLE,

        /// <summary>
        /// The captured type was a weapon.
        /// </summary>
        WEAPON

    }

    /// <summary>
    /// <see cref="IMatchEvent"/> implementation for the event of equipment being captured.
    /// </summary>
    public class CaptureEvent : IMatchEvent {

        public char Identifier => 'T';

        public uint Uid { get; }

        /// <summary>
        /// Get the <see cref="Player"/> who triggered the capture event.
        /// </summary>
        public Player CapturingPlayer { get; }

        /// <summary>
        /// Get the type of blueprint that was captured.
        /// </summary>
        public BlueprintType CapturedBlueprintType { get; }

        /// <summary>
        /// Get the capture type this event represents.
        /// </summary>
        public CaptureEventType CaptureType { get; }

        /// <summary>
        /// Get the <see cref="Blueprint"/> that was captured.
        /// </summary>
        public Blueprint CapturedBlueprint { get; }

        /// <summary>
        /// Create a new <see cref="CaptureEvent"/>.
        /// </summary>
        /// <param name="values">The string arguments containing capture specifics.</param>
        /// <param name="player">The <see cref="Player"/> that captured the equipment.</param>
        /// <exception cref="ArgumentException"/>
        public CaptureEvent(uint id, string[] values, Player player) {

            // Set id
            this.Uid = id;

            // Verify input size
            if (values.Length == 3) {

                // Set the player capturing the event.
                this.CapturingPlayer = player;

                // Parse capture event type
                if (int.TryParse(values[1], out int capType)) {
                    this.CaptureType = (CaptureEventType)capType;
                } else {
                    this.CaptureType = CaptureEventType.UNKNOWN;
                    Trace.WriteLine($"Failed to parse '{values[1]}' as capture event - setting to 0 by default.", nameof(CaptureEvent));
                }

                // Set properties
                this.CapturedBlueprintType = Enum.Parse<BlueprintType>(values[2]);
                this.CapturedBlueprint = BlueprintManager.FromBlueprintName(values[0], this.CapturedBlueprintType); // This may fail (return null).

            } else {

                // Throw argument out of range exception
                throw new ArgumentOutOfRangeException(nameof(values), values.Length, "Argument was not of valid size. Expected 3 elements in array.");

            }

        }

    }

}
