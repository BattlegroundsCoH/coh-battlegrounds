using System;

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
    
    /// <summary>
    /// 
    /// </summary>
    bool IsPartial { get; }
    
    /// <summary>
    /// 
    /// </summary>
    Player[] Players { get; }
    
    /// <summary>
    /// 
    /// </summary>
    IPlaybackTick[] Ticks { get; }

    /// <summary>
    /// 
    /// </summary>
    TimeSpan Length { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool LoadPlayback();

}
