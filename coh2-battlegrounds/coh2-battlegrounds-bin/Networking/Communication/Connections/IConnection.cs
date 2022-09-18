using System;

using Battlegrounds.Networking.Communication.Golang;

namespace Battlegrounds.Networking.Communication.Connections;

/// <summary>
/// Represents a connection between two endpoints.
/// </summary>
public interface IConnection {
    
    /// <summary>
    /// Get if the connection is connected to endpoint.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Get the identifier identifying the local end of the connection.
    /// </summary>
    ulong SelfId { get; }

    /// <summary>
    /// Send a message along the connection and await no response.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void SendMessage(Message message);

    /// <summary>
    /// Send a message along the connection and await a response.
    /// </summary>
    /// <param name="message">The message to send along the connection.</param>
    /// <param name="time">The timespan to wait for the reply.</param>
    /// <returns>The received content message.</returns>
    ContentMessage? SendAndAwaitReply(Message message, TimeSpan? time);

    /// <summary>
    /// Make the connection shutdown communications.
    /// </summary>
    void Shutdown();

}
