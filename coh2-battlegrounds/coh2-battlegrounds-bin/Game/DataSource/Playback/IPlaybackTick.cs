using System;
using System.Collections.Generic;

namespace Battlegrounds.Game.DataSource.Playback;

/// <summary>
/// 
/// </summary>
public interface IPlaybackTick {

    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Timestamp { get; }

    /// <summary>
    /// 
    /// </summary>
    public IList<IPlaybackEvent> Events { get; }

}
