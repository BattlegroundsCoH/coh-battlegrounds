using System.ComponentModel;
using System.Text.Json.Serialization;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Modding.Content;

[JsonConverter(typeof(ModFactionAbilityLoader))]
public class FactionAbility { // Class because too many variables for it to make sense to being a struct

    public readonly struct VeterancyRequirement {
        public bool RequireVeterancy { get; }
        public float Veterancy { get; }
        public VeterancyRequirement(bool RequireVeterancy, float Veterancy) {
            this.RequireVeterancy = RequireVeterancy;
            this.Veterancy = Veterancy;
        }
    }

    public readonly struct AbilityVeterancy {
        public string ScreenName { get; }
        public float Experience { get; }
        public Modifier[] Modifiers { get; }
        public AbilityVeterancy(string ScreenName, float Experience, Modifier[] Modifiers) {
            this.ScreenName = ScreenName;
            this.Experience = Experience;
            this.Modifiers = Modifiers;
        }
    }

    /// <summary>
    /// Get the blueprint name of the ability.
    /// </summary>
    public string Blueprint { get; }

    /// <summary>
    /// Get the blueprint name that is required for this ability to be available.
    /// </summary>
    [DefaultValue("")]
    public string LockoutBlueprint { get; }

    /// <summary>
    /// Get the ability category
    /// </summary>
    public AbilityCategory AbilityCategory { get; }

    /// <summary>
    /// Get the max use in a match (-1 = infinite)
    /// </summary>
    [DefaultValue(0)]
    public int MaxUsePerMatch { get; }

    /// <summary>
    /// Get if requires granting units to be off-map
    /// </summary>
    [DefaultValue(false)]
    public bool RequireOffmap { get; }

    /// <summary>
    /// Get the effectiveness multiplier when multiple units are off-map
    /// </summary>
    [DefaultValue(0.0f)]
    public float OffmapCountEffectivenesss { get; }

    /// <summary>
    /// Get if the ability grants veterancy.
    /// </summary>
    [DefaultValue(false)]
    public bool CanGrantVeterancy { get; }

    /// <summary>
    /// Get the ranks associated with this ability. Empty list means no special abilities.
    /// </summary>
    [DefaultValue(null)]
    public AbilityVeterancy[] VeterancyRanks { get; }

    /// <summary>
    /// Get the veterancy requirement on units before this ability can be used.
    /// </summary>
    [DefaultValue(null)]
    public VeterancyRequirement? VeterancyUsageRequirement { get; }

    /// <summary>
    /// Get the amount of experience granted after each use.
    /// </summary>
    [DefaultValue(0.0f)]
    public float VeterancyExperienceGain { get; }

    public FactionAbility(string Blueprint, string LockoutBlueprint, AbilityCategory AbilityCategory, int MaxUsePerMatch, bool RequireOffmap,
        float OffmapCountEffectivenesss, bool CanGrantVeterancy, AbilityVeterancy[] VeterancyRanks, VeterancyRequirement? VeterancyUsageRequirement,
        float VeterancyExperienceGain) {

        // Set properties
        this.Blueprint = Blueprint;
        this.LockoutBlueprint = LockoutBlueprint;
        this.AbilityCategory = AbilityCategory;
        this.MaxUsePerMatch = MaxUsePerMatch;
        this.RequireOffmap = RequireOffmap;
        this.OffmapCountEffectivenesss = OffmapCountEffectivenesss;
        this.CanGrantVeterancy = CanGrantVeterancy;
        this.VeterancyRanks = VeterancyRanks;
        this.VeterancyUsageRequirement = VeterancyUsageRequirement;
        this.VeterancyExperienceGain = VeterancyExperienceGain;

    }

}
