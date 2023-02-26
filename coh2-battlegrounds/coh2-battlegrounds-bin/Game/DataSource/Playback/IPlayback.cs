using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.DataSource.Playback;

/// <summary>
/// Enum flag marking match type
/// </summary>
public enum MatchType {

    /// <summary>
    /// PVP
    /// </summary>
    Multiplayer = 1,

    /// <summary>
    /// Skirmish (vs AI)
    /// </summary>
    Skirmish = 2,

}

/// <summary>
/// 
/// </summary>
public interface IPlayback {
    
    bool IsPartial { get; }
    
    Player[] Players { get; }
    
    IPlaybackTick[] Ticks { get; }

    TimeSpan Length { get; }

    bool LoadPlayback();

}
