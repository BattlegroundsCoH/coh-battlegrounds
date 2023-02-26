using System;

namespace Battlegrounds.Game.DataSource.Playback;

/// <summary>
/// 
/// </summary>
public interface IPlaybackEvent {
    
    /// <summary>
    /// 
    /// </summary>
    string AttachedMessage { get; }

    /// <summary>
    /// 
    /// </summary>
    byte PlayerID { get; }
    
    /// <summary>
    /// 
    /// </summary>
    TimeSpan Timestamp { get; }
    
    /// <summary>
    /// 
    /// </summary>
    byte Type { get; }
    
    /// <summary>
    /// 
    /// </summary>
    GameEventType EventType { get; }

}
