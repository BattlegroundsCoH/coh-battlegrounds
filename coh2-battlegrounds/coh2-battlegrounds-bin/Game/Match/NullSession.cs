using System;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Null object representation of an <see cref="ISession"/>.
    /// </summary>
    public class NullSession : ISession {

        public Guid SessionID => Guid.Empty;

        public bool AllowPersistency => false;

        public Scenario Scenario => new Scenario();

        public IWinconditionMod Gamemode => new Wincondition("Unknown Gamemode", new Guid());

        public ITuningMod TuningMod => BattlegroundsInstance.BattleGroundsTuningMod;

        public Company GetPlayerCompany(ulong steamID) => null;

    }

}
