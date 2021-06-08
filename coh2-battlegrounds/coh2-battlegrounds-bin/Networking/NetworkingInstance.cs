using System.Diagnostics;

using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking {
    
    /// <summary>
    /// 
    /// </summary>
    public static class NetworkingInstance {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool HasLocalServer()
            => Process.GetProcessesByName("bg-server").Length > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetBestAddress() {
            if (HasLocalServer()) {
                return "localhost";
            } else {
                return "194.37.80.249";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ServerAPI GetServerAPI()
            => new ServerAPI(GetBestAddress());

    }

}
