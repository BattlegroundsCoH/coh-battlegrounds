namespace Battlegrounds.Models.Playing;

/// <summary>
/// Specifies the available difficulty levels for AI opponents or human players in the game.
/// </summary>
/// <remarks>Use this enumeration to select the desired challenge level for AI-controlled opponents. Each value
/// corresponds to a distinct AI behavior and strategy, ranging from human-controlled to expert-level AI. The selected
/// difficulty affects the AI's decision-making and overall gameplay experience.</remarks>
public enum AIDifficulty : byte {

    /// <summary>
    /// Represents the human category in the enumeration.
    /// </summary>
    HUMAN = 0, // Human player

    /// <summary>
    /// Represents the easy difficulty level.
    /// </summary>
    EASY = 1, // Easy AI

    /// <summary>
    /// Represents the normal state or standard difficulty level.
    /// </summary>
    NORMAL = 2, // Normal AI

    /// <summary>
    /// Represents a hard difficulty level.
    /// </summary>
    HARD = 3, // Hard AI

    /// <summary>
    /// Represents the expert difficulty level.
    /// </summary>
    EXPERT = 4, // Expert AI

}

/// <summary>
/// Provides extension methods for the AIDifficulty enumeration, enabling conversion between difficulty names and their
/// corresponding enumeration values.
/// </summary>
/// <remarks>This static class includes methods to retrieve the string representation of an AIDifficulty value and
/// to parse a string into an AIDifficulty enumeration. These extensions are useful for displaying difficulty levels in
/// user interfaces and for parsing user input or configuration values into strongly typed enumeration values.</remarks>
public static class AIDifficultyExtensions {

    extension(AIDifficulty self) {

        /// <summary>
        /// Gets the display name that corresponds to the current AI difficulty level.
        /// </summary>
        /// <remarks>The returned value matches the difficulty level defined in the AIDifficulty
        /// enumeration. An ArgumentOutOfRangeException is thrown if the value of the underlying difficulty is not
        /// defined in the enumeration.</remarks>
        public string Name => self switch {
            AIDifficulty.HUMAN => "Human",
            AIDifficulty.EASY => "Easy",
            AIDifficulty.NORMAL => "Normal",
            AIDifficulty.HARD => "Hard",
            AIDifficulty.EXPERT => "Expert",
            _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
        };

        /// <summary>
        /// Converts the specified string representation of an AI difficulty level to its corresponding <see
        /// cref="AIDifficulty"/> enumeration value.
        /// </summary>
        /// <remarks>The conversion is case-insensitive; the method normalizes the input to lowercase
        /// before matching. Use this method to map user input or configuration values to the corresponding <see
        /// cref="AIDifficulty"/> enumeration.</remarks>
        /// <param name="name">The name of the AI difficulty level to convert. Valid values are "human", "easy", "normal", "hard", or
        /// "expert". The comparison is case-insensitive.</param>
        /// <returns>The <see cref="AIDifficulty"/> value that corresponds to the specified difficulty level name.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> does not match any valid AI difficulty level.</exception>
        public static AIDifficulty FromName(string name) => name.ToLowerInvariant() switch {
            "human" or "" => AIDifficulty.HUMAN,
            "easy" => AIDifficulty.EASY,
            "normal" => AIDifficulty.NORMAL,
            "hard" => AIDifficulty.HARD,
            "expert" => AIDifficulty.EXPERT,
            _ => throw new ArgumentException("Invalid AI difficulty name", nameof(name))
        };

    }

}
