using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Steam {

    /// <summary>
    /// Represents a Steam user
    /// </summary>
    public sealed class SteamUser {
    
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public ulong ID { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="steamUID"></param>
        public SteamUser(ulong steamUID) {
            this.ID = steamUID;
            this.Name = "";
        }

    }

}
