namespace Battlegrounds.Game {
    
    /// <summary>
    /// Enum representation of ingame AI difficulty levels
    /// </summary>
    public enum AIDifficulty {
    
        /// <summary>
        /// Human (Not AI)
        /// </summary>
        Human,

        /// <summary>
        /// Easy AI
        /// </summary>
        AI_Easy,

        /// <summary>
        /// Standard AI
        /// </summary>
        AI_Standard,

        /// <summary>
        /// Hard AI
        /// </summary>
        AI_Hard,

        /// <summary>
        /// Expert AI
        /// </summary>
        AI_Expert,

    }

    /// <summary>
    /// Extension class to the <see cref="AIDifficulty"/> enum.
    /// </summary>
    public static class AIExtensions {

        /// <summary>
        /// Check if a <see cref="AIDifficulty"/> allows for persistency after a match has been played.
        /// </summary>
        /// <param name="difficulty">The difficulty level to check</param>
        /// <returns>True when <see cref="AIDifficulty.AI_Hard"/> or greater or if <see cref="AIDifficulty.Human"/>. Otherwise, false.</returns>
        public static bool AllowsPersistency(this AIDifficulty difficulty)
            => difficulty >= AIDifficulty.AI_Hard || difficulty == AIDifficulty.Human;

        /// <summary>
        /// Get the ingame display name of a <see cref="AIDifficulty"/>.
        /// </summary>
        /// <param name="difficulty">The difficulty to get display name of.</param>
        /// <returns>The <see cref="string"/> representation of an <see cref="AIDifficulty"/> value. Empty if <see cref="AIDifficulty.Human"/>.</returns>
        public static string GetIngameDisplayName(this AIDifficulty difficulty)
            => difficulty switch
            {
                AIDifficulty.AI_Easy => "AI - Easy",
                AIDifficulty.AI_Expert => "AI - Expert",
                AIDifficulty.AI_Hard => "AI - Hard",
                AIDifficulty.AI_Standard => "AI - Standard",
                _ => string.Empty
            };

    }

}
