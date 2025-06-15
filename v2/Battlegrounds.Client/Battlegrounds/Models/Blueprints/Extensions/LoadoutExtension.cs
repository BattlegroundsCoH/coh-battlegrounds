namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record LoadoutExtension(IList<LoadoutExtension.LoadoutData> Entities)
    : BlueprintExtension(nameof(LoadoutExtension)) {
    public sealed record LoadoutData(int Count, string EBP);
}
