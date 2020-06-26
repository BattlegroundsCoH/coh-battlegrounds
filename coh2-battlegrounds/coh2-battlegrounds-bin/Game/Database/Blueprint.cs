using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Database.json;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// 
    /// </summary>
    public class Blueprint : IJsonOjbect {
    
        /// <summary>
        /// 
        /// </summary>
        public ulong PBGID { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

    }

}
