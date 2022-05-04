namespace Battlegrounds.Networking.Communication.Connections;

/// <summary>
/// Represents a connection between two endpoints.
/// </summary>
public interface IConnection {
    
    /// <summary>
    /// Get if the connection is connected to endpoint.
    /// </summary>
    public bool IsConnected { get; }

    /// <summary>
    /// Make the connection shutdown communications.
    /// </summary>
    void Shutdown();

}
