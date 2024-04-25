using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Core.Games;

public readonly struct AIDifficulty {

    public static readonly AIDifficulty AI_HUMAN = new AIDifficulty(0);
    public static readonly AIDifficulty AI_EASY = new AIDifficulty(1);
    public static readonly AIDifficulty AI_NORMAL = new AIDifficulty(2);
    public static readonly AIDifficulty AI_HARD = new AIDifficulty(3);
    public static readonly AIDifficulty AI_EXPERT = new AIDifficulty(4);

    private readonly int _tag;

    private AIDifficulty(int tag) {
        _tag = tag;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is AIDifficulty other && _tag == other._tag;

    public static bool operator ==(AIDifficulty left, AIDifficulty right) {
        return left.Equals(right);
    }

    public static bool operator !=(AIDifficulty left, AIDifficulty right) {
        return !(left == right);
    }

    public static implicit operator int(AIDifficulty v) => v._tag;

    public override int GetHashCode() => _tag.GetHashCode();

    public override string ToString() => _tag switch {
        0 => "AI_HUMAN",
        1 => "AI_EASY",
        2 => "AI_NORMAL",
        3 => "AI_HARD",
        4 => "AI_EXPERT",
        _ => throw new InvalidProgramException()
    };

    public static AIDifficulty FromInt(int value) => value switch {
        0 => AI_HUMAN,
        1 => AI_EASY,
        2 => AI_NORMAL,
        3 => AI_HARD,
        4 => AI_EXPERT,
        _ => throw new ArgumentException($"Invalid integer {value} - cannot convert to AI difficulty", nameof(value))
    };

}
