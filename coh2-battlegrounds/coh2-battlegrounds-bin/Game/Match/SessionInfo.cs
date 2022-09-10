using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds.AI;
using Battlegrounds.Game.Database;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Info struct used by a <see cref="Session"/> instance to infer information.
/// </summary>
public struct SessionInfo {

    /// <summary>
    /// The selected game mode.
    /// </summary>
    public IGamemode SelectedGamemode { get; set; }

    /// <summary>
    /// The selected tuning mod to play with.
    /// </summary>
    public ITuningMod SelectedTuningMod { get; set; }

    /// <summary>
    /// The selected game mode option.
    /// </summary>
    public int SelectedGamemodeOption { get; set; }

    /// <summary>
    /// Get or set whether the given gamemode option is the direct value and not an index.
    /// </summary>
    public bool IsOptionValue { get; set; }

    /// <summary>
    /// Get or set if the order of the teams is fixed.
    /// </summary>
    public bool IsFixedTeamOrder { get; set; }

    /// <summary>
    /// Get or set if the team order is reversed.
    /// </summary>
    public bool ReverseTeamOrder { get; set; }

    /// <summary>
    /// The selected scenario.
    /// </summary>
    public Scenario SelectedScenario { get; set; }

    /// <summary>
    /// The Allied players participating in the <see cref="Session"/>.
    /// </summary>
    [NotNull]
    public SessionParticipant[] Allies { get; set; }

    /// <summary>
    /// The Axis players participating in the <see cref="Session"/>.
    /// </summary>
    [NotNull]
    public SessionParticipant[] Axis { get; set; }

    /// <summary>
    /// Fill in AI players if there are not enough players on one team.
    /// </summary>
    public bool FillAI { get; set; }

    /// <summary>
    /// The default difficulty to give the AI being filled in.
    /// </summary>
    public AIDifficulty DefaultDifficulty { get; set; }

    /// <summary>
    /// Get or set if the day/night system should be enabled.
    /// </summary>
    public bool EnableDayNightCycle { get; set; }

    /// <summary>
    /// Get or set if the supply system should be enabled.
    /// </summary>
    public bool EnableSupply { get; set; }

    /// <summary>
    /// Get or set the additional options to give to the session
    /// </summary>
    [NotNull]
    public Dictionary<string, int> AdditionalOptions { get; set; }

    /// <summary>
    /// Get or set the array of goals that must be accomplished throughout the session.
    /// </summary>
    [NotNull]
    public SessionPlanGoalInfo[] Goals { get; set; }

    /// <summary>
    /// Get or set the array of entities to spawn at the start of the session.
    /// </summary>
    [NotNull]
    public SessionPlanEntityInfo[] Entities { get; set; }

    /// <summary>
    /// Get or set array of squads to spawn at the start of the session.
    /// </summary>
    [NotNull]
    public SessionPlanSquadInfo[] Squads { get; set; }

}
