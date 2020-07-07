using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Database;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Info struct used by a <see cref="Session"/> instance to infer information.
    /// </summary>
    public struct SessionInfo {

        public const string AI_Easy = "AI - Easy\x03";
        public const string AI_Standard = "AI - Standard\x03";
        public const string AI_Hard = "AI - Hard\x03";
        public const string AI_Expert = "AI - Expert\x03";

        /// <summary>
        /// The selected game mode.
        /// </summary>
        public IWinconditionMod SelectedGamemode { get; set; }

        /// <summary>
        /// The selected game mode option.
        /// </summary>
        public int SelectedGamemodeOption { get; set; }

        /// <summary>
        /// The selected scenario.
        /// </summary>
        public Scenario SelectedScenario { get; set; }

        /// <summary>
        /// The selected tuning mod to play with.
        /// </summary>
        public ITuningMod SelectedTuningMod { get; set; }

        /// <summary>
        /// The allied players.
        /// </summary>
        public string[] Allies { get; set; }

        /// <summary>
        /// The axis players.
        /// </summary>
        public string[] Axis { get; set; }

        /// <summary>
        /// Fill in AI players if there are not enough players on one team.
        /// </summary>
        public bool FillAI { get; set; }

    }

}
