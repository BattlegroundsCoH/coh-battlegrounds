using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Json;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class Wincondition : IWinconditionMod, IJsonObject {
    
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public Guid Guid => Guid.NewGuid(); // TODO: Actually implement...

        public string ToJsonReference() => this.Guid.ToString();

    }

}
