namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record LoadoutExtension()
    : BlueprintExtension(nameof(LoadoutExtension)) {
    public sealed record LoadoutData(int Number, string BlueprintReference);
}
