using System;
using System.Collections.Generic;
using System.Text;

using Battlegrounds.Game.Database.Management;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class SlotItemBlueprint : Blueprint {

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        public override BlueprintType BlueprintType => BlueprintType.IBP;

        public override string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LocaleName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LocaleDescription { get; set; }

    }

}
