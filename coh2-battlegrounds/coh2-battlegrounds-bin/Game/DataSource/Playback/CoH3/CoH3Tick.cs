using System;
using System.Collections.Generic;

namespace Battlegrounds.Game.DataSource.Playback.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3Tick : IPlaybackTick {

    ///<inheritdoc/>
    public TimeSpan Timestamp => throw new NotImplementedException();

    ///<inheritdoc/>
    public IList<IPlaybackEvent> Events => throw new NotImplementedException();

}
