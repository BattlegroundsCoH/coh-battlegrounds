using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Online.Services {

    /// <summary>
    /// 
    /// </summary>
    public enum ManagedLobbyPlayerEventType {

        /// <summary>
        /// 
        /// </summary>
        Join,

        /// <summary>
        /// 
        /// </summary>
        Leave,

        /// <summary>
        /// Sent when a player was kicked
        /// </summary>
        Kicked,

        /// <summary>
        /// 
        /// </summary>
        Message,

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="from"></param>
    /// <param name="message"></param>
    public delegate void ManagedLobbyPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message);

    /// <summary>
    /// 
    /// </summary>
    public enum ManagedLobbyLocalEventType {

        /// <summary>
        /// 
        /// </summary>
        HOST,

        /// <summary>
        /// 
        /// </summary>
        KICKED,

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    public delegate void ManagedLobbyLocalEvent(ManagedLobbyLocalEventType type, string message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <param name="lobby"></param>
    public delegate void ManagedLobbyConnectCallback(ManagedLobbyStatus status, ManagedLobby lobby);

}
