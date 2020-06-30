using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// Represents a <see cref="Blueprint"/> for the behaviour of instances within Company of Heroes 2. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public class Blueprint : IJsonObject {
    
        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public ulong PBGID { get; set; }

        /// <summary>
        /// The name of the <see cref="Blueprint"/> file in the game files (See the instances folder in the mod tools).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BlueprintType BlueprintType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => $"{this.BlueprintType}:{this.PBGID}";

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{BlueprintType}:{this.Name}";

    }

}
