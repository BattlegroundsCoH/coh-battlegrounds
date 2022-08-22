using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Verification;

namespace Battlegrounds.Modding.Content.Companies;

/// <summary>
/// Class representing a company type for a specific faction.
/// </summary>
public class FactionCompanyType : IChecksumElement {

    /// <summary>
    /// Readonly struct representing a modifier to cost (or anything resource related).
    /// </summary>
    public readonly struct CostModifier {

        /// <summary>
        /// Get the manpower modifier.
        /// </summary>
        public float Manpower { get; }

        /// <summary>
        /// Get the munition modifier.
        /// </summary>
        public float Munition { get; }

        /// <summary>
        /// Get the fuel modifier.
        /// </summary>
        public float Fuel { get; }

        /// <summary>
        /// Initialise a new <see cref="CostModifier"/>.
        /// </summary>
        /// <param name="Manpower">The manpower modifier.</param>
        /// <param name="Munition">The munition modifier.</param>
        /// <param name="Fuel">The fuel modifier.</param>
        [JsonConstructor]
        public CostModifier(float Manpower, float Munition, float Fuel) {
            this.Manpower = Manpower;
            this.Munition = Munition;
            this.Fuel = Fuel;
        }
    }

    /// <summary>
    /// Readonly struct representing a transport option for a <see cref="FactionCompanyType"/>.
    /// </summary>
    public readonly struct TransportOption {
        
        /// <summary>
        /// Get the cost modifier for picking this transport option.
        /// </summary>
        public float CostModifier { get; }
        
        /// <summary>
        /// Get if this transport option is also available for towing.
        /// </summary>
        public bool Tow { get; }

        /// <summary>
        /// Get the cost modifier for picking this tow option.
        /// </summary>
        public float TowCostModifier { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CostModifier"></param>
        /// <param name="Tow"></param>
        [JsonConstructor]
        public TransportOption(float CostModifier, bool Tow, float TowCostModifier) {
            this.CostModifier = CostModifier;
            this.Tow = Tow;
            this.TowCostModifier = TowCostModifier;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Phase {

        /// <summary>
        /// 
        /// </summary>
        public int ActivationTime { get; init; }

        /// <summary>
        /// Get the additional deployment delay applied to unlocked units
        /// </summary>
        public float DeployDelay { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public CostModifier ResourceIncomeModifier { get; init; }

        /// <summary>
        /// Get the cost modifier applied to units that are unlocked in the previous phase
        /// </summary>
        public CostModifier UnitCostModifier { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public string[] Unlocks { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ActivationTime"></param>
        /// <param name="DeployDelay"></param>
        /// <param name="ResourceIncomeModifier"></param>
        /// <param name="UnitCostModifier"></param>
        /// <param name="Unlocks"></param>
        [JsonConstructor]
        public Phase(int ActivationTime, float DeployDelay, CostModifier ResourceIncomeModifier, CostModifier UnitCostModifier, string[] Unlocks) {
            this.ActivationTime = ActivationTime;
            this.DeployDelay = DeployDelay;
            this.ResourceIncomeModifier = ResourceIncomeModifier;
            this.UnitCostModifier = UnitCostModifier;
            this.Unlocks = Unlocks ?? Array.Empty<string>();
        }

    }

    private readonly Dictionary<string, DeploymentPhase> m_unitUnlocks;

    /// <summary>
    /// 
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string Icon { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public int MaxInfantry { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public int MaxTeamWeapons { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public int MaxVehicles { get; init; }
    
    /// <summary>
    /// 
    /// </summary>
    public int MaxLeaders { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public int MaxAbilities { get; init; }

    /// <summary>
    /// Get or initialise how many units can spawn in the initial phase.
    /// </summary>
    public int MaxInitialPhase { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string[] Exclude { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string[] DeployTypes { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, TransportOption> DeployBlueprints { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, Phase> Phases { get; init; }

    /// <summary>
    /// Get the checksum associated with the company type
    /// </summary>
    [JsonIgnore]
    public ulong Checksum => this.Id.ToCharArray().Fold(0ul, (s, c) => (s + 1) * c);

    /// <summary>
    /// Initialise a new <see cref="FactionCompanyType"/> instance.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Icon"></param>
    /// <param name="MaxInfantry"></param>
    /// <param name="MaxTeamWeapons"></param>
    /// <param name="MaxVehicles"></param>
    /// <param name="MaxLeaders"></param>
    /// <param name="MaxAbilities"></param>
    /// <param name="MaxInitialPhase"></param>
    /// <param name="Exclude"></param>
    /// <param name="DeployTypes"></param>
    /// <param name="DeployBlueprints"></param>
    /// <param name="Phases"></param>
    [JsonConstructor]
    public FactionCompanyType(string Id, string Icon, 
        int MaxInfantry, int MaxTeamWeapons, int MaxVehicles, int MaxLeaders, int MaxAbilities, int MaxInitialPhase,
        string[] Exclude, string[] DeployTypes, Dictionary<string, TransportOption> DeployBlueprints, Dictionary<string, Phase> Phases) {

        // Set properties
        this.Id = Id;
        this.Icon = Icon;
        this.MaxLeaders = MaxLeaders;
        this.MaxInfantry = MaxInfantry;
        this.MaxVehicles = MaxVehicles;
        this.MaxTeamWeapons = MaxTeamWeapons;
        this.MaxAbilities = MaxAbilities;
        this.MaxInitialPhase = MaxInitialPhase > 0 ? MaxInitialPhase : Company.DEFAULT_INITIAL;
        this.Exclude = Exclude;
        this.DeployTypes = DeployTypes;
        this.DeployBlueprints = DeployBlueprints;
        this.Phases = Phases;

        // Init internals
        this.m_unitUnlocks = new();
        foreach (var (phasename, phase) in this.Phases) {
            DeploymentPhase p = Enum.Parse<DeploymentPhase>(phasename);
            for (int i = 0; i < phase.Unlocks.Length; i++) {
                this.m_unitUnlocks[phase.Unlocks[i]] = p;
            }
        }

    }

    /// <summary>
    /// Get the earliest phase a unit is available in.
    /// </summary>
    /// <param name="blueprint">The squad blueprint to get earliest phase for.</param>
    /// <returns>
    /// The earliest phase for the unit. If the unit is excluded <see cref="DeploymentPhase.PhaseNone"/> is returned.
    /// If no unlock phase is specified for a unit, <see cref="DeploymentPhase.PhaseA"/>; Otherwise the specified <see cref="DeploymentPhase"/>.
    /// </returns>
    public DeploymentPhase GetEarliestPhase(SquadBlueprint blueprint) {

        // Ignore if excluded
        if (this.Exclude.Any(x => x == blueprint.Name))
            return DeploymentPhase.PhaseNone;

        // Try and look up in unit unlocks
        if (this.m_unitUnlocks.TryGetValue(blueprint.Name, out DeploymentPhase p))
            return p;

        // Return Phase A ==> Available by default
        return DeploymentPhase.PhaseA;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IList<SquadBlueprint> GetTowTransports() {
        List<SquadBlueprint> tows = new(); ;
        foreach (var (k, v) in this.DeployBlueprints) {
            if (v.Tow) {
                tows.Add(BlueprintManager.FromBlueprintName<SquadBlueprint>(k));
            }
        }
        return tows;
    }

}
