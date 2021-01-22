using System;
using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Null object representation of an <see cref="ISession"/>.
    /// </summary>
    public class NullSession : ISession {

        public Guid SessionID => Guid.Empty;

        public bool AllowPersistency => false;

        public Company GetPlayerCompany(ulong steamID) => null;

    }

}
