namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record TeamWeaponExtension(BlueprintReference<SquadBlueprint> RecrewSquadBlueprint) 
    : BlueprintExtension(nameof(TeamWeaponExtension)) {

    public static readonly TeamWeaponExtension None = new(BlueprintReference<SquadBlueprint>.Empty);

}
