using System;
using System.Diagnostics;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

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
        /// Get the capture type (1 = captured vehicle, 2 = captured weapon/equipment)
        /// </summary>
        public int CaptureType { get; }

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
            this.Uid = id;
            if (values.Length == 3) {
                this.CapturingPlayer = player;
                if (int.TryParse(values[1], out int capType)) {
                    this.CaptureType = capType;
                } else {
                    this.CaptureType = 1;
                    Trace.WriteLine($"Failed to parse '{values[1]}' as capture event - setting to 1 by default.", "CaptureEvent");
                }
                this.CapturedBlueprintType = Enum.Parse<BlueprintType>(values[2]);
                this.CapturedBlueprint = BlueprintManager.FromBlueprintName(values[0], this.CapturedBlueprintType); // This may fail (return null).
            } else {
                throw new ArgumentException("Argument was not of valid size.", nameof(values));
            }
        }

    }

}
