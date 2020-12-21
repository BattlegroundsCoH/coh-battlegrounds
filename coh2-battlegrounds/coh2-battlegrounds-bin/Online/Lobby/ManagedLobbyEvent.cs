namespace Battlegrounds.Online.Lobby {

    /// <summary>
    /// Callback event for handling a connection request to a <see cref="ManagedLobby"/>.
    /// </summary>
    /// <param name="status">The status report on the <see cref="ManagedLobby"/> connection request.</param>
    /// <param name="lobby">The managed lobby instance that was created. This may be null depending on the query result.</param>
    public delegate void ManagedLobbyConnectCallback(ManagedLobbyStatus status, ManagedLobby lobby);

    /// <summary>
    /// Event type triggered by a player event in a <see cref="ManagedLobby"/>.
    /// </summary>
    public enum ManagedLobbyPlayerEventType {

        /// <summary>
        /// A player has joined the lobby.
        /// </summary>
        Join,

        /// <summary>
        /// A player has voluntarily left the lobby (or unexpectedly lost connection to the server).
        /// </summary>
        Leave,

        /// <summary>
        /// P player was kicked from the server by the host.
        /// </summary>
        Kicked,

        /// <summary>
        /// A message was sent from a player.
        /// </summary>
        Message,

        /// <summary>
        /// A meta-message (small data) was sent from a player.
        /// </summary>
        Meta,

    }

    /// <summary>
    /// Event delegate for handling a <see cref="ManagedLobby"/> player event.
    /// </summary>
    /// <param name="type">The type of event that triggered the event.</param>
    /// <param name="from">The player who triggered the event.</param>
    /// <param name="message">The message attached to the event.</param>
    public delegate void ManagedLobbyPlayerEvent(ManagedLobbyPlayerEventType type, string from, string message);

    /// <summary>
    /// Event type triggered when a <see cref="ManagedLobby"/> receives a local-oriented message.
    /// </summary>
    public enum ManagedLobbyLocalEventType {

        /// <summary>
        /// The local client was upgrades to host.
        /// </summary>
        Host,

        /// <summary>
        /// The local client was kicked from the lobby.
        /// </summary>
        Kicked,

    }

    /// <summary>
    /// Event delegate for handling a <see cref="ManagedLobby"/> local-targeted event.
    /// </summary>
    /// <param name="type">The type of event that triggered the event.</param>
    /// <param name="message">The message attached to the local event.</param>
    public delegate void ManagedLobbyLocalEvent(ManagedLobbyLocalEventType type, string message);

    /// <summary>
    /// Event delegate for handling lobby information updates.
    /// </summary>
    /// <param name="info">The information that was updated.</param>
    /// <param name="value">The new value of the information.</param>
    public delegate void ManagedLobbyInfoChanged(string info, string value);

    /// <summary>
    /// Event delegate for a simple response to a <see cref="ManagedLobby"/> query.
    /// </summary>
    /// <param name="arg1">The first argument of the response.</param>
    /// <param name="arg2">The second argument of the response. May be empty.</param>
    public delegate void ManagedLobbyQueryResponse(string arg1, string arg2);

    /// <summary>
    /// Event delegate for queries sent to the local client by a <see cref="ManagedLobby"/> instance.
    /// </summary>
    /// <param name="isFileRequest">Boolean flag marking whether it's a file request or an information request.</param>
    /// <param name="asker">The username of the user who asked for this information.</param>
    /// <param name="requestData">The specific data that was requested.</param>
    /// <param name="identifier">The unique response identifier used for this query. To respond to the query, use this identifier when responding.</param>
    public delegate void ManagedLobbyQuery(bool isFileRequest, string asker, string requestData, int identifier);

    /// <summary>
    /// Event delegate triggered when a file was received (or failed to be received).
    /// </summary>
    /// <param name="sender">The username of the user who sent the file.</param>
    /// <param name="filename">The name of the file that was sent.</param>
    /// <param name="received">Boolean flag describing the 'received' state of the file.</param>
    /// <param name="file">The full byte-file-content that was received. May by null if no file was successfully received.</param>
    /// <param name="identifier">The identifier used to send the file.</param>
    public delegate void ManagedLobbyFileReceived(string sender, string filename, bool received, byte[] file, int identifier);

    /// <summary>
    /// Event delegate triggered when the host has sent the start match message.
    /// </summary>
    public delegate void ManagedLobbyMatchStart();

    /// <summary>
    /// Event delegate triggered when local data was requested internally by a <see cref="ManagedLobby"/> process.
    /// </summary>
    /// <param name="arg">The string name identifier for the type of data that was requested.</param>
    /// <returns>Any <see cref="System.Object"/> that is a pointer-or-direct value of the requested data.</returns>
    public delegate object ManagedLobbyLocalDataRequest(string arg);

    /// <summary>
    /// Event delegate triggered when the lobby task is done.
    /// </summary>
    /// <param name="self">A reference to the <see cref="ManagedLobby"/> instance that finished executing.</param>
    public delegate void ManagedLobbyTaskDone(ManagedLobby self);

}
