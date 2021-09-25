using Battlegrounds.Networking.Remoting.Objects;

namespace Battlegrounds.Networking.LobbySystem.Roles;

/// <summary>
/// Interface representing an object whose true state is defined remotely.
/// </summary>
public interface IRemoteReference {

    /// <summary>
    /// Get the ID of the remote object.
    /// </summary>
    public IObjectID ObjectID { get; }

}
