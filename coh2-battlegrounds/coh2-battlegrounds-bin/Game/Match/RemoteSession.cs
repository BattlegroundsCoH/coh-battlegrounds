using System;
using System.Collections.Generic;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// A session that's being controlled remotely. Implements <see cref="ISession"/>.
    /// </summary>
    public sealed class RemoteSession : ISession {

        public Guid SessionID { get; }

        public bool AllowPersistency => true;

        public Scenario Scenario => new Scenario();

        public IGamemode Gamemode => new Wincondition("Unknown Gamemode", new Guid());

        public string GamemodeOption => "0";

        public ITuningMod TuningMod => ModManager.GetMod<ITuningMod>(ModManager.GetPackage("mod_bg").TuningGUID);

        public IDictionary<string, object> Settings => throw new NotSupportedException();

        public RemoteSession(string session) => this.SessionID = Guid.Parse(session);

        public Company GetPlayerCompany(ulong steamID) => null;

        public ISessionParticipant[] GetParticipants() => throw new NotSupportedException();

    }

}
