using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// Representation of a <see cref="Blueprint"/> with upgrade specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
    /// </summary>
    public class UpgradeBlueprint : Blueprint {

        /// <summary>
        /// The UI symbol used in-game to show unit type
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The UI icon of the squad
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The localized string ID for the display name of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public string LocaleName { get; set; }

        /// <summary>
        /// The localized string ID for the description of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public string LocaleDescription { get; set; }

        /// <summary>
        /// The base <see cref="Gameplay.Cost"/> to field instances of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public Cost Cost { get; set; }

    }

}
