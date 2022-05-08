using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem;
public interface ILobbyMatchNotifier {

    /// <summary>
    /// Event triggered when the host has pressed the "begin match" button.
    /// </summary>
    public event LobbyEventHandler? OnLobbyBeginMatch;

    /// <summary>
    /// Event triggered when the game should be launched.
    /// </summary>
    public event LobbyEventHandler? OnLobbyLaunchGame;

    /// <summary>
    /// Event triggered when the connection to the lobby was lost.
    /// </summary>
    public event LobbyEventHandler<string>? OnLobbyConnectionLost;

    /// <summary>
    /// Event triggered when the match startup has been cancelled.
    /// </summary>
    public event LobbyEventHandler<LobbyMatchStartupCancelledEventArgs>? OnLobbyCancelStartup;

    /// <summary>
    /// Event triggered when the lobby has sent a request for the local machine's company file.
    /// </summary>
    public event LobbyEventHandler<ServerAPI>? OnLobbyRequestCompany;

    /// <summary>
    /// Event triggered when the lobby has been asked to notify the local machine the gamemode is ready for download.
    /// </summary>
    public event LobbyEventHandler<ServerAPI>? OnLobbyNotifyGamemode;

    /// <summary>
    /// Event triggered when the lobby has been asked to notify the local machine the results of the latest match are available.
    /// </summary>
    public event LobbyEventHandler<ServerAPI>? OnLobbyNotifyResults;

    /// <summary>
    /// Event triggered when the server has sent a countdown event.
    /// </summary>
    public event LobbyEventHandler<int>? OnLobbyCountdown;

    /// <summary>
    /// Event triggered when the host has sent error information regarding the match.
    /// </summary>
    public event LobbyEventHandler<LobbyMatchInfoEventArgs>? OnLobbyMatchError;

    /// <summary>
    /// Event triggered when the host has sent information regarding the match.
    /// </summary>
    public event LobbyEventHandler<LobbyMatchInfoEventArgs>? OnLobbyMatchInfo;

    /// <summary>
    /// Event triggerd when the participants report their ready status.
    /// </summary>
    public event LobbyEventHandler<string>? OnPoll;

    /// <summary>
    /// Event triggered when the host has sent a lobby halt message
    /// </summary>
    public event LobbyEventHandler? OnLobbyMatchHalt;

}
