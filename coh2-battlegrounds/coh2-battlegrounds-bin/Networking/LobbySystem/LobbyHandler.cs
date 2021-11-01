using Battlegrounds.Networking.Communication.Broker;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.DataStructures;
using Battlegrounds.Networking.LobbySystem.Playing;
using Battlegrounds.Networking.Remoting;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Requests;

namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// 
    /// </summary>
    public class LobbyHandler {

        /// <summary>
        /// 
        /// </summary>
        public ILobby Lobby { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IObjectID LobbyID { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public ILobbyParticipant Self { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IObjectPool ObjectPool { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IStaticInterface StaticInterface { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IConnection Connection { get; init; }

        /// <summary>
        /// Get or set the current match context of  the handler (<see langword="null"/> by default).
        /// </summary>
        public ILobbyMatchContext MatchContext { get; set; }

        /// <summary>
        /// Get or set the match start/close timer.
        /// </summary>
        public ISynchronizedTimer MatchStartTimer { get; set; }

        /// <summary>
        /// Get or set the handler for broker
        /// </summary>
        public BrokerHandler BrokerHandler { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ulong LobbyUID { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHost { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="api"></param>
        /// <param name="lobby"></param>
        public LobbyHandler(bool isHost, ulong luid) {
            this.IsHost = isHost;
            this.LobbyUID = luid;
        }

    }

}
