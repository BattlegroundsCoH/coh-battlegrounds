using System;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// A session that's being controlled remotely. Implements <see cref="ISession"/>.
    /// </summary>
    public sealed class RemoteSession : ISession {
        
        public Guid SessionID { get; }

        public bool AllowPersistency => true;

        public Scenario Scenario => new Scenario();

        public IWinconditionMod Gamemode => new Wincondition("Unknown Gamemode", new Guid());

        public ITuningMod TuningMod => BattlegroundsInstance.BattleGroundsTuningMod;

        public RemoteSession(string session) => this.SessionID = Guid.Parse(session);

        public Company GetPlayerCompany(ulong steamID) => null;

    }

}
