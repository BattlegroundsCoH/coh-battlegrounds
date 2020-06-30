using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Modding {

    /// <summary>
    /// 
    /// </summary>
    public interface ITuningMod : IGameMod {
    
        /// <summary>
        /// 
        /// </summary>
        public string VerificationUpgrade { get; }

    }

}
