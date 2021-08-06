using System;
using System.Collections.Generic;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Null object representation of an <see cref="ISession"/>.
    /// </summary>
    public class NullSession : ISession {

        private Dictionary<ulong, Company> m_dummyCompanies = new();

        public Guid SessionID => Guid.Empty;

        public bool AllowPersistency { get; }

        public Scenario Scenario => new Scenario();

        public IGamemode Gamemode => new Wincondition("Unknown Gamemode", new Guid());

        public ITuningMod TuningMod => ModManager.GetMod<ITuningMod>(ModManager.GetPackage("mod_bg").TuningGUID);

        public NullSession() { this.AllowPersistency = false; }

        public NullSession(bool allowPersistency) { this.AllowPersistency = allowPersistency; }

        public Company GetPlayerCompany(ulong steamID) => this.m_dummyCompanies.ContainsKey(steamID) ? this.m_dummyCompanies[steamID] : null;

        /// <summary>
        /// Create a company for specified <paramref name="steamID"/> using specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="steamID">The Steam ID of the player.</param>
        /// <param name="builder">The builder function</param>
        public void CreateCompany(ulong steamID, Faction army, string companyName, Func<CompanyBuilder, CompanyBuilder> builder) {
            CompanyBuilder bld = new();
            this.m_dummyCompanies[steamID] = builder(bld.NewCompany(army).ChangeName(companyName).ChangeTuningMod(this.TuningMod.Guid)).Commit().Result;
        }

    }

}
