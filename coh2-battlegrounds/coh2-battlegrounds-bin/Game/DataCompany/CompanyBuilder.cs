using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany.Builder;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Builder class for building a <see cref="Company"/>. Inherit to extend functionality and add support for modded companies. 
/// This class is intended to be used for method chaining (But is not required for use).
/// </summary>
public class CompanyBuilder : IBuilder<Company> {

    // Record detailing the current setup of the company
    // This is very much meant for functional style changes
    public record BuildableCompany(
        string Name,
        FactionCompanyType Type,
        ModGuid ModGuid,
        Faction Faction,
        UnitBuilder[] Units,
        Ability[] Abilities,
        CompanyItem[] Items,
        bool AutoReinforce);

    public sealed record RemoveUnitAction(UnitBuilder Builder) : IEditAction<BuildableCompany> {
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Units = target.Units.Except(this.Builder)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Units = target.Units.Append(this.Builder)
        };
    }

    public sealed record AddUnitAction(UnitBuilder Builder) : IEditAction<BuildableCompany> {
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Units = target.Units.Append(this.Builder)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Units = target.Units.Except(this.Builder)
        };
    }

    public sealed record RemoveAbilityAction(Ability Ability) : IEditAction<BuildableCompany> {
        private Ability? m_removedAbility; // cache ability incase we need to undo this
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Abilities = target.Abilities.Except(this.m_removedAbility = target.Abilities.First(x => x == Ability))
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Abilities = this.m_removedAbility is null ? target.Abilities : target.Abilities.Append(this.m_removedAbility)
        };
    }

    public sealed record AddAbilityAction(Ability Ability) : IEditAction<BuildableCompany> {
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Abilities = target.Abilities.Append(this.Ability)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Abilities = target.Abilities.Except(this.Ability)
        };
    }

    public sealed record RemoveEquipmentAction(CompanyItem Equipment) : IEditAction<BuildableCompany> {
        private CompanyItem? m_removeEquipment; // cache ability incase we need to undo this
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Items = target.Items.Except(this.m_removeEquipment = target.Items.First(x => x == this.Equipment))
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Items = this.m_removeEquipment is null ? target.Items : target.Items.Append(this.m_removeEquipment)
        };
    }

    public sealed record AddEquipmentAction(CompanyItem Equipment) : IEditAction<BuildableCompany> {
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Items = target.Items.Append(this.Equipment)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Items = target.Items.Except(this.Equipment)
        };
    }

    public sealed record RenameAction(string Name) : IEditAction<BuildableCompany> {
        private string m_prevName = "";
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Name = this.Name.And(() => this.m_prevName = target.Name)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Name = this.m_prevName
        };
    }

    public sealed record ModAction(ModGuid ModGuid) : IEditAction<BuildableCompany> {
        private ModGuid m_modGuid;
        public BuildableCompany Apply(BuildableCompany target) => target with {
            ModGuid = this.ModGuid.And(() => this.m_modGuid = target.ModGuid)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            ModGuid = this.m_modGuid
        };
    }

    public sealed record AutoReinforceAction(bool NewValue) : IEditAction<BuildableCompany> {
        private bool m_oldValue;
        public BuildableCompany Apply(BuildableCompany target) => target with {
            AutoReinforce = this.NewValue.And(() => this.m_oldValue = target.AutoReinforce)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            AutoReinforce = this.m_oldValue
        };
    }

    private Company? m_companyResult;
    private BuildableCompany m_target;

    private readonly Stack<IEditAction<BuildableCompany>> m_actions;
    private readonly Stack<IEditAction<BuildableCompany>> m_redoActions;
    private int m_changeCounter;

    /// <summary>
    /// Get or set the <see cref="CompanyAvailabilityType"/> of the company.
    /// </summary>
    public CompanyAvailabilityType AvailabilityType { get; set; }

    /// <summary>
    /// Get the current company type.
    /// </summary>
    public FactionCompanyType CompanyType => this.m_target.Type;

    /// <summary>
    /// Get or set the statistics of the company.
    /// </summary>
    public CompanyStatistics Statistics { get; set; }

    /// <summary>
    /// Get the resulting <see cref="Company"/>.
    /// </summary>
    /// <remarks>
    /// Is <see langword="null"/> if no call to <see cref="Commit"/> have been made.
    /// </remarks>
    public Company Result => this.m_companyResult ?? throw new InvalidOperationException("To obtain build result, Commit must have been invoked at least once first.");

    /// <summary>
    /// 
    /// </summary>
    public bool IsChanged => this.m_actions.Count + this.m_redoActions.Count > this.m_changeCounter;

    /// <summary>
    /// 
    /// </summary>
    public bool CanUndo => this.m_actions.Count > 0;

    /// <summary>
    /// 
    /// </summary>
    public bool CanRedo => this.m_redoActions.Count > 0;

    /// <summary>
    /// Get the current size of the company.
    /// </summary>
    public int Size => this.m_target.Units.Length;

    /// <summary>
    /// Get the current amount of abilities in the company.
    /// </summary>
    public int AbilityCount => this.m_target.Abilities.Length;

    /// <summary>
    /// Get the current infantry count.
    /// </summary>
    public int InfantryCount => this.m_target.Units.Count(x => x.Blueprint.Category is SquadCategory.Infantry);

    /// <summary>
    /// Get the current support unit count.
    /// </summary>
    public int SupportCount => this.m_target.Units.Count(x => x.Blueprint.Category is SquadCategory.Support);

    /// <summary>
    /// Get the current vehicle count.
    /// </summary>
    public int VehicleCount => this.m_target.Units.Count(x => x.Blueprint.Category is SquadCategory.Vehicle);

    /// <summary>
    /// Get the current leader count.
    /// </summary>
    public int LeaderCount => this.m_target.Units.Count(x => x.Blueprint.Category is SquadCategory.Leader);

    /// <summary>
    /// Get if the company currently has auto-reinforcement enabled
    /// </summary>
    public bool AutoReinforce =>
        this.m_actions.FirstOrDefault(x => x is AutoReinforceAction) is AutoReinforceAction a ? a.NewValue : (this.m_companyResult?.AutoReplenish ?? false);

    private CompanyBuilder(BuildableCompany company) {

        // Set private fields
        this.m_target = company;
        this.m_actions = new();
        this.m_redoActions = new();

        // Set default statistics
        this.Statistics = new();

    }

    /// <summary>
    /// Add a unit to the <see cref="Company"/> using a <see cref="UnitBuilder"/>.
    /// </summary>
    /// <param name="blueprint">The squad blueprint the new unit will have</param>
    /// <param name="builder">The function to build the unit.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder AddUnit(SquadBlueprint blueprint, Func<UnitBuilder, UnitBuilder> builder) {
        UnitBuilder bld = UnitBuilder.NewUnit(blueprint);
        this.AddUnit(builder(bld));
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual CompanyBuilder AddUnit(UnitBuilder builder)
        => this.ApplyAction(new AddUnitAction(builder));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ability"></param>
    public virtual void AddAbility(Ability ability)
        => this.ApplyAction(new AddAbilityAction(ability));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public virtual CompanyBuilder RemoveAbility(Ability ability)
        => this.ApplyAction(new RemoveAbilityAction(ability));

    /// <summary>
    /// Remove unit from with <paramref name="unitID"/> from the company.
    /// </summary>
    /// <param name="unitID">The ID of the unit to remove.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder RemoveUnit(UnitBuilder builder)
        => this.ApplyAction(new RemoveUnitAction(builder));

    /// <summary>
    /// Change the name of the <see cref="Company"/>.
    /// </summary>
    /// <param name="name">The new name to set.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder ChangeName(string name)
        => this.ApplyAction(new RenameAction(name));

    /// <summary>
    /// Change the associated <see cref="Guid"/> of the <see cref="Company"/>. (This will decide from where the blueprints can be drawn from).
    /// </summary>
    /// <param name="tuningGUID">The tuning mod GUID.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder ChangeTuningMod(ModGuid tuningGUID)
        => this.ApplyAction(new ModAction(tuningGUID));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blueprint"></param>
    public virtual CompanyBuilder AddEquipment(CompanyItem blueprint)
        => this.ApplyAction(new AddEquipmentAction(blueprint));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enable"></param>
    /// <returns></returns>
    public CompanyBuilder SetAutoReinforce(bool enable) => this.ApplyAction(new AutoReinforceAction(enable));

    /// <summary>
    /// Undo the most recent change.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Undo() {
        if (!this.CanUndo) {
            throw new InvalidOperationException("No actions to undo.");
        }
        var top = this.m_actions.Pop();
        this.m_target = top.Undo(this.m_target);
        this.m_redoActions.Push(top);
    }

    /// <summary>
    /// Redo the most recent action undone
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Redo() {
        if (!this.CanRedo) {
            throw new InvalidOperationException("No actions to redo");
        }
        var top = this.m_redoActions.Pop();
        this.m_target = top.Apply(this.m_target);
        this.m_actions.Push(top);
    }

    private CompanyBuilder ApplyAction(IEditAction<BuildableCompany> editAction) {

        // Add action to list of actions performed
        this.m_actions.Push(editAction);

        // Apply acction
        this.m_target= editAction.Apply(this.m_target);

        // Return self
        return this;

    }

    /// <summary>
    /// Get the amount of units in the specified deployment <paramref name="phase"/>.
    /// </summary>
    /// <param name="phase">The phase to fetch amount of units from.</param>
    /// <returns>The amount of units in specific phase</returns>
    public virtual int CountUnitsInPhase(DeploymentPhase phase)
        => this.m_target.Units.Count(x => x.Phase == phase);

    /// <summary>
    /// Get if the phase has capacity for more units in specified phase.
    /// </summary>
    /// <param name="phase">The phase to check if new units can be assiged to.</param>
    /// <returns>If phase has capaciy <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public virtual bool IsPhaseAvailable(DeploymentPhase phase, SquadBlueprint? blueprint = null) => (phase, this.CountUnitsInPhase(phase)) switch {
        // Check if there's space in initial AND the unit is available in phase A
        (DeploymentPhase.PhaseInitial, int x) => 
            x < this.CompanyType.MaxInitialPhase && (blueprint is null || (blueprint is not null && this.CompanyType.GetEarliestPhase(blueprint) is <=DeploymentPhase.PhaseA)),
        // Basic check on phases
        (DeploymentPhase.PhaseA, int x) when x < this.CompanyType.GetMaxInPhase(DeploymentPhase.PhaseA) => 
            (blueprint is null || (blueprint is not null && this.CompanyType.GetEarliestPhase(blueprint) is <=DeploymentPhase.PhaseA)),
        (DeploymentPhase.PhaseB, int x) when x < this.CompanyType.GetMaxInPhase(DeploymentPhase.PhaseA) => 
            (blueprint is null || (blueprint is not null && this.CompanyType.GetEarliestPhase(blueprint) is <=DeploymentPhase.PhaseB)),
        (DeploymentPhase.PhaseC, int x) when x < this.CompanyType.GetMaxInPhase(DeploymentPhase.PhaseA) => 
            (blueprint is null || (blueprint is not null && this.CompanyType.GetEarliestPhase(blueprint) is <=DeploymentPhase.PhaseC)),
        // Default to not available
        _ => false
    };

    /// <summary>
    /// Get the first deployment phase not fully occupied.
    /// </summary>
    /// <param name="minPhase">The minimum phase to begin checking from.</param>
    /// <returns>The first available deployment phase</returns>
    public virtual DeploymentPhase GetFirstAvailablePhase(DeploymentPhase minPhase) {
        if (IsPhaseAvailable(minPhase))
            return minPhase;
        return GetFirstAvailablePhase(((int)minPhase + 1));
    }

    /// <summary>
    /// Get the first deployment phase not fully occupied.
    /// </summary>
    /// <param name="minPhase">The minimum phase to begin checking from.</param>
    /// <returns>The first available deployment phase</returns>
    public virtual DeploymentPhase GetFirstAvailablePhase(int minPhase) {
        if (IsPhaseAvailable((DeploymentPhase)minPhase))
            return (DeploymentPhase)minPhase;
        return minPhase is (int)DeploymentPhase.PhaseC ? DeploymentPhase.PhaseNone : GetFirstAvailablePhase(minPhase + 1);
    }

    /// <summary>
    /// Get a list of available transport units based on the settings of the <see cref="CompanyBuilder"/>.
    /// </summary>
    /// <param name="isTow">Flag setting whether transport units should be for towing or not.</param>
    /// <returns>Array of blueprints for transport use.</returns>
    public virtual IList<SquadBlueprint> GetTransports(bool isTow) {

        // Grab list from company type
        var transports = isTow ? this.CompanyType.GetTowTransports() : this.CompanyType.DeployBlueprints.Map(v => BlueprintManager.FromBlueprintName<SquadBlueprint>(v.Blueprint));

        // Return the transports
        return transports;

    }

    /// <summary>
    /// Add a crew to a company item that is crewable.
    /// </summary>
    /// <remarks>
    /// This action cannot be undone
    /// </remarks>
    /// <param name="itemIndex">The index of the company item to crew.</param>
    /// <param name="crew">The <see cref="SquadBlueprint"/> of the crew that will take control of the team weapon.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    /// <exception cref="InvalidOperationException"/>
    public virtual CompanyBuilder CrewCompanyItem(uint itemIndex, SquadBlueprint crew) {

        // Grab item
        var item = this.m_target.Items.FirstOrDefault(x => x.ItemId == itemIndex);
        if (item is null) 
            throw new InvalidOperationException("Cannot crew a company item that does not exist");

        // Create unit
        var unit = (item.Item switch {
            SquadBlueprint sbp => UnitBuilder.NewUnit(sbp),
            EntityBlueprint ebp => UnitBuilder.NewUnit(this.CompanyType!.FactionData!.Package!.GetCaptureSquad(ebp, this.m_target.Faction)),
            _ => throw new NotImplementedException()
        }).SetDeploymentPhase(GetFirstAvailablePhase(DeploymentPhase.PhaseA));

        // Remove item and add unit
        this.m_target = this.m_target with {
            Items = this.m_target.Items.Except(item),
            Units = this.m_target.Units.Append(unit)
        };

        // Return self
        return this;

    }

    /// <summary>
    /// Commit all unsaved changes to the <see cref="Company"/> target instance.
    /// </summary>
    /// <remarks>
    /// Result can be extracted from the <see cref="Result"/> property.
    /// </remarks>
    /// <param name="arg">Optional argument. Ignored by this method.</param>
    /// <returns>
    /// The calling <see cref="CompanyBuilder"/> instance.
    /// </returns>
    [MemberNotNull(nameof(m_companyResult))]
    public virtual IBuilder<Company> Commit(object? arg = null) {

        // Update company fluff
        var company = new Company(this.m_target.Faction, this.m_target.Type);
        company.SetAvailability(this.AvailabilityType);
        company.SetAppVersion(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0");
        company.SetReinforcementsEnabled(this.m_target.AutoReinforce);
        company.Name = this.m_target.Name;
        company.TuningGUID = this.m_target.ModGuid;
        company.UpdateStatistics(_ => this.Statistics);

        // Loop over units and add to company
        foreach (var unit in this.m_target.Units)
            company.AddSquad(unit);

        // Loop over abilities and add to company
        foreach (var ability in this.m_target.Abilities)
            company.AddAbility(ability);

        // Loop over items and add to company
        foreach (var item in this.m_target.Items)
            company.AddInventoryItem(item);

        // Trigger checksum calculation
        company.CalculateChecksum();

        // Set as result
        this.m_companyResult = company;

        // Update change threshold
        this.m_changeCounter = this.m_actions.Count + this.m_redoActions.Count;

        // Return self.
        return this;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual bool HasAbility(string blueprint) => this.m_target.Abilities.Any(x => x.ABP.Name == blueprint);

    /// <summary>
    /// Get if the company under construction has a squad with specified <paramref name="blueprint"/>.
    /// </summary>
    /// <param name="blueprint">The name of the blueprint to check for.</param>
    /// <returns><see langword="true"/>, if <paramref name="blueprint"/> is found; Otherwise <see langword="false"/>.</returns>
    public virtual bool HasSquad(string blueprint) => this.m_target.Units.Any(x => x.Blueprint.Name == blueprint);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="squad"></param>
    public virtual void EachUnit(Action<UnitBuilder> action, Func<UnitBuilder, int> sort) => this.m_target.Units.OrderBy(sort).ForEach(action);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public virtual void EachUnit(Action<UnitBuilder> action) => this.EachUnit(action, x => x.OverrideIndex);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public virtual void EachAbility(Action<Ability, bool> action) {
        if (ModManager.GetPackageFromGuid(this.m_target.ModGuid) is ModPackage package) {
            Company.GetSpecialUnitAbilities(this.m_target.Faction, package, this.m_target.Units.Map(x => x.Blueprint).Distinct()).ForEach(x => action(x, true));
            this.m_target.Abilities.ForEach(x => action(x, false));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public virtual void EachItem(Action<CompanyItem> action) => this.m_target.Items.ForEach(action);

    /// <summary>
    /// Create a <see cref="CompanyBuilder"/> capable of constructing a mutated version of the given <paramref name="company"/>.
    /// </summary>
    /// <param name="company">The company to edit.</param>
    /// <returns>A <see cref="CompanyBuilder"/> reflecting the current version of the <paramref name="company"/>.</returns>
    public static CompanyBuilder EditCompany(Company company) {

        // Grab unit list as unit builders
        var units = company.Units.Map(UnitBuilder.EditUnit);

        // Create buildable variant
        var buildable = new BuildableCompany(company.Name, 
            company.Type, company.TuningGUID, company.Army, 
            units, company.Abilities.ToArray(), company.Inventory.ToArray(), company.AutoReplenish);

        // Return company buuilder instance
        return new CompanyBuilder(buildable) {
            AvailabilityType = company.AvailabilityType,
            Statistics = company.Statistics
        };

    }

    /// <summary>
    /// Create a new <see cref="Company"/> using a <see cref="CompanyBuilder"/> instance.
    /// </summary>
    /// <param name="name">The name of the company to be created.</param>
    /// <param name="type">The <see cref="FactionCompanyType"/> of the new company to be created.</param>
    /// <param name="availabilityType">The availability type of the company.</param>
    /// <param name="faction">The faction of the company</param>
    /// <param name="modGuid">The assoociated tuning pack mod guid.</param>
    /// <returns>A <see cref="CompanyBuilder"/> instance that can construct a new <see cref="Company"/>.</returns>
    public static CompanyBuilder NewCompany(string name, FactionCompanyType type, CompanyAvailabilityType availabilityType, Faction faction, ModGuid modGuid) {

        // return new company builder 
        return new CompanyBuilder(new(name, type, modGuid, faction, Array.Empty<UnitBuilder>(), Array.Empty<Ability>(), Array.Empty<CompanyItem>(), false)) {
            AvailabilityType = availabilityType,
        };

    }

}
