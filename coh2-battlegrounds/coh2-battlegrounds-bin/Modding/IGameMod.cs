using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Modding {
    
    /// <summary>
    /// 
    /// </summary>
    public interface IGameMod {
    
        /// <summary>
        /// 
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

    }

}
