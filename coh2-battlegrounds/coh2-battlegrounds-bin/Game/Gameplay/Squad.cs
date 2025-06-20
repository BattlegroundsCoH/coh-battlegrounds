﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Battlegrounds.Game.Gameplay.DataConverters;
using Battlegrounds.Functional;
using Battlegrounds.Verification;
using Battlegrounds.Data.Generators.Lua.RuntimeServices;
using Battlegrounds.Game.Blueprints;

namespace Battlegrounds.Game.Gameplay;

/// <summary>
/// The method in which to deploy a <see cref="Squad"/>.
/// </summary>
public enum DeploymentMethod : byte {

    /// <summary>
    /// No special method is defined (units walks unto the battlefield)
    /// </summary>
    None,

    /// <summary>
    /// The unit is transported unto the battlefield using the support blueprint. The transport will then leave the battlefield.
    /// </summary>
    DeployAndExit,

    /// <summary>
    /// The unit will be transported unto the battlefield using the support blueprint.
    /// </summary>
    DeployAndStay,

    /// <summary>
    /// The unit is dropped from the skies using a parachute.
    /// </summary>
    Paradrop,

    /// <summary>
    /// The unit is deployed using a glider.
    /// </summary>
    Glider,

}

/// <summary>
/// The phase in which a <see cref="Squad"/> can be deployed.
/// </summary>
public enum DeploymentPhase : byte {

    /// <summary>
    /// No phase - <see cref="Squad"/> may not be deployed in any standard way.
    /// </summary>
    PhaseNone,

    /// <summary>
    /// Deployed in the beginning of a match.
    /// </summary>
    PhaseInitial,

    /// <summary>
    /// Deployment is available throughout the match.
    /// </summary>
    PhaseStandard,

}

/// <summary>
/// The role or command level the squad falls under.
/// </summary>
public enum DeploymentRole : byte {

    /// <summary>
    /// The unit is under direct command of the company.
    /// </summary>
    DirectCommand,

    /// <summary>
    /// The unit serves a supporting role of the company.
    /// </summary>
    SupportRole,

    /// <summary>
    /// The unit serves as a reserve unit of the company.
    /// </summary>
    ReserveRole,

}

/// <summary>
/// Representation of a Squad.
/// </summary>
[JsonConverter(typeof(SquadWriter.SquadJson))]
[LuaConverter(typeof(SquadWriter.SquadLua))]
public class Squad : IChecksumPropertyItem {

    private byte m_veterancyRank;
    private float m_veterancyProgress;

    private string m_customName;
    private bool m_isCrewSquad;
    private DeploymentMethod m_deployMode;
    private DeploymentPhase m_deployPhase;
    private DeploymentRole m_deployRole;
    private Squad? m_crewSquad;
    private Blueprint? m_deployBp;
    private EntityBlueprint? m_weaponBlueprint;

    private readonly HashSet<Blueprint> m_upgrades;
    private readonly HashSet<Blueprint> m_slotItems;
    private readonly HashSet<Modifier> m_modifiers;

    private TimeSpan m_combatTime;

    /// <summary>
    /// The unique squad ID used to identify the <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty]
    public ushort SquadID { get; }

    /// <summary>
    /// The player who (currently) owns the <see cref="Squad"/>.
    /// </summary>
    public Player? PlayerOwner { get; }

    /// <summary>
    /// The (crew if squad is a vehicle) <see cref="Blueprints.Blueprint"/> the <see cref="Squad"/> is a type of.
    /// </summary>
    [ChecksumProperty]
    public Blueprint Blueprint { get; }

    /// <summary>
    /// Get the squad or entity <see cref="Blueprints.Blueprint"/> to support this squad.
    /// </summary>
    [ChecksumProperty]
    public Blueprint? SupportBlueprint => this.m_deployBp;

    /// <summary>
    /// Get the sync weapon assigned to the squad.
    /// </summary>
    [ChecksumProperty]
    public EntityBlueprint? SyncWeapon => this.m_weaponBlueprint;

    /// <summary>
    /// The method to use when deploying a <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty]
    public DeploymentMethod DeploymentMethod => this.m_deployMode;

    /// <summary>
    /// The phase in which a squad can be deployed.
    /// </summary>
    [ChecksumProperty]
    public DeploymentPhase DeploymentPhase => this.m_deployPhase;

