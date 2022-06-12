using System;
using System.Collections.Generic;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Null object representation of an <see cref="ISession"/>.
/// </summary>
public class NullSession : ISession {

    private readonly Dictionary<ulong, Company> m_dummyCompanies = new();

    public Guid SessionID => Guid.Empty;

    public bool AllowPersistency { get; }

    public Scenario Scenario => new Scenario();

    public IGamemode Gamemode => new Wincondition("Unknown Gamemode", ModGuid.BaseGame);

    public ITuningMod TuningMod 
        => ModManager.GetMod<ITuningMod>(ModManager.GetPackage("mod_bg")?.TuningGUID ?? throw new Exception("Invalid tuning GUID set for mod_bg")) 
        ?? throw new Exception("Not tuning mod found for mod_bg");

    public string GamemodeOption => "0";

    public IDictionary<string, object> Settings => throw new NotSupportedException();

    public IList<string> Names => new List<string>();

    public bool HasPlanning => false;

    public NullSession() { this.AllowPersistency = false; }

    public NullSession(bool allowPersistency) { this.AllowPersistency = allowPersistency; }

    public Company? GetPlayerCompany(ulong steamID) => this.m_dummyCompanies.ContainsKey(steamID) ? this.m_dummyCompanies[steamID] : null;

    /// <summary>
    /// Create a company for specified <paramref name="steamID"/> using specified <paramref name="builder"/>.
    /// </summary>
    /// <param name="steamID">The Steam ID of the player.</param>
    /// <param name="builder">The builder function</param>
    public void CreateCompany(ulong steamID, Faction army, string companyName, Func<CompanyBuilder, CompanyBuilder> builder) {
        CompanyBuilder bld = CompanyBuilder.NewCompany(companyName, CompanyType.Unspecified, CompanyAvailabilityType.AnyMode, army, ModGuid.FromGuid(""));
        this.m_dummyCompanies[steamID] = bld.Commit().Result;
    }

    public ISessionParticipant[] GetParticipants() => throw new NotSupportedException();

    public ISessionPlanEntity[] GetPlanEntities() {
        throw new NotSupportedException();
    }

    public ISessionPlanSquad[] GetPlanSquads() {
        throw new NotSupportedException();
    }

    public ISessionPlanGoal[] GetPlanGoals() {
        throw new NotSupportedException();
    }

}
