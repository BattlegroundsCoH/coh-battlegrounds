using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany.Builder;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

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
        CompanyType Type,
        ModGuid ModGuid,
        Faction Faction,
        UnitBuilder[] Units,
        Ability[] Abilities,
        Blueprint[] Items);

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
        private Ability m_removedAbility; // cache ability incase we need to undo this
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Abilities = target.Abilities.Except(this.m_removedAbility = target.Abilities.First(x => x == Ability))
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Abilities = target.Abilities.Append(this.m_removedAbility)
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

    public sealed record RenameAction(string Name) : IEditAction<BuildableCompany> {
        private string m_prevName;
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Name = this.Name.And(() => this.m_prevName = target.Name)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Name = this.m_prevName
        };
    }

    public sealed record TypeAction(CompanyType Type) : IEditAction<BuildableCompany> {
        private CompanyType m_prevType;
        public BuildableCompany Apply(BuildableCompany target) => target with {
            Type = this.Type.And(() => this.m_prevType = target.Type)
        };
        public BuildableCompany Undo(BuildableCompany target) => target with {
            Type = this.m_prevType
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

    private Company m_companyResult;
    private BuildableCompany m_target;

    private readonly Stack<IEditAction<BuildableCompany>> m_actions;
    private readonly Stack<IEditAction<BuildableCompany>> m_redoActions;

    /// <summary>
    /// Get or set the <see cref="CompanyAvailabilityType"/> of the company.
    /// </summary>
    public CompanyAvailabilityType AvailabilityType { get; set; }

    /// <summary>
    /// Get the current company type.
    /// </summary>
    public CompanyType CompanyType => this.m_target.Type;

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
    public Company Result => this.m_companyResult;

    /// <summary>
    /// 
    /// </summary>
    public bool IsChanged => this.m_actions.Count > 0;

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
    /// New instance of the <see cref="CompanyBuilder"/>.
    /// </summary>
    [Obsolete("Please use specialised static methods when creating a company.")]
    public CompanyBuilder() {
        this.m_companyResult = null;
        this.m_actions = new();
        this.m_redoActions = new();
    }

    private CompanyBuilder(BuildableCompany company) {

        // Set private fields
        this.m_target = company;
        this.m_actions = new();
        this.m_redoActions = new();

        // Set default statistics
        this.Statistics = new();

    }

    /// <summary>
    /// Creates a new <see cref="Company"/> internally that the <see cref="CompanyBuilder"/> will modify while building.
    /// </summary>
    /// <param name="faction">The <see cref="Faction"/> that the company will belong to.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder NewCompany(Faction faction) {
        this.m_companyResult = new Company(faction);
        return this;
    }

    /// <summary>
    /// Clones an existing <see cref="Company"/> instance using a <see cref="CompanyTemplate"/> to clone.
    /// </summary>
    /// <remarks>
    /// Company progression is lost when using this method.
    /// </remarks>
    /// <param name="company">The company to clone.</param>
    /// <param name="newName">The new name of the company.</param>
    /// <param name="companyAvailability">The <see cref="CompanyAvailabilityType"/> new availability type</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    [Obsolete("Please use the static method for cloning a company.")]
    public CompanyBuilder CloneCompany(Company company, string newName, CompanyAvailabilityType companyAvailability) {
        var template = CompanyTemplate.FromCompany(company);
        this.m_companyResult = CompanyTemplate.FromTemplate(template);
        this.AvailabilityType = companyAvailability;
        return this;
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
    [Obsolete("Please use AddUnit")]
    public virtual void AddAndCommitUnit(UnitBuilder builder) {

        // If null, throw error
        if (builder == null) {
            throw new ArgumentNullException(nameof(builder), "The given unit builder may not be null");
        }

        // If null, throw error
        if (this.m_companyResult == null) {
            throw new ArgumentNullException("CompanyTarget", "Cannot add unit to a company that has not been created.");
        }

        // Set the mod GUID (So we get no conflicts between mods).
        builder.SetModGUID(this.m_target.ModGuid);

        // Add squad
        this.m_companyResult.AddSquad(builder);

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
    /// Change the specified <see cref="CompanyType"/> of the <see cref="Company"/>.
    /// </summary>
    /// <param name="type">The new <see cref="CompanyType"/> to set.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder ChangeType(CompanyType type)
        => this.ApplyAction(new TypeAction(type));

    /// <summary>
    /// Change the name of the <see cref="Company"/>.
    /// </summary>
    /// <param name="name">The new name to set.</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder ChangeName(string name)
        => this.ApplyAction(new RenameAction(name));

    /// <summary>
    /// Change the user of the company (Possibly obsolete - you may ignore it).
    /// </summary>
    /// <param name="name"></param>
    [Obsolete("Please remove any call to this method")]
    public virtual CompanyBuilder ChangeUser(string name) {
        return this;
    }

    /// <summary>
    /// Change the associated <see cref="Guid"/> of the <see cref="Company"/>. (This will decide from where the blueprints can be drawn from).
    /// </summary>
    /// <param name="tuningGUID">The string version of the mod GUID. (May contain '-' characters).</param>
    /// <returns>The calling <see cref="CompanyBuilder"/> instance.</returns>
    public virtual CompanyBuilder ChangeTuningMod(string tuningGUID)
        => this.ChangeTuningMod(ModGuid.FromGuid(tuningGUID));

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
    public void AddEquipment(Blueprint blueprint) => throw new NotImplementedException();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="equipment"></param>
    /// <returns></returns>
    public CompanyBuilder RemoveEquipment(Blueprint equipment) => throw new NotImplementedException();

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
        => this.m_companyResult.Units.Count(x => x.DeploymentPhase == phase);

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
    [MemberNotNull(nameof(Result), nameof(m_companyResult))]
    public virtual IBuilder<Company> Commit(object arg = null) {

        // Update company fluff
        var company = new Company(this.m_target.Faction);
        company.SetType(this.m_target.Type);
        company.SetAvailability(this.AvailabilityType);
        company.SetAppVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        company.Name = this.m_target.Name;
        company.TuningGUID = this.m_target.ModGuid;
        company.UpdateStatistics(_ => this.Statistics);

        // Loop over units and add to company
        foreach (var unit in this.m_target.Units)
            company.AddSquad(unit);

        // Loop over abilities and add to company
        foreach (var ability in this.m_target.Abilities)
            company.AddAbility(ability);

        // Set as result
        this.m_companyResult = company;

        // Return self.
        return this;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual bool HasAbility(string blueprint) => this.m_companyResult.Abilities.Any(x => x.ABP.Name == blueprint);

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
        var package = ModManager.GetPackageFromGuid(this.m_target.ModGuid);
        _ = Company.GetSpecialUnitAbilities(this.m_target.Faction, package, this.m_target.Units.Map(x => x.Blueprint).Distinct()).ForEach(x => action(x, true));
        _ = this.m_target.Abilities.ForEach(x => action(x, false));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public virtual void EachItem(Action<Blueprint> action) => this.m_target.Items.ForEach(action);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="company"></param>
    /// <returns></returns>
    public static CompanyBuilder EditCompany(Company company) {

        // Grab unit list as unit builders
        var units = company.Units.Map(UnitBuilder.EditUnit);

        // Create buildable variant
        var buildable = new BuildableCompany(company.Name, company.Type, company.TuningGUID, company.Army, units, company.Abilities.ToArray(), company.Inventory.ToArray());

        // Return company buuilder instance
        return new CompanyBuilder(buildable) {
            AvailabilityType = company.AvailabilityType,
            Statistics = company.Statistics
        };

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="availabilityType"></param>
    /// <param name="faction"></param>
    /// <param name="modGuid"></param>
    /// <returns></returns>
    public static CompanyBuilder NewCompany(string name, CompanyType type, CompanyAvailabilityType availabilityType, Faction faction, ModGuid modGuid) {

        // return new company builder 
        return new CompanyBuilder(new(name, type, modGuid, faction, Array.Empty<UnitBuilder>(), Array.Empty<Ability>(), Array.Empty<Blueprint>())) {
            AvailabilityType = availabilityType,
        };

    }

}
