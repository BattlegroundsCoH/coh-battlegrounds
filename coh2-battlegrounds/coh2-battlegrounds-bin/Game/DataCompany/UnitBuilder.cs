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
        DeploymentMethod DeploymentMethod,
        DeploymentPhase DeploymentPhase,
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

    public sealed record NameAction(string Name) : IEditAction<BuildableSquad> {
        private string m_prev = "";
        public BuildableSquad Apply(BuildableSquad target) => target with {
            CustomName = this.Name.And(() => this.m_prev = target.CustomName)
        };
        public BuildableSquad Undo(BuildableSquad target) => target with {
            CustomName = this.m_prev
        };
    }

    public sealed record TransportAction(SquadBlueprint Transport) : IEditAction<BuildableSquad> {
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

    private readonly Stack<IEditAction<BuildableSquad>> m_actions;
    private readonly Stack<IEditAction<BuildableSquad>> m_redoActions;

    private Squad m_result;
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
    public SquadBlueprint Transport => this.m_target.Transport;

    /// <summary>
    /// 
    /// </summary>
    public DeploymentPhase Phase => this.m_target.DeploymentPhase;

    /// <summary>
    /// 
    /// </summary>
    public DeploymentMethod DeployMethod => this.m_target.DeploymentMethod;

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
    public UnitBuilder CrewBuilder => this.m_target.CrewBuilder;

    /// <summary>
    /// Get or set the amount of time this unit has been deployed.
    /// </summary>
    public TimeSpan CombatTime { get; set; }

    /// <summary>
    /// Get the <see cref="Squad"/> instance result from a call to <see cref="Commit(object)"/>.
    /// </summary>
    public Squad Result => this.m_result;

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

    /// <summary>
    /// New basic <see cref="UnitBuilder"/> instance of for building a <see cref="Squad"/>.
    /// </summary>
    [Obsolete("Please use specialised static methods when creating a unit.")]
    public UnitBuilder() {
    }

    /// <summary>
    /// New <see cref="UnitBuilder"/> instance based on the settings of an already built <see cref="Squad"/> instance.
    /// </summary>
    /// <param name="squad">The <see cref="Squad"/> instance to copy the unit data from.</param>
    /// <param name="overrideIndex">Should the built squad keep the index from <paramref name="squad"/>.</param>
    /// <remarks>This will not modify the <see cref="Squad"/> instance.</remarks>
    [Obsolete("Please use specialised static methods when creating a unit.")]
    public UnitBuilder(Squad squad, bool overrideIndex = true) {
        this.m_hasOverrideIndex = overrideIndex;
        this.m_overrideIndex = squad.SquadID;
    }

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
    /// Set the tuning pack GUID this unit should be based on.
    /// </summary>
    /// <param name="guid">The GUID (in coh2 string format).</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    [Obsolete("Should not set mod GUID")]
    public UnitBuilder SetModGUID(ModGuid guid) {
        return this;
    }

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
    /// Set the <see cref="SquadBlueprint"/> the <see cref="Squad"/> instance being built will use.
    /// </summary>
    /// <param name="sbp">The <see cref="SquadBlueprint"/> to set.</param>
    /// <remarks>This must be set before certain other methods.</remarks>
    /// <returns>The modified instance the method is invoked with.</returns>
    [Obsolete("Unit builder should already know blueprint.")]
    public virtual UnitBuilder SetBlueprint(SquadBlueprint sbp) {
        return this;
    }

    /// <summary>
    /// Set the <see cref="SquadBlueprint"/> the <see cref="Squad"/> instance being built will use.
    /// </summary>
    /// <remarks>
    /// This must be called before certain other methods.
    /// </remarks>
    /// <param name="sbpName">The blueprint name to use when finding the <see cref="Blueprint"/>.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    [Obsolete("Unit builder should already know blueprint.")]
    public virtual UnitBuilder SetBlueprint(string sbpName) {
        return this;
    }

    /// <summary>
    /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
    /// </summary>
    /// <param name="sbp">The <see cref="SquadBlueprint"/> to set.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetTransportBlueprint(SquadBlueprint sbp)
        => this.ApplyAction(new TransportAction(sbp));

    /// <summary>
    /// Set the transport <see cref="SquadBlueprint"/> of the <see cref="Squad"/> instance being built will use when entering the battlefield.
    /// </summary>
    /// <remarks>
    /// This must be called before certain other methods.
    /// </remarks>
    /// <param name="sbpName">The blueprint name to use when finding the <see cref="Blueprint"/>.</param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder SetTransportBlueprint(string sbpName) 
        => this.SetTransportBlueprint(BlueprintManager.FromBlueprintName(sbpName, BlueprintType.SBP) as SquadBlueprint ?? throw new ObjectNotFoundException("Blueprint not found"));

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
        => this.AddUpgrade(BlueprintManager.FromBlueprintName(upb, BlueprintType.UBP) as UpgradeBlueprint);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="upbs"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddUpgrade(string[] upbs) {
        upbs.ForEach(x => this.AddUpgrade(BlueprintManager.FromBlueprintName(x, BlueprintType.UBP) as UpgradeBlueprint));
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
        => this.AddSlotItem(BlueprintManager.FromBlueprintName(ibp, BlueprintType.IBP) as SlotItemBlueprint);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ibps"></param>
    /// <returns>The modified instance the method is invoked with.</returns>
    public virtual UnitBuilder AddSlotItem(string[] ibps) {
        ibps.ForEach(x => this.AddSlotItem(BlueprintManager.FromBlueprintName(x, BlueprintType.IBP) as SlotItemBlueprint));
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
    /// Build the <see cref="Squad"/> instance using the data collected with the <see cref="UnitBuilder"/>. 
    /// The ID will be copied from the original <see cref="Squad"/> if possible.
    /// </summary>
    /// <param name="ID">The unique ID to use when creating the <see cref="Squad"/> instance.</param>
    /// <returns>A <see cref="Squad"/> instance with all the parameters defined by the <see cref="UnitBuilder"/>.</returns>
    [Obsolete("Use commit instead")]
    public virtual Squad Build(ushort ID) 
        => this.Commit(ID).Result;

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
    /// Clone self and resets the current instance.
    /// </summary>
    /// <returns>The cloned instance.</returns>
    [Obsolete("Unit builder instances should not be reused.")]
    public UnitBuilder GetAndReset() {
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public CostExtension GetCost()
        => Squad.ComputeFullCost(this.Blueprint.Cost, this.Rank, this.Upgrades, this.Transport, this.DeployMethod, this.Phase, this.Blueprint.Category);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public SquadBlueprint[] GetTransportUnits(CompanyBuilder builder)
        => builder.GetTransports(this.Blueprint.Types.IsHeavyArtillery || this.Blueprint.Types.IsAntiTank);

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
        squad.SetDeploymentMethod(this.Transport, this.DeployMethod, this.Phase);
        squad.SetVeterancy(this.Rank, this.Experience);
        squad.SetCombatTime(this.CombatTime);
        squad.SetIsCrew(this.IsCrew);

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

    public static UnitBuilder NewUnit(string sbp, ModGuid modGuid) {
        var sbp_val = BlueprintManager.GetCollection<SquadBlueprint>()
            .FilterByMod(modGuid).FirstOrDefault(x => x.Name == sbp);
        if (sbp_val is not null) {
            return NewUnit(sbp_val);
        } else {
            throw new ObjectNotFoundException($"Blueprint with name '{sbp}' not found in blueprint database.");
        }
    }

    public static UnitBuilder NewUnit(SquadBlueprint sbp)
        => new(new BuildableSquad(0, 0.0f, false, string.Empty, sbp, null, DeploymentMethod.None, DeploymentPhase.PhaseNone, 
            sbp.HasCrew ? NewCrew(sbp.GetCrewBlueprint()) : null,
            Array.Empty<UpgradeBlueprint>(), Array.Empty<SlotItemBlueprint>(), Array.Empty<Modifier>()));

    public static UnitBuilder NewCrew(SquadBlueprint sbp)
        => new(new BuildableSquad(0, 0.0f, true, string.Empty, sbp, null, DeploymentMethod.None, DeploymentPhase.PhaseNone, null,
            Array.Empty<UpgradeBlueprint>(), Array.Empty<SlotItemBlueprint>(), Array.Empty<Modifier>()));

    public static UnitBuilder EditUnit(Squad squad) {

        // Grab elements
        var upgrades = squad.Upgrades.ToArray().Map(x => x as UpgradeBlueprint).NotNull();
        var items = squad.SlotItems.ToArray().Map(x => x as SlotItemBlueprint).NotNull();

        // Create buildabe
        var buildable = new BuildableSquad(squad.VeterancyRank, squad.VeterancyProgress, squad.IsCrew, squad.CustomName, 
            squad.SBP, squad.SupportBlueprint as SquadBlueprint,
            squad.DeploymentMethod, squad.DeploymentPhase,
            squad.Crew is not null ? EditUnit(squad.Crew) : null,
            upgrades, items, squad.Modifiers.ToArray());

        // Return the buildable
        return new UnitBuilder(buildable) {
            CombatTime = squad.CombatTime
        };

    }

}

