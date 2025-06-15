namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record TypesExtension(IList<string> Values) : BlueprintExtension(nameof(TypesExtension));
