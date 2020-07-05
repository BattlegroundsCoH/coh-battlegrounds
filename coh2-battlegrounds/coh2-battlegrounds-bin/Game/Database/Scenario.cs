using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// The theatre of war
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
        /// Axis vs Allies
        /// </summary>
        SharedFront,

    }

    /// <summary>
    /// Represents a scenario. Implements <see cref="IJsonObject"/>. This class cannot be inherited.
    /// </summary>
    public sealed class Scenario : IJsonObject {

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RelativeFilename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte MaxPlayers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ScenarioTheatre Theatre { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsWintermap { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonReference] 
        public List<Wincondition> Gamemodes { get; set; }

        public string ToJsonReference() => this.RelativeFilename;

    }

}
