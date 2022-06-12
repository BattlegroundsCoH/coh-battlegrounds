using System;
using System.Collections.Generic;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Interface for session data for a session to be played.
/// </summary>
public interface ISession {

    /// <summary>
    /// Get the ID given to the session.
    /// </summary>
    Guid SessionID { get; }

    /// <summary>
    /// Get a value determining if the <see cref="ISession"/> allow for persistency (events ingame will be saved in the company).
    /// </summary>
    bool AllowPersistency { get; }

    /// <summary>
    /// 
    /// </summary>
    bool HasPlanning { get; }

    /// <summary>
    /// Get the scenario to be played.
    /// </summary>
    Scenario Scenario { get; }

    /// <summary>
    /// Get the wincondition to be played.
    /// </summary>
    IGamemode Gamemode { get; }

    /// <summary>
    /// Get the gamemode option to be played.
    /// </summary>
    string GamemodeOption{ get; }

    /// <summary>
    /// Get the GUID or name of the tuning mod to be played with.
    /// </summary>
    ITuningMod TuningMod { get; }

    /// <summary>
    /// Get the settings data
    /// </summary>
    IDictionary<string, object> Settings { get; }

    /// <summary>
    /// Get the list of custom names that are defined for this session
    /// </summary>
    IList<string> Names { get; }

    /// <summary>
    /// Get all participants in the session.
    /// </summary>
    /// <returns>Array of participants in the session.</returns>
    ISessionParticipant[] GetParticipants();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ISessionPlanEntity[] GetPlanEntities();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ISessionPlanSquad[] GetPlanSquads();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ISessionPlanGoal[] GetPlanGoals();

    /// <summary>
    /// Get the <see cref="Company"/> for the player with the given <paramref name="steamID"/>.
    /// </summary>
    /// <param name="steamID">The unique steam ID of the player to get the <see cref="Company"/> of.</param>
    /// <returns>The <see cref="Company"/> belonging to the player with given <paramref name="steamID"/>. Otherwise <see langword="null"/>.</returns>
    Company? GetPlayerCompany(ulong steamID);

}
