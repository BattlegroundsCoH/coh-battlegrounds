using Battlegrounds.ErrorHandling.CommonExceptions;

namespace Battlegrounds.ErrorHandling.Networking {
    
    /// <summary>
    /// 
    /// </summary>
    public class RemoteQueryFailedException : BattlegroundsException {

        /// <summary>
        /// 
        /// </summary>
        public RemoteQueryFailedException() : base("Failed to run command query on remote machine.") { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public RemoteQueryFailedException(string message) : base(message) { }

    }

}
