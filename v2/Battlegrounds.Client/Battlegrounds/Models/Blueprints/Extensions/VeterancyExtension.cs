
namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record VeterancyExtension(VeterancyExtension.VeterancyRank[] Ranks)
    : BlueprintExtension(nameof(VeterancyExtension)) {

    public sealed record VeterancyRank(float Experience, string ScreenName);

    private readonly VeterancyRank[] _ranks = [.. Ranks.OrderBy(rank => rank.Experience)]; // Ensure ranks are sorted by experience

    public int MaxRank => _ranks.Length;

    public int GetRank(float experience) {
        int rank = 0;
        for (int i = 0; i < _ranks.Length; i++) {
            if (experience >= _ranks[i].Experience) {
                rank = i + 1;
            } else {
                break;
            }
        }
        return rank;
    }

    public float GetRankProgress(float experience) {
        int currentRank = GetRank(experience);
        if (currentRank <= 0) {
            return experience / _ranks[0].Experience; // If no rank, progress is based on the first rank's experience
        }
        if (currentRank < MaxRank) {
            float currentExperience = _ranks[currentRank - 1].Experience;
            float nextExperience = _ranks[currentRank].Experience;
            float progress = (experience - currentExperience) / (nextExperience - currentExperience);
            return Math.Clamp(progress, 0.0f, 1.0f); // Ensure progress is between 0 and 1
        }
        return 1.0f; // If at max rank, progress is complete
    }

    public float GetNextRankExperience(float experience) {
        int currentRank = GetRank(experience);
        if (currentRank <= 0)
            return _ranks[0].Experience; // If no rank, return the first rank's experience
        else if (currentRank >= MaxRank)
            return _ranks[^1].Experience; // If at max rank, return the last rank's experience
        return _ranks[currentRank].Experience;
    }

}
