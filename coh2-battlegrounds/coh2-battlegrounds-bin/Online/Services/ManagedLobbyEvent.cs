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

        /// <summary>
        /// 
        /// </summary>
        Meta,

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
    public enum ManagedLobbyHostEventType {



    }

    /// <summary>
    /// 
    /// </summary>
    public delegate void ManagedLobbyHostEvent();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="status"></param>
    /// <param name="lobby"></param>
    public delegate void ManagedLobbyConnectCallback(ManagedLobbyStatus status, ManagedLobby lobby);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    public delegate void ManagedLobbyQueryResponse(string arg1, string arg2);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isFileRequest"></param>
    /// <param name="asker"></param>
    /// <param name="requestData"></param>
    /// <param name="identifier"></param>
    public delegate void ManagedLobbyQuery(bool isFileRequest, string asker, string requestData, int identifier);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="file"></param>
    public delegate void ManagedLobbyFileReceived(string from, byte[] file);

}