    /// <summary>
    /// Get the role which the unit is deployed in.
    /// </summary>
    [ChecksumProperty]
    public DeploymentRole DeploymentRole => this.m_deployRole;

    /// <summary>
    /// Get the custom name of the squad. This is null if no name is defined.
    /// </summary>
    [ChecksumProperty]
    public string CustomName => this.m_customName;

    /// <summary>
    /// The squad data for the crew.
    /// </summary>
    [ChecksumProperty]
    public Squad? Crew => this.m_crewSquad;

    /// <summary>
    /// Is the <see cref="Squad"/> the crew for another <see cref="Squad"/> instance.
    /// </summary>
    [ChecksumProperty]
    public bool IsCrew => this.m_isCrewSquad;

    /// <summary>
    /// The <see cref="Blueprint"/> in a <see cref="SquadBlueprint"/> form.
    /// </summary>
    /// <exception cref="InvalidCastException"/>
    public SquadBlueprint SBP => this.Blueprint as SquadBlueprint ?? throw new Exception("Expected squad blueprint but found none!");

    /// <summary>
    /// The achieved veterancy rank of a <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty]
    public byte VeterancyRank => this.m_veterancyRank;

    /// <summary>
    /// The current veterancy progress of a <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty]
    public float VeterancyProgress => this.m_veterancyProgress;

    /// <summary>
    /// The current upgrades applied to a <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableHashSet<Blueprint> Upgrades => this.m_upgrades.ToImmutableHashSet();

    /// <summary>
    /// The current slot items carried by the <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableHashSet<Blueprint> SlotItems => this.m_slotItems.ToImmutableHashSet();

    /// <summary>
    /// Get the current modifiers applied to the <see cref="Squad"/>.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableHashSet<Modifier> Modifiers => this.m_modifiers.ToImmutableHashSet();

    /// <summary>
    /// Get the total amount of time the <see cref="Squad"/> has been in combat.
    /// </summary>
    [ChecksumProperty]
    public TimeSpan CombatTime => this.m_combatTime;

    /// <summary>
    /// Create new <see cref="Squad"/> instance with a unique squad ID, a <see cref="Player"/> owner and a <see cref="Blueprints.Blueprint"/>.
    /// </summary>
    /// <param name="squadID">The unique squad ID used to identify the squad</param>
    /// <param name="owner">The <see cref="Player"/> who owns the squad</param>
    /// <param name="squadBlueprint">The <see cref="Blueprints.Blueprint"/> the squad is an instance of</param>
    public Squad(ushort squadID, Player? owner, Blueprint squadBlueprint) {
        this.SquadID = squadID;
        this.PlayerOwner = owner;
        this.Blueprint = squadBlueprint;
        this.m_slotItems = new HashSet<Blueprint>();
        this.m_upgrades = new HashSet<Blueprint>();
        this.m_modifiers = new HashSet<Modifier>();
        this.m_deployMode = DeploymentMethod.None;
        this.m_deployPhase = DeploymentPhase.PhaseNone;
        this.m_crewSquad = null;
        this.m_customName = string.Empty;
    }

    /// <summary>
    /// Set the veterancy of the <see cref="Squad"/>. The rank and progress is not checked with the blueprint - any veterancy level can be achieved here.
    /// </summary>
    /// <param name="rank">The rank (or level) the squad has achieved.</param>
    /// <param name="progress">The current progress towards the next veterancy level</param>
    public void SetVeterancy(byte rank, float progress = 0.0f) {
        this.m_veterancyRank = rank;
        this.m_veterancyProgress = progress;
    }

    /// <summary>
    /// Increase the veterancy of the <see cref="Squad"/>.  The veterancy rank achievable is max 5.
    /// </summary>
    /// <remarks>
    /// <paramref name="progressValue"/> must be an absolute value and cannot be a difference.
    /// </remarks>
    /// <param name="rankChange">The change in veterancy rank.</param>
    /// <param name="progressValue">The total amount of experience achieved by the <see cref="Squad"/>.</param>
    public void IncreaseVeterancy(byte rankChange, float progressValue = 0.0f) {
        this.m_veterancyRank = (byte)Math.Clamp(this.m_veterancyRank + rankChange, 0, 5);
        if (progressValue != 0.0f) {
            this.m_veterancyProgress = progressValue;
        }
    }

