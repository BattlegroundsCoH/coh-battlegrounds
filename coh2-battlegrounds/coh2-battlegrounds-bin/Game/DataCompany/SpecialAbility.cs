using System;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Scar;
using Battlegrounds.Json;

namespace Battlegrounds.Game.DataCompany {

    /// <summary>
    /// Special ability category an ability may belong to.
    /// </summary>
    public enum SpecialAbilityCategory {

        /// <summary>
        /// The category is undefined (Will not be compiled).
        /// </summary>
        Undefined,

        /// <summary>
        /// Show up as a "commander" ability (Currently not compiled - CoDiEx 01/01/20).
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

    }

    /// <summary>
    /// Represents a <see cref="SpecialAbility"/> ingame ability. Implements <see cref="IJsonObject"/> and <see cref="IScarValue"/>.
    /// </summary>
    public struct SpecialAbility : IJsonObject, IScarValue {

        [JsonIgnoreIfValue(0)] private int m_useCount;

        /// <summary>
        /// The <see cref="AbilityBlueprint"/> being granted by the <see cref="SpecialAbility"/>.
        /// </summary>
        public AbilityBlueprint ABP { get; }

        /// <summary>
        /// The <see cref="SpecialAbilityCategory"/> the <see cref="SpecialAbility"/> will belong to.
        /// </summary>
        [JsonEnum(typeof(SpecialAbilityCategory))] public SpecialAbilityCategory Category { get; }

        /// <summary>
        /// The amount of uses a player has during each match.
        /// </summary>
        [JsonIgnore]
        public int Uses { get => m_useCount; set => m_useCount = value; }

        /// <summary>
        /// Instantiate a new <see cref="SpecialAbility"/> with predefined <see cref="SpecialAbilityCategory"/> and use count.
        /// </summary>
        /// <param name="blueprint">The ability blueprint.</param>
        /// <param name="category">The category.</param>
        /// <param name="maxUse">The maximum amount of uses each match.</param>
        public SpecialAbility(AbilityBlueprint blueprint, SpecialAbilityCategory category, int maxUse) {
            this.ABP = blueprint;
            this.Category = category;
            this.m_useCount = maxUse;
        }

        public string ToJsonReference() => throw new NotSupportedException();

        public string ToScar() => $"{{ abp = {this.ABP.ToScar()}, max_use = {this.m_useCount} }}";

    }

}
