using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany.Builder;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Builder class for building a <see cref="Squad"/> instance with serial-style methods.
/// </summary>
public class UnitBuilder : IBuilder<Squad> {

    public record BuildableSquad(
        byte Rank,
        float Experience,
        bool IsCrew,
        string CustomName,
        SquadBlueprint Blueprint,
        SquadBlueprint? Transport,
        EntityBlueprint? SyncWeapon,
        DeploymentMethod DeploymentMethod,
        DeploymentPhase DeploymentPhase,
        DeploymentRole DeploymentRole,
        UnitBuilder? CrewBuilder,
        UpgradeBlueprint[] Upgrades,
        SlotItemBlueprint[] Items,
        Modifier[] Modifiers);

    public sealed record RankAction(byte Rank) : IEditAction<BuildableSquad> {
        private byte m_prevRank;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Rank = this.Rank.And(() => this.m_prevRank = target.Rank)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {                
            Rank = this.m_prevRank
        };
    }

    public sealed record ExperienceAction(float Experience) : IEditAction<BuildableSquad> {
        private float m_prevExperience;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Experience = this.Experience.And(() => this.m_prevExperience = target.Rank)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Experience = this.m_prevExperience
        };
    }

    public sealed record DeploymentAction(DeploymentMethod Method) : IEditAction<BuildableSquad> {
        private DeploymentMethod m_prevMethod;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            DeploymentMethod = this.Method.And(() => this.m_prevMethod = target.DeploymentMethod)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            DeploymentMethod = this.m_prevMethod
        };
    }

    public sealed record PhaseAction(DeploymentPhase Phase) : IEditAction<BuildableSquad> {
        private DeploymentPhase m_prevPhase;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            DeploymentPhase = this.Phase.And(() => this.m_prevPhase = target.DeploymentPhase)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            DeploymentPhase = this.m_prevPhase
        };
    }

    public sealed record RoleAction(DeploymentRole Role) : IEditAction<BuildableSquad> {
        private DeploymentRole m_prevRole;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            DeploymentRole = this.Role.And(() => this.m_prevRole = target.DeploymentRole)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            DeploymentRole = this.m_prevRole
        };
    }

    public sealed record NameAction(string Name) : IEditAction<BuildableSquad> {
        private string m_prev = "";
        public BuildableSquad Apply(BuildableSquad target) => target with {
            CustomName = this.Name.And(() => this.m_prev = target.CustomName)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            CustomName = this.m_prev
        };
    }

    public sealed record TransportAction(SquadBlueprint? Transport) : IEditAction<BuildableSquad> {
        private SquadBlueprint? m_prev;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Transport = this.Transport.And(() => this.m_prev = target.Transport)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Transport = this.m_prev
        };
    }

    public sealed record AddUpgradeAction(UpgradeBlueprint Blueprint) : IEditAction<BuildableSquad> {
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Upgrades = target.Upgrades.Append(this.Blueprint)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Upgrades = target.Upgrades.Except(this.Blueprint)
        };
    }

    public sealed record AddItemAction(SlotItemBlueprint Blueprint) : IEditAction<BuildableSquad> {
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Items = target.Items.Append(this.Blueprint)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Items = target.Items.Except(this.Blueprint)
        };
    }

    public sealed record AddSyncWeaponAction(EntityBlueprint Blueprint) : IEditAction<BuildableSquad> {
        private EntityBlueprint? m_prev;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            SyncWeapon = this.Blueprint.And(() => this.m_prev = target.SyncWeapon)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            SyncWeapon = this.m_prev
        };
    }

    public sealed record AddModifierAction(Modifier Modifier) : IEditAction<BuildableSquad> {
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Modifiers = target.Modifiers.Append(this.Modifier)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Modifiers = target.Modifiers.Except(this.Modifier)
        };
    }

    public sealed record RemoveUpgradeAction(UpgradeBlueprint Blueprint) : IEditAction<BuildableSquad> {
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Upgrades = target.Upgrades.Except(this.Blueprint)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Upgrades = target.Upgrades.Append(this.Blueprint)
        };
    }

    public sealed record RemoveItemAction(SlotItemBlueprint Blueprint) : IEditAction<BuildableSquad> {
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Items = target.Items.Except(this.Blueprint)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Items = target.Items.Append(this.Blueprint)
        };
    }

    public sealed record RemoveModifierAction(Modifier Modifier) : IEditAction<BuildableSquad> {
        public BuildableSquad Apply(BuildableSquad target) => target with {
            Modifiers = target.Modifiers.Except(this.Modifier)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            Modifiers = target.Modifiers.Append(this.Modifier)
        };
    }

    public sealed record RemoveSyncWeaponAction() : IEditAction<BuildableSquad> {
        private EntityBlueprint? m_prev;
        public BuildableSquad Apply(BuildableSquad target) => target with {
            SyncWeapon = (this.m_prev = target.SyncWeapon).Then(_ => null)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            SyncWeapon = this.m_prev
        };
    }

    private readonly Stack<IEditAction<BuildableSquad>> m_actions;
    private readonly Stack<IEditAction<BuildableSquad>> m_redoActions;

    private Squad? m_result;
    private BuildableSquad m_target;

    private readonly ushort m_overrideIndex = ushort.MaxValue;
    private readonly bool m_hasOverrideIndex = false;

    /// <summary>
    /// Get if builder already has an index to use
    /// </summary>
    public bool HasIndex => this.m_hasOverrideIndex;

    /// <summary>
    /// Get the current blueprint of the unit.
    /// </summary>
    public SquadBlueprint Blueprint => this.m_target.Blueprint;

    /// <summary>
    /// Get the override index of the unit (0 if none)
    /// </summary>
    public ushort OverrideIndex => this.m_overrideIndex;

    /// <summary>
    /// 
    /// </summary>
    public bool IsCrew => this.m_target.IsCrew;

    /// <summary>
    /// 
    /// </summary>
    public byte Rank => this.m_target.Rank;

    /// <summary>
    /// 
    /// </summary>
    public float Experience => this.m_target.Experience;

    /// <summary>
    /// 
    /// </summary>
    public SquadBlueprint? Transport => this.m_target.Transport;

    /// <summary>
    /// Get the deployment phase of the unit.
    /// </summary>
    public DeploymentPhase Phase => this.m_target.DeploymentPhase;

    /// <summary>
    /// Get the deployment method of the unit.
    /// </summary>
    public DeploymentMethod DeployMethod => this.m_target.DeploymentMethod;

    /// <summary>
    /// Get the deployment role of the unit.
    /// </summary>
    public DeploymentRole Role => this.m_target.DeploymentRole;

    /// <summary>
    /// 
    /// </summary>
    public UpgradeBlueprint[] Upgrades => this.m_target.Upgrades;

    /// <summary>
    /// 
    /// </summary>
    public SlotItemBlueprint[] Items => this.m_target.Items;

    /// <summary>
    /// 
    /// </summary>
    public AbilityBlueprint[] Abilities => this.m_target.Blueprint.Abilities.Map(x => BlueprintManager.FromBlueprintName<AbilityBlueprint>(x));

    /// <summary>
    /// 
    /// </summary>
    public Modifier[] Mods => this.m_target.Modifiers;

    /// <summary>
    /// 
    /// </summary>
    public UnitBuilder? CrewBuilder => this.m_target.CrewBuilder;

    /// <summary>
    /// Get or set the amount of time this unit has been deployed.
    /// </summary>
    public TimeSpan CombatTime { get; set; }

    /// <summary>
    /// Get the <see cref="Squad"/> instance result from a call to <see cref="Commit(object)"/>.
    /// </summary>
    public Squad Result => this.m_result ?? throw new InvalidOperationException("No result available - commit must be invoked beforehand!");

    /// <summary>
    /// Get if any changes have been made
    /// </summary>
    public bool IsChanged => this.m_actions.Count > 0;

    /// <summary>
    /// Get if anything can be undone
    /// </summary>
    public bool CanUndo => this.m_actions.Count > 0;

    /// <summary>
    /// Get if anything can be redone
    /// </summary>
    public bool CanRedo => this.m_redoActions.Count > 0;

    private UnitBuilder(BuildableSquad squad) {

        // Set initial squad
        this.m_target = squad;

        // Init action queues
        this.m_actions = new();
        this.m_redoActions = new();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetName() => this.m_target.CustomName switch {
        "" => this.m_target.Blueprint.UI.ScreenName,
        _ => this.m_target.CustomName
    };

    /// <summary>
    /// Set the veterancy rank of the <see cref="Squad"/> instance being built.
    /// </summary>
    /// <param name="level">The veterancy rank in byte-range to set.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetVeterancyRank(byte level)
        => this.ApplyAction(new RankAction(level));

    /// <summary>
    /// Set the veterancy progress of the <see cref="Squad"/> instance being built.
    /// </summary>
    /// <param name="experience">The veterancy progress to set.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetVeterancyExperience(float experience)
        => this.ApplyAction(new ExperienceAction(experience));

    /// <summary>
    /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
    /// </summary>
    /// <param name="sbp">The <see cref="SquadBlueprint"/> to set.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetTransportBlueprint(SquadBlueprint? sbp)
        => this.ApplyAction(new TransportAction(sbp));

    /// <summary>
    /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
    /// </summary>
    /// <remarks>
    /// This must be called before certain other methods.
    /// </remarks>
    /// <param name="sbpName">The blueprint name to use when finding the <see cref="Blueprint"/>. If null or the empty string, the transport blueprint is set to 'none'</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetTransportBlueprint(string? sbpName) {
        if (string.IsNullOrEmpty(sbpName)) {
            this.ApplyAction(new TransportAction(null));
        } else {
            this.SetTransportBlueprint(BlueprintManager.FromBlueprintName(sbpName, BlueprintType.SBP) as SquadBlueprint ?? throw new ObjectNotFoundException("Blueprint not found"));
        }
        return this;
    }

    /// <summary>
    /// Set the <see cref="DeploymentMethod"/> to use when the <see cref="Squad"/> instance being built is deployed.
    /// </summary>
    /// <param name="method">The <see cref="DeploymentMethod"/> to use when deploying.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetDeploymentMethod(DeploymentMethod method)
        => this.ApplyAction(new DeploymentAction(method));

    /// <summary>
    /// Set the <see cref="DeploymentPhase"/> the <see cref="Squad"/> instance being built may be deployed in.
    /// </summary>
    /// <param name="phase">The <see cref="DeploymentPhase"/> to set.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetDeploymentPhase(DeploymentPhase phase)
        => this.ApplyAction(new PhaseAction(phase));

    /// <summary>
    /// Set the <see cref="DeploymentRole"/> of the <see cref="Squad"/> instance; dictacting the cost and deployment delay of units.
    /// </summary>
    /// <param name="role">The <see cref="DeploymentRole"/> to set.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetDeploymentRole(DeploymentRole role)
        => this.ApplyAction(new RoleAction(role));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="upb"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddUpgrade(UpgradeBlueprint upb) 
        => this.ApplyAction(new AddUpgradeAction(upb));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="upbs"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddUpgrade(UpgradeBlueprint[] upbs) {
        upbs.ForEach(x => this.AddUpgrade(x));
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="upb"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddUpgrade(string upb)
        => this.AddUpgrade(BlueprintManager.FromBlueprintName(upb, BlueprintType.UBP) as UpgradeBlueprint ?? throw new ObjectNotFoundException("Blueprint not found"));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="upbs"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddUpgrade(string[] upbs) {
        upbs.ForEach(x => this.AddUpgrade(BlueprintManager.FromBlueprintName(x, BlueprintType.UBP) as UpgradeBlueprint ?? throw new ObjectNotFoundException("Blueprint not found")));
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ibp"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddSlotItem(SlotItemBlueprint ibp) 
        => this.ApplyAction(new AddItemAction(ibp));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ibp"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddSlotItem(string ibp)
        => this.AddSlotItem(BlueprintManager.FromBlueprintName(ibp, BlueprintType.IBP) as SlotItemBlueprint ?? throw new ObjectNotFoundException("Blueprint not found"));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ibps"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddSlotItem(string[] ibps) {
        ibps.ForEach(x => this.AddSlotItem(BlueprintManager.FromBlueprintName(x, BlueprintType.IBP) as SlotItemBlueprint ?? throw new ObjectNotFoundException("Blueprint not found")));
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifier"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddModifier(Modifier modifier)
        => this.ApplyAction(new AddModifierAction(modifier));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ubp"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder RemoveUpgrade(UpgradeBlueprint ubp) => this.ApplyAction(new RemoveUpgradeAction(ubp));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ibp"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder RemoveSlotItem(SlotItemBlueprint ibp) => this.ApplyAction(new RemoveItemAction(ibp));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifier"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder RemoveModifier(Modifier modifier) => this.ApplyAction(new RemoveModifierAction(modifier));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customName"></param>
    public virtual UnitBuilder SetCustomName(string customName)
        => this.ApplyAction(new NameAction(customName));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="squad"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual UnitBuilder SetCrew(Squad squad) {
        if (this.Blueprint is null) {
            throw new InvalidOperationException("Attempt to create a crew for a unit without a blueprint.");
        }
        if (!this.Blueprint.HasCrew) {
            throw new InvalidOperationException("Attempt to create a crew for a unit that does not support crews.");
        }
        this.m_target = this.m_target with {
            CrewBuilder = EditUnit(squad)
        };
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    public virtual UnitBuilder SetCombatTime(TimeSpan span) {
        this.CombatTime = span;
        return this;
    }

    /// <summary>
    /// Get available transport units for the unit.
    /// </summary>
    /// <param name="companyType">The faction type that dictates available transports.</param>
    /// <returns>An array of valid transports.</returns>
    public SquadBlueprint[] GetTransportUnits(FactionCompanyType companyType) {

        // Get transports
        var transports = companyType.DeployBlueprints.ToArray()
            .Filter(x => x.Units.Length is 0 || (!x.Units.Any(y => y == this.Blueprint.Name)))
            .Filter(x => x.SupportsRole(this.Role)); ;

        // Filter to tow only
        if (this.Blueprint.Types.IsHeavyArtillery || this.Blueprint.Types.IsAntiTank)
            transports = transports.Filter(x => x.Tow);

        // Map to proper transports
        return transports.Map(x => BlueprintManager.FromBlueprintName<SquadBlueprint>(x.Blueprint));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityBlueprint"></param>
    /// <returns></returns>
    public virtual UnitBuilder SetSyncWeapon(EntityBlueprint entityBlueprint)
        => this.ApplyAction(new AddSyncWeaponAction(entityBlueprint));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IBuilder<Squad> Commit(object? arg) {

        // Get the ID to use
        ushort id = arg switch {
            ushort u => this.m_hasOverrideIndex ? this.m_overrideIndex : u,
            _ => this.m_hasOverrideIndex ? this.m_overrideIndex : throw new ArgumentException("Expected squad index on commit action but no valid value given.", nameof(arg))
        };

        // Create actual squad
        Squad squad = new Squad(id, null, this.Blueprint);
        squad.SetName(this.m_target.CustomName);
        squad.SetDeploymentMethod(this.Transport, this.DeployMethod, this.Phase, this.Role);
        squad.SetVeterancy(this.Rank, this.Experience);
        squad.SetCombatTime(this.CombatTime);
        squad.SetIsCrew(this.IsCrew);
        squad.SetSyncWeapon(this.m_target.SyncWeapon);

        // Handle crew
        if (this.m_target.CrewBuilder is not null) {
            squad.SetCrew(this.m_target.CrewBuilder.Commit((ushort)(id + 1)).Result);
        }

        // Loop over upgrades
        foreach (var upg in this.Upgrades)
            squad.AddUpgradeIfNotFound(upg);

        // Loop over items
        foreach (var item in this.Items)
            squad.AddSlotItem(item);

        // Loop over items
        foreach (var mod in this.Mods)
            squad.AddModifier(mod);

        // Set result
        this.m_result = squad;

        // Return self
        return this;

    }

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

    private UnitBuilder ApplyAction(IEditAction<BuildableSquad> editAction) {

        // Add action to list of actions performed
        this.m_actions.Push(editAction);

        // Apply acction
        this.m_target = editAction.Apply(this.m_target);

        // Return self
        return this;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sbp"></param>
    /// <param name="modGuid"></param>
    /// <returns></returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    public static UnitBuilder NewUnit(string sbp, ModGuid modGuid) {
        var sbp_val = BlueprintManager.GetCollection<SquadBlueprint>()
            .FilterByMod(modGuid).FirstOrDefault(x => x.Name == sbp);
        if (sbp_val is not null) {
            return NewUnit(sbp_val);
        } else {
            throw new ObjectNotFoundException($"Blueprint with name '{sbp}' not found in blueprint database.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sbp"></param>
    /// <returns></returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    public static UnitBuilder NewUnit(SquadBlueprint sbp)
        => new(new BuildableSquad(0, 0.0f, false, string.Empty, sbp, null, null, DeploymentMethod.None, DeploymentPhase.PhaseNone, DeploymentRole.ReserveRole,
            sbp.HasCrew ? NewCrew(sbp.GetCrewBlueprint() ?? throw new ObjectNotFoundException($"Crew blueprint not found for blueprint '{sbp}'.")) : null,
            Array.Empty<UpgradeBlueprint>(), Array.Empty<SlotItemBlueprint>(), Array.Empty<Modifier>()));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sbp"></param>
    /// <returns></returns>
    public static UnitBuilder NewCrew(SquadBlueprint sbp)
        => new(new BuildableSquad(0, 0.0f, true, string.Empty, sbp, null, null, DeploymentMethod.None, DeploymentPhase.PhaseNone, DeploymentRole.ReserveRole, null,
            Array.Empty<UpgradeBlueprint>(), Array.Empty<SlotItemBlueprint>(), Array.Empty<Modifier>()));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="squad"></param>
    /// <returns></returns>
    public static UnitBuilder EditUnit(Squad squad) {

        // Grab elements
        var upgrades = squad.Upgrades.ToArray().Map(x => x as UpgradeBlueprint).NotNull();
        var items = squad.SlotItems.ToArray().Map(x => x as SlotItemBlueprint).NotNull();

        // Create buildabe
        var buildable = new BuildableSquad(squad.VeterancyRank, squad.VeterancyProgress, squad.IsCrew, squad.CustomName, 
            squad.SBP, squad.SupportBlueprint as SquadBlueprint,
            squad.SyncWeapon,
            squad.DeploymentMethod, squad.DeploymentPhase, squad.DeploymentRole,
            squad.Crew is not null ? EditUnit(squad.Crew) : null,
            upgrades, items, squad.Modifiers.ToArray());

        // Return the buildable
        return new UnitBuilder(buildable) {
            CombatTime = squad.CombatTime
        };

    }

    /// <summary>
    /// Checks if the given <see cref="UnitBuilder"/> instance allows for custom naming.
    /// </summary>
    /// <param name="builder">The builder instance to check.</param>
    /// <returns>If custom name is allowed, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public static bool AllowCustomName(UnitBuilder builder) {

        // Bail if not vet
        if (builder.Rank < 3)
            return false;

        // Grab typelist
        var ts = builder.Blueprint.Types;

        // Return ok if of type
        return ts.IsSniper || ts.IsSpecialInfantry || ts.IsVehicle;

    }

}
