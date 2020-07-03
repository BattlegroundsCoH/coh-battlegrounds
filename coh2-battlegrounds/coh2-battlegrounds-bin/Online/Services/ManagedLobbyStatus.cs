using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// 
    /// </summary>
    public readonly struct ManagedLobbyStatus {
        
        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_s"></param>
        public ManagedLobbyStatus(bool _s) {
            this.Success = _s;
            this.Message = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_s"></param>
        /// <param name="_m"></param>
        public ManagedLobbyStatus(bool _s, string _m) {
            this.Success = _s;
            this.Message = _m;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.Message;

    }

}
