using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Game.Scar {
    
    /// <summary>
    /// .NET interface for representing a Scar value.
    /// </summary>
    public interface IScarValue {

        /// <summary>
        /// Convert the instance into its scar-source-code requivalent.
        /// </summary>
        /// <returns>The instance in source code format.</returns>
        public string ToScar();

    }

}
