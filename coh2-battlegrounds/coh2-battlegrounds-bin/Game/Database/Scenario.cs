using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// The theatre of war a scenario is taking place in.
    /// </summary>
    public enum ScenarioTheatre {
        
        /// <summary>
        /// Axis vs Soviets
        /// </summary>
        EasternFront,
        
        /// <summary>
        /// Axis vs UKF & USF
        /// </summary>
        WesternFront,
        
        /// <summary>
        /// Axis vs Allies (Germany)
        /// </summary>
        SharedFront,

    }

    /// <summary>
    /// Represents a scenario. Implements <see cref="IJsonObject"/>. This class cannot be inherited.
    /// </summary>
    public sealed class Scenario : IJsonObject {

        /// <summary>
        /// The text-name for the <see cref="Scenario"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description text for the <see cref="Scenario"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The relative filename to the "mp" scenarios folder.
        /// </summary>
        public string RelativeFilename { get; set; }

        /// <summary>
        /// The max amount of players who can play on this map.
        /// </summary>
        public byte MaxPlayers { get; set; }

        /// <summary>
        /// The <see cref="ScenarioTheatre"/> this map takes place in.
        /// </summary>
        [JsonEnum(typeof(ScenarioTheatre))]
        public ScenarioTheatre Theatre { get; set; }

        /// <summary>
        /// Can this <see cref="Scenario"/> be considered a winter map.
        /// </summary>
        public bool IsWintermap { get; set; }

        /// <summary>
        /// The <see cref="Wincondition"/> instances designed for this <see cref="Scenario"/>. Empty list means all <see cref="Wincondition"/> instances can be used.
        /// </summary>
        [JsonReference] 
        public List<Wincondition> Gamemodes { get; set; }

        public string ToJsonReference() => this.RelativeFilename;

        public override string ToString() => this.Name;

    }

}
