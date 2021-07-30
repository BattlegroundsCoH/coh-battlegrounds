using Battlegrounds.Game.Database;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Match {
    
    /// <summary>
    /// Info struct used by a <see cref="Session"/> instance to infer information.
    /// </summary>
    public struct SessionInfo {

        /// <summary>
        /// The selected game mode.
        /// </summary>
        public IGamemode SelectedGamemode { get; set; }

        /// <summary>
        /// The selected tuning mod to play with.
        /// </summary>
        public ITuningMod SelectedTuningMod { get; set; }

        /// <summary>
        /// The selected game mode option.
        /// </summary>
        public int SelectedGamemodeOption { get; set; }

        /// <summary>
        /// Get or set whether the given gamemode option is the direct value and not an index.
        /// </summary>
        public bool IsOptionValue { get; set; }

        /// <summary>
        /// The selected scenario.
        /// </summary>
        public Scenario SelectedScenario { get; set; }

        /// <summary>
        /// The Allied players participating in the <see cref="Session"/>.
        /// </summary>
        public SessionParticipant[] Allies { get; set; }

        /// <summary>
        /// The Axis players participating in the <see cref="Session"/>.
        /// </summary>
        public SessionParticipant[] Axis { get; set; }

        /// <summary>
        /// Fill in AI players if there are not enough players on one team.
        /// </summary>
        public bool FillAI { get; set; }

        /// <summary>
        /// The default difficulty to give the AI being filled in.
        /// </summary>
        public AIDifficulty DefaultDifficulty { get; set; }

    }

}
