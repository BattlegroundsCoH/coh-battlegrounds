using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data;

public class NoMatchData : IMatchData {

    public ISession Session => new NullSession();

    public TimeSpan Length => TimeSpan.Zero;

    public bool IsSessionMatch => false;

    public ReadOnlyCollection<Player> Players => new(Array.Empty<Player>());

    public IEnumerator<IMatchEvent> GetEnumerator() => (IEnumerator<IMatchEvent>)Array.Empty<IMatchEvent>().GetEnumerator();

    public bool LoadMatchData(string matchFile) => false;

    public bool ParseMatchData() => false;

    IEnumerator IEnumerable.GetEnumerator() => this.Players.GetEnumerator();

}
