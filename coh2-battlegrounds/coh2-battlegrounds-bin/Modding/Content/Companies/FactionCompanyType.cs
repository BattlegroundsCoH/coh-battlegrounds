using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Lua.Generator.RuntimeServices;
using Battlegrounds.Verification;

namespace Battlegrounds.Modding.Content.Companies;

/// <summary>
/// Class representing a company type for a specific faction.
/// </summary>
public class FactionCompanyType : IChecksumElement {

    /// <summary>
    /// Readonly class representing a modifier to cost (or anything resource related).
    /// </summary>
    public class CostModifier {

        /// <summary>
        /// Get the manpower modifier.
        /// </summary>
        [LuaName("manpower")]
        public float Manpower { get; }

        /// <summary>
        /// Get the munition modifier.
        /// </summary>
        [LuaName("munition")]
        public float Munition { get; }

        /// <summary>
        /// Get the fuel modifier.
        /// </summary>
        [LuaName("fuel")]
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
        /// Get the blueprint associated with the transport option.
        /// </summary>
        public string Blueprint { get; }

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
        /// Get the name of the phase this transport method first becomes available.
        /// </summary>
        public string AvailableInPhase { get; }

        /// <summary>
        /// Get the list of units that can be transported. If null or empty, all units are allowed.
        /// </summary>
        public string[] Units { get; }

        /// <summary>
        /// Initialise a new <see cref="TransportOption"/> instance.
        /// </summary>
        /// <param name="Blueprint"></param>
        /// <param name="CostModifier"></param>
        /// <param name="Tow"></param>
        /// <param name="TowCostModifier"></param>
        /// <param name="AvailableInPhase"></param>
        /// <param name="Units"></param>
        [JsonConstructor]
        public TransportOption(string Blueprint, float CostModifier, bool Tow, float TowCostModifier, string AvailableInPhase, string[] Units) {
            this.AvailableInPhase = AvailableInPhase;
            this.Blueprint = Blueprint;
            this.CostModifier = CostModifier;
            this.Tow = Tow;
            this.TowCostModifier = TowCostModifier;
            this.Units = Units ?? Array.Empty<string>();
        }

    }

    /// <summary>
    /// Class representing phase specific data.
    /// </summary>
    public class Phase {

        /// <summary>
        /// Get or initialise when the phase is activated.
        /// </summary>
        public int ActivationTime { get; init; }

        /// <summary>
        /// Get the additional deployment delay applied to unlocked units.
        /// </summary>
        public float DeployDelay { get; init; }

        /// <summary>
        /// Get or initialise the income modifier given to the player when entering the phase.
        /// </summary>
        public CostModifier ResourceIncomeModifier { get; init; }

        /// <summary>
        /// Get the cost modifier applied to units that are unlocked in the previous phase.
        /// </summary>
        public CostModifier UnitCostModifier { get; init; }

        /// <summary>
        /// Get or initialise the blueprints that become available in this phase.
        /// </summary>
        public string[] Unlocks { get; init; }

        /// <summary>
        /// Get or initialise the maximum amount of unts in this phase.
        /// </summary>
        /// <remarks>
        /// If none is specified the value is (<see cref="Company.MAX_SIZE"/> / 3) + 1
        /// </remarks>
        public int MaxPhase { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ActivationTime"></param>
        /// <param name="DeployDelay"></param>
        /// <param name="ResourceIncomeModifier"></param>
        /// <param name="UnitCostModifier"></param>
        /// <param name="Unlocks"></param>
        [JsonConstructor]
        public Phase(int ActivationTime, float DeployDelay, CostModifier ResourceIncomeModifier, CostModifier UnitCostModifier, 
            string[] Unlocks, int MaxPhase) {
            this.ActivationTime = ActivationTime;
            this.DeployDelay = DeployDelay;
            this.ResourceIncomeModifier = ResourceIncomeModifier;
            this.UnitCostModifier = UnitCostModifier;
            this.Unlocks = Unlocks ?? Array.Empty<string>();
            this.MaxPhase = MaxPhase <= 0 ? (Company.MAX_SIZE / 3 + 1) : MaxPhase;
        }

    }

    private readonly Dictionary<string, DeploymentPhase> m_unitUnlocks;
    private string m_typeId;

    /// <summary>
    /// 
    /// </summary>
    public string Id => this.m_typeId;

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
    public TransportOption[] DeployBlueprints { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, Phase> Phases { get; init; }

    /// <summary>
    /// Get or initialise the blueprint to use when recrewing team weapons
    /// </summary>
    public string TeamWeaponCrew { get; init; }

    /// <summary>
    /// Get the checksum associated with the company type
    /// </summary>
    [JsonIgnore]
    public ulong Checksum => this.Id.ToCharArray().Fold(0ul, (s, c) => (s + 1) * c);

    /// <summary>
    /// Get or set the faction data associated with this company type
    /// </summary>
    [JsonIgnore]
    public FactionData? FactionData { get; set; }

    /// <summary>
    /// Get or init the source file to load data from.
    /// </summary>
    public string SourceFile { get; init; }

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
        string[] Exclude, string[] DeployTypes, TransportOption[] DeployBlueprints, Dictionary<string, Phase> Phases,
        string TeamWeaponCrew, string SourceFile) {

        // Set properties
        this.m_typeId = Id;
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
        this.TeamWeaponCrew = TeamWeaponCrew;
        this.SourceFile = SourceFile;

        // Init internals
        this.m_unitUnlocks = new();
        if (this.Phases is not null) {
            foreach (var (phasename, phase) in this.Phases) {
                DeploymentPhase p = Enum.Parse<DeploymentPhase>(phasename);
                for (int i = 0; i < phase.Unlocks.Length; i++) {
                    this.m_unitUnlocks[phase.Unlocks[i]] = p;
                }
            }
        } else {
            this.Phases = new();
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
    public IList<SquadBlueprint> GetTowTransports() 
        => this.DeployBlueprints.Filter(x => x.Tow).Map(x => BlueprintManager.FromBlueprintName<SquadBlueprint>(x.Blueprint));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="phase"></param>
    /// <returns></returns>
    public int GetMaxInPhase(DeploymentPhase phase) {
        if (this.Phases.TryGetValue(phase.ToString(), out Phase? p)) {
            return p.MaxPhase;
        }
        return Company.MAX_SIZE / 3 + 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    public SquadBlueprint GetWeaponsCrew() {
        if (string.IsNullOrEmpty(this.TeamWeaponCrew)) {
            return BlueprintManager.FromBlueprintName<SquadBlueprint>(this.FactionData?.TeamWeaponCrew ?? throw new ObjectNotFoundException("Weapons crew squad is unspecified."));
        }
        return BlueprintManager.FromBlueprintName<SquadBlueprint>(this.TeamWeaponCrew);
    }

    internal void ChangeId(string id) => this.m_typeId = id;

}