    /// <summary>
    /// Set the deployment method used by a <see cref="Squad"/>.
    /// </summary>
    /// <param name="transportBlueprint">The <see cref="Blueprints.Blueprint"/> to use as transport unit.</param>
    /// <param name="deployMode">The mode used to deploy a <see cref="Squad"/>.</param>
    /// <param name="phase">The deployment phase</param>
    /// <param name="role">The deployment role</param>
    public void SetDeploymentMethod(Blueprint? transportBlueprint, DeploymentMethod deployMode, DeploymentPhase phase, DeploymentRole role) {
        this.m_deployMode = deployMode;
        this.m_deployBp = transportBlueprint;
        this.m_deployPhase = phase;
        this.m_deployRole = role;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="crew"></param>
    public void SetCrew(Squad crew) {
        this.m_crewSquad = crew;
        this.m_crewSquad.SetIsCrew(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isCrew"></param>
    public void SetIsCrew(bool isCrew) 
        => this.m_isCrewSquad = isCrew;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="syncWeapon"></param>
    public void SetSyncWeapon(EntityBlueprint? syncWeapon) 
        => this.m_weaponBlueprint = syncWeapon;

    /// <summary>
    /// Add an upgrade to the squad
    /// </summary>
    /// <param name="upgradeBP">The upgrade blueprint to add</param>
    public void AddUpgrade(Blueprint upgradeBP) => this.m_upgrades.Add(upgradeBP);

    /// <summary>
    /// Add an <see cref="UpgradeBlueprint"/> to the <see cref="Squad"/> if it doesn't already have the upgrade.
    /// </summary>
    /// <param name="upgradeBlueprint">The <see cref="UpgradeBlueprint"/> to add to squad's internal list of upgrades.</param>
    public void AddUpgradeIfNotFound(UpgradeBlueprint upgradeBlueprint)
        => this.m_upgrades.Contains(upgradeBlueprint).IfFalse().Then(() => this.m_upgrades.Add(upgradeBlueprint));

    /// <summary>
    /// Add a slot item to the squad
    /// </summary>
    /// <param name="slotItemBP">The slot item blueprint to add</param>
    public void AddSlotItem(Blueprint slotItemBP) => this.m_slotItems.Add(slotItemBP);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifier"></param>
    public void AddModifier(Modifier modifier) => this.m_modifiers.Add(modifier);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifierName"></param>
    public void RemoveModifier(string modifierName) => this.m_modifiers.RemoveWhere(x => x.Name == modifierName);

    /// <summary>
    /// Increase the amount of combat time the <see cref="Squad"/> has had.
    /// </summary>
    /// <param name="time">The <see cref="TimeSpan"/> to increase combat time by.</param>
    public void IncreaseCombatTime(TimeSpan time)
        => this.m_combatTime += time;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    public void SetCombatTime(TimeSpan time)
        => this.m_combatTime = time;

    /// <summary>
    /// Set the custom name of the squad.
    /// </summary>
    /// <param name="customName">The custom name to set. A null value disable the custom name.</param>
    public void SetName(string customName)
        => this.m_customName = customName;

    /// <summary>
    /// Get the display name of the squad.
    /// </summary>
    /// <returns>If squad has a custom display name, it is returned; Otherwise the <see cref="SquadBlueprint.UI"/> screen name is returned.</returns>
    public string GetName()
        => string.IsNullOrEmpty(this.m_customName) ? this.SBP.UI.ScreenName : this.m_customName;

    /// <summary>
    /// Get the category the squad belongs to. This may change depending on certain parameters.
    /// </summary>
    /// <param name="simplified">Use a simplified (3) category system (team_weapon, vehicle, or infantry).</param>
    /// <returns>The matching category in string format. If no matching category is found, 'infantry' or 'main_infantry' is returned depending on the simplified parameter.</returns>
    public string GetCategory(bool simplified) {

        if (simplified) {

            if (this.SBP.Types.IsAntiTank || this.SBP.Types.IsHeavyArtillery || this.SBP.Types.Contains("mortar") || this.SBP.Types.Contains("hmg")) {
                return "team_weapon";
            } else if (this.SBP.Types.Contains("vehicle")) {
                return "vehicle";
            } else {
                return "infantry";
            }

        } else {
            return "main_infantry";
        }

    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{this.SBP.Name}${this.SquadID}";

}
