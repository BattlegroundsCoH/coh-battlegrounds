using System;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Battlegrounds {

    /// <summary>
    /// The battlegrounds tuning mod. Implements <see cref="ITuningMod"/>. This class cannot be inherited.
    /// </summary>
    public sealed class BattlegroundsTuning : ITuningMod {

        /// <summary>
        /// 
        /// </summary>
        public Guid Guid => Guid.Parse("142b113740474c82a60b0a428bd553d5");

        /// <summary>
        /// 
        /// </summary>
        public string Name => "Battlegrounds";

        /// <summary>
        /// 
        /// </summary>
        public string VerificationUpgrade => "bg_verify";

        /// <summary>
        /// 
        /// </summary>
        public string TowUpgrade => "is_towed";

        /// <summary>
        /// 
        /// </summary>
        public string TowingUpgrade => "is_towing";

    }

}
