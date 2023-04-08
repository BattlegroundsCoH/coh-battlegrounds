using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding.Verifier;
using Battlegrounds.Util;
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
        public float Manpower { get; }

        /// <summary>
        /// Get the munition modifier.
        /// </summary>
        public float Munitions { get; }

        /// <summary>
        /// Get the fuel modifier.
        /// </summary>
        public float Fuel { get; }

        /// <summary>
        /// Get the fuel modifier.
        /// </summary>
        public float FieldTime { get; }

        /// <summary>
        /// Initialise a new <see cref="CostModifier"/>.
        /// </summary>
        /// <param name="Manpower">The manpower modifier.</param>
        /// <param name="Munitions">The munition modifier.</param>
        /// <param name="Fuel">The fuel modifier.</param>
        /// <param name="FieldTime">The fieldtime modifier.</param>
        [JsonConstructor]
        public CostModifier(float Manpower, float Munitions, float Fuel, float FieldTime) {
            this.Manpower = Manpower;
            this.Munitions = Munitions;
            this.Fuel = Fuel;
            this.FieldTime = FieldTime;
        }

        public static implicit operator CostExtension(CostModifier modifier)
            => new(modifier.Manpower, modifier.Munitions, modifier.Fuel, modifier.FieldTime);

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
        public string? AvailableInRole { get; }

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
        public TransportOption(string Blueprint, float CostModifier, bool Tow, float TowCostModifier, string? AvailableInRole, string[]? Units) {
            this.AvailableInRole = AvailableInRole;
            this.Blueprint = Blueprint;
            this.CostModifier = CostModifier;
            this.Tow = Tow;
            this.TowCostModifier = TowCostModifier;
            this.Units = Units ?? Array.Empty<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool SupportsRole(DeploymentRole role) 
            => string.IsNullOrEmpty(this.AvailableInRole) || (role.ToString() == this.AvailableInRole);

    }

    /// <summary>
    /// 
    /// </summary>
    public readonly struct CompanyAbility {

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public float CostModifier { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="CostModifier"></param>
        public CompanyAbility(string Id, float CostModifier) {
            this.Id = Id;
            this.CostModifier = CostModifier;
        }

    }

    /// <summary>
    /// Class representing phase specific data.
    /// </summary>
    public class CommandLevel {

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
        /// If none is specified the value is <see cref="BattlegroundsDefine.COMPANY_ROLE_MAX"/>
        /// </remarks>
        public int MaxRole { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UnitCostModifier"></param>
        /// <param name="Unlocks"></param>
        /// <param name="MaxPhase"></param>
        [JsonConstructor]
        public CommandLevel(CostModifier UnitCostModifier, string[] Unlocks, int MaxRole) {
            this.UnitCostModifier = UnitCostModifier;
            this.Unlocks = Unlocks ?? Array.Empty<string>();
            this.MaxRole = MaxRole <= 0 ? BattlegroundsDefine.COMPANY_ROLE_MAX : MaxRole;
        }

    }

    /// <summary>
    /// Readonly struct representing UI elements for the faction company type.
    /// </summary>
    public readonly struct UI {

        /// <summary>
        /// Get or initialise the type icon.
        /// </summary>
        public string Icon { get; init; }

        /// <summary>
        /// Get or initialise the array of units to highlight.
        /// </summary>
        public string[] HighlightUnits { get; init; }

        /// <summary>
        /// Get or initialise the array of abilities to highlight.
        /// </summary>
        public string[] HighlightAbilities { get; init; }

        /// <summary>
        /// Initialise a new <see cref="UI"/> instance.
        /// </summary>
        /// <param name="Icon">The icon.</param>
        /// <param name="HighlightUnits">Highlight units.</param>
        /// <param name="HighlightAbilities">Highlight abilities.</param>
        public UI(string? Icon, string[]? HighlightUnits, string[]? HighlightAbilities) {
            this.Icon = string.IsNullOrEmpty(Icon) ? "ct_unspecified" : Icon;
            this.HighlightAbilities = HighlightAbilities ?? Array.Empty<string>();
            this.HighlightUnits = HighlightUnits ?? Array.Empty<string>();
        }

    }

    private readonly Dictionary<string, DeploymentRole> m_unitUnlocks;
    private readonly Dictionary<string, int> m_unitTransports;
    private readonly Dictionary<DeploymentRole, CostModifier> m_roleModifier;
    private string m_typeId;

    /// <summary>
    /// 
    /// </summary>
    public string Id => this.m_typeId;

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
    public Dictionary<string, CommandLevel> Roles { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<string, CostModifier> CostModifiers { get; init; }

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
    /// Get or init if the company type is currently hidden (should be ignored)
    /// </summary>
    public bool Hidden { get; init; }

    /// <summary>
    /// Get or init the UI data for the company type.
    /// </summary>
    public UI UIData { get; init; }

    /// <summary>
    /// Get or init the ability data for the company type.
    /// </summary>
    public CompanyAbility[] Abilities { get; init; }

    /// <summary>
    /// Initialise a new <see cref="FactionCompanyType"/> instance.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="UIData"></param>
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
    public FactionCompanyType(string Id, UI UIData, CompanyAbility[]? Abilities,
        int MaxInfantry, int MaxTeamWeapons, int MaxVehicles, int MaxLeaders, int MaxAbilities, int MaxInitialPhase,
        string[]? Exclude, string[]? DeployTypes, TransportOption[]? DeployBlueprints, Dictionary<string, CommandLevel>? Roles,
        Dictionary<string, CostModifier>? CostModifiers, string TeamWeaponCrew, string SourceFile, bool Hidden) {

        // Set properties
        this.m_typeId = Id;
        this.UIData = UIData;
        this.Abilities = Abilities ?? Array.Empty<CompanyAbility>();
        this.MaxLeaders = MaxLeaders;
        this.MaxInfantry = MaxInfantry;
        this.MaxVehicles = MaxVehicles;
        this.MaxTeamWeapons = MaxTeamWeapons;
        this.MaxAbilities = MaxAbilities;
        this.MaxInitialPhase = MaxInitialPhase > 0 ? MaxInitialPhase : BattlegroundsDefine.COMPANY_DEFAULT_INITIAL;
        this.Exclude = Exclude ?? Array.Empty<string>();
        this.DeployTypes = DeployTypes ?? Array.Empty<string>();
        this.DeployBlueprints = DeployBlueprints ?? Array.Empty<TransportOption>();
        this.Roles = Roles ?? new();
        this.CostModifiers = CostModifiers ?? new();
        this.TeamWeaponCrew = TeamWeaponCrew;
        this.SourceFile = SourceFile;
        this.Hidden = Hidden;

        // Init internals
        this.m_unitUnlocks = new();
        this.m_roleModifier = new();
        this.m_unitTransports = new();
        if (this.Roles is not null) {
            foreach (var (phasename, phase) in this.Roles) {
                if (!Enum.TryParse(phasename, out DeploymentRole p)) {
                    Trace.WriteLine($"Company type '{this.Id}' has an invalid role '{phasename}'.", nameof(CompanyTypeWarnings));
                    continue;
                }
                for (int i = 0; i < phase.Unlocks.Length; i++) {
                    this.m_unitUnlocks[phase.Unlocks[i]] = p;
                }
                this.m_roleModifier[p] = phase.UnitCostModifier;
            }
            for (int i = 0; i < this.DeployBlueprints.Length; i++) {
                this.m_unitTransports[this.DeployBlueprints[i].Blueprint] = i;
            }
        } else {
            this.Roles = new();
        }

    }

    /// <summary>
    /// Get the role a unit is available in.
    /// </summary>
    /// <param name="blueprint">The squad blueprint to get role for.</param>
    /// <returns>
    /// The role for the unit. If the unit is excluded <see cref="DeploymentRole.ReserveRole"/> is returned.
    /// If no role is specified for a unit, <see cref="DeploymentRole.DirectCommand"/>; Otherwise the specified <see cref="DeploymentRole"/>.
    /// </returns>
    public DeploymentRole GetUnitRole(SquadBlueprint blueprint) {

        // Ignore if excluded
        if (this.Exclude.Any(x => x == blueprint.Name))
            return DeploymentRole.ReserveRole;

        // Try and look up in unit unlocks
        if (this.m_unitUnlocks.TryGetValue(blueprint.Name, out DeploymentRole p))
            return p;

        // Return Phase A ==> Available by default
        return DeploymentRole.DirectCommand;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IList<SquadBlueprint> GetTowTransports() 
        => this.DeployBlueprints.Filter(x => x.Tow).Map(x => FactionData!.Package!.GetDataSource().GetBlueprints(FactionData.Game).FromBlueprintName<SquadBlueprint>(x.Blueprint));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="phase"></param>
    /// <returns></returns>
    public int GetMaxInRole(DeploymentRole role) {
        if (this.Roles.TryGetValue(role.ToString(), out CommandLevel? p)) {
            return p.MaxRole;
        }
        return BattlegroundsDefine.COMPANY_ROLE_MAX;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    public SquadBlueprint GetWeaponsCrew() {
        if (string.IsNullOrEmpty(this.TeamWeaponCrew)) {
            return FactionData!.Package!.GetDataSource().GetBlueprints(FactionData.Game)
                .FromBlueprintName<SquadBlueprint>(this.FactionData?.TeamWeaponCrew ?? throw new ObjectNotFoundException("Weapons crew squad is unspecified."));
        }
        return FactionData!.Package!.GetDataSource().GetBlueprints(FactionData.Game).FromBlueprintName<SquadBlueprint>(this.TeamWeaponCrew);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sbp"></param>
    /// <param name="ubps"></param>
    /// <param name="rank"></param>
    /// <param name="role"></param>
    /// <param name="transport"></param>
    /// <returns></returns>
    public CostExtension GetUnitCost(SquadBlueprint sbp, UpgradeBlueprint[] ubps, byte rank, DeploymentRole role, SquadBlueprint? transport) {
        
        // Get basic cost
        var result = sbp.Cost;
        
        // Add flat cost modifier
        if (this.CostModifiers.TryGetValue(sbp.Name, out CostModifier? mod)) {
            result += mod;
        }

        // Compute veterancy modifier
        float rankModifier = rank.Fold(1.0f, (_, v) => v + BattlegroundsDefine.VET_COSTMODIFIER);

        // Modify by rank
        result *= rankModifier;

        // Modify by role modifier if not limited to specified role
        result *= this.m_roleModifier[role];

        // Apply transport costs (if any)
        if (transport is SquadBlueprint tbp) {
            if (this.m_unitTransports.TryGetValue(tbp.Name, out int i)) {
                if (sbp.IsTeamWeapon)
                    result *= this.DeployBlueprints[i].TowCostModifier;
                else
                    result *= this.DeployBlueprints[i].CostModifier;
            }
        }

        // Add upgrade costs
        result += ubps.Fold(new CostExtension(), (s, x) => s + x.Cost);

        // Return resulting cost
        return result;

    }

    internal void ChangeId(string id) => this.m_typeId = id;

    /// <inheritdoc/>
    public override string ToString() => this.m_typeId;

}
