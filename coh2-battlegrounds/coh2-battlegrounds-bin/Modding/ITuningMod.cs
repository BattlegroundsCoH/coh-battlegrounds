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

        /// <summary>
        /// 
        /// </summary>
        public string TowUpgrade { get; }

        /// <summary>
        /// 
        /// </summary>
        public string TowingUpgrade { get; }

    }

}
