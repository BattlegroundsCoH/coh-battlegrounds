namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record CostExtension(float Manpower, float Munitions, float Fuel) 
    : BlueprintExtension(nameof(CostExtension));
