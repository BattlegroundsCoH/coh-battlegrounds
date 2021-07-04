namespace Battlegrounds.Modding {

    /// <summary>
    /// Interface for a tuning mod. Extension of the <see cref="IGameMod"/> interface.
    /// </summary>
    public interface ITuningMod : IGameMod {
    
        /// <summary>
        /// Get the blueprint name of the tuning verification upgrade.
        /// </summary>
        public string VerificationUpgrade { get; }

        /// <summary>
        /// Get the upgrade blueprint name to apply when being towed.
        /// </summary>
        public string TowUpgrade { get; }

        /// <summary>
        /// Get the upgrade blueprint name to apply when towing.
        /// </summary>
        public string TowingUpgrade { get; }

    }

}
