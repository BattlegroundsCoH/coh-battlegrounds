using System.Net;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Helper class for getting the connection endpoints
    /// </summary>
    public static class AddressBook {

        /// <summary>
        /// The IP address of the lobby (and file) server
        /// </summary>
        public const string LobbyServerAddress = "194.37.80.249";

        private static IPEndPoint __lobbyEndPoint;
        private static IPEndPoint __fileEndPoint;

        /// <summary>
        /// Get the lobby server endpoint
        /// </summary>
        /// <returns>Lobby endpoint</returns>
        public static IPEndPoint GetLobbyServer() {

            if (__lobbyEndPoint == null) {
                __lobbyEndPoint = new IPEndPoint(IPAddress.Parse(LobbyServerAddress), 11000);
            }

            return __lobbyEndPoint;

        }

        /// <summary>
        /// Get the file server endpoint
        /// </summary>
        /// <returns>File server endpoint</returns>
        public static IPEndPoint GetFileServer() {

            if (__fileEndPoint == null) {
                __fileEndPoint = new IPEndPoint(IPAddress.Parse(LobbyServerAddress), 11010);
            }

            return __fileEndPoint;

        }

    }

}
