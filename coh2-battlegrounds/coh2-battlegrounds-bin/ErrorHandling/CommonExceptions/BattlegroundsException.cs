using System;
using System.Diagnostics;

namespace Battlegrounds.ErrorHandling.CommonExceptions {

    /// <summary>
    /// Base exception class for exceptions thrown by the Battlegrounds app.
    /// </summary>
    public class BattlegroundsException : Exception {
        
        /// <summary>
        /// 
        /// </summary>
        public BattlegroundsException() : base() {  }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public BattlegroundsException(string message) : base(message) {}

        /// <summary>
        /// Log the battlegrounds exception.
        /// </summary>
        public void Log() => Trace.WriteLine(this);

    }

}
