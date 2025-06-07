namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record LoadoutExtension(IList<LoadoutExtension.LoadoutData> Loadout)
    : BlueprintExtension(nameof(LoadoutExtension)) {
    public sealed record LoadoutData(int Number, string BlueprintReference);
}
