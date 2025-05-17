namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record VeterancyExtension(VeterancyExtension.VeterancyRank[] Ranks)
    : BlueprintExtension(nameof(VeterancyExtension)) {
    public sealed record VeterancyRank(float Experience, string ScreenName);   

    public int GetRank(float experience) {
        int rank = 0;
        for (int i = 0; i < Ranks.Length; i++) {
            if (experience >= Ranks[i].Experience) {
                rank = i;
            } else {
                break;
            }
        }
        return rank;
    }

}
