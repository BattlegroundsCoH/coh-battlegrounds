using Battlegrounds.Game;
using Battlegrounds.Game.Blueprints;

namespace Battlegrounds.Resources.Extensions;

public static class UIExtensionSource {

    public static IIconSource GetUnitIcon(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.UNIT_ICONS, blueprint.UI.Icon),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.UNIT_ICONS, blueprint.UI.Icon),
        _ => new DefaultIconSource(ResourceIdenitifers.UNIT_ICONS, blueprint.UI.Icon)
    };

    public static IIconSource GetEntityIcon(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.ENTITY_ICONS, blueprint.UI.Icon),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.ENTITY_ICONS, blueprint.UI.Icon),
        _ => new DefaultIconSource(ResourceIdenitifers.ENTITY_ICONS, blueprint.UI.Icon)
    };

    public static IIconSource GetAbilityIcon(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.ABILITY_ICONS, blueprint.UI.Icon),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.COH3_ICONS, blueprint.UI.Icon),
        _ => new DefaultIconSource(ResourceIdenitifers.ABILITY_ICONS, blueprint.UI.Icon)
    };

    public static IIconSource GetUpgradeIcon(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.UPGRADE_ICONS, blueprint.UI.Icon),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.UPGRADE_ICONS, blueprint.UI.Icon),
        _ => new DefaultIconSource(ResourceIdenitifers.UPGRADE_ICONS, blueprint.UI.Icon)
    };

    public static IIconSource GetSymbol(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.SYMBOL_ICONS, blueprint.UI.Symbol),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.SYMBOL_ICONS, blueprint.UI.Symbol),
        _ => new DefaultIconSource(ResourceIdenitifers.SYMBOL_ICONS, blueprint.UI.Icon)
    };

    public static IIconSource GetEntitySymbol(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.ENTITY_SYMBOL_ICONS, blueprint.UI.Symbol),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.ENTITY_SYMBOL_ICONS, blueprint.UI.Symbol),
        _ => new DefaultIconSource(ResourceIdenitifers.ENTITY_SYMBOL_ICONS, blueprint.UI.Icon)
    };

    public static IIconSource GetPortrait(this IUIBlueprint blueprint) => blueprint.Game switch {
        GameCase.CompanyOfHeroes2 => new DefaultIconSource(ResourceIdenitifers.PORTRAITS, blueprint.UI.Portrait),
        GameCase.CompanyOfHeroes3 => ResolveCoH3LegacyIcon(ResourceIdenitifers.PORTRAITS, blueprint.UI.Portrait),
        _ => new DefaultIconSource(ResourceIdenitifers.PORTRAITS, blueprint.UI.Icon)
    };

    private static IIconSource ResolveCoH3LegacyIcon(string container, string identifier) {
        string pathname = Path.GetFileNameWithoutExtension(identifier);
        var legacySource = new DefaultIconSource(container, pathname);
        if (ResourceHandler.HasIcon(legacySource)) {
            return legacySource;
        }
        return new DefaultIconSource(ResourceIdenitifers.COH3_ICONS, identifier);
    }

}
