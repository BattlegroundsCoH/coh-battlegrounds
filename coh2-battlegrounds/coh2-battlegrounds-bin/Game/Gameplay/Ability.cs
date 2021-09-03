using System.Text.Json.Serialization;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay.DataConverters;
using Battlegrounds.Lua.Generator.RuntimeServices;

namespace Battlegrounds.Game.Gameplay {

    /// <summary>
    /// Special ability category an ability may belong to.
    /// </summary>
    public enum AbilityCategory {

        /// <summary>
        /// The category is undefined (Will not be compiled).
        /// </summary>
        Undefined,

        /// <summary>
        /// Show up as a "commander" ability (Currently not compiled - CoDiEx 05/08/21).
        /// </summary>
        Default,

        /// <summary>
        /// Be included as an artillery upgrade.
        /// </summary>
        Artillery,

        /// <summary>
        /// Be included as an air support.
        /// </summary>
        AirSupport,

        /// <summary>
        /// The ability is a unit ability (Should not be compiled).
        /// </summary>
        Unit,

    }

    /// <summary>
    /// Represents a <see cref="Ability"/> ingame ability.
    /// </summary>
    [LuaConverter(typeof(AbilityConverter))]
    public class Ability {

        /// <summary>
        /// The <see cref="AbilityBlueprint"/> being granted by the <see cref="Ability"/>.
        /// </summary>
        public AbilityBlueprint ABP { get; }

        /// <summary>
        /// The <see cref="AbilityCategory"/> the <see cref="Ability"/> will belong to.
        /// </summary>
        public AbilityCategory Category { get; }

        /// <summary>
        /// The amount of uses a player has during each match.
        /// </summary>
        public int MaxUse { get; }

        /// <summary>
        /// Get or set the amount of times this special ability has been used.
        /// </summary>
        [JsonIgnore]
        public int UsedCount { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="Ability"/> with predefined <see cref="AbilityCategory"/> and use count.
        /// </summary>
        /// <param name="blueprint">The ability blueprint.</param>
        /// <param name="category">The category.</param>
        /// <param name="maxUse">The maximum amount of uses each match.</param>
        /// <param name="count">The amount of times this has been used.</param>
        [JsonConstructor]
        public Ability(AbilityBlueprint ABP, AbilityCategory Category, int MaxUse, int UsedCount = 0) {
            this.ABP = ABP;
            this.Category = Category;
            this.MaxUse = MaxUse;
            this.UsedCount = UsedCount;
        }

    }

}
