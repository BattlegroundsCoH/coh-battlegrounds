using System;

using Battlegrounds.Lobby.Components;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util.Threading;

namespace Battlegrounds.Lobby.Playing;

/// <summary>
/// Factory for constructing a <see cref="IPlayModel"/> instance based on <see cref="ILobbyHandle"/> state.
/// </summary>
public static class PlayModelFactory {

    /// <summary>
    /// Get a <see cref="IPlayModel"/> for the <paramref name="handle"/> state.
    /// </summary>
    /// <param name="handle">The handle to retrieve basic information from.</param>
    /// <param name="lobbyChat">The chat instance used by the <see cref="IPlayModel"/>.</param>
    /// <param name="dispatcher">The dispatcher to use when interacting with the UI.</param>
    /// <param name="callbackHandler">Handler for reporting on upload progress.</param>
    /// <param name="cancelTime">The amount of seconds participants will have to cancel.</param>
    /// <returns>A <see cref="IPlayModel"/> instance</returns>
    /// <exception cref="Exception"></exception>
    public static IPlayModel GetModel(ILobbyHandle handle, IChatSpectator lobbyChat, IDispatcher dispatcher, uint cancelTime, UploadProgressCallbackHandler? callbackHandler) {

        // If local then return early with a singleplayer
        if (handle is LocalLobbyHandle) {
            return new SingleplayerModel(handle, lobbyChat, dispatcher);
        }

        // If only one human -> single model
        uint humans = handle.GetPlayerCount(humansOnly: true);
        if (humans == 1) {
            return new SingleplayerModel(handle, lobbyChat, dispatcher);
        } else {
            UploadProgressCallbackHandler uploadHandler = callbackHandler ?? throw new Exception("Expected upload callback handler");
            return new OnlineModel(handle, lobbyChat, dispatcher, uploadHandler, cancelTime);
        }

    }

}
