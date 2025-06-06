namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record UIExtension(
    LocaleString ScreenName, LocaleString HelpText, LocaleString BriefText, 
    string IconName, string SymbolIconName, string PortraitName) : BlueprintExtension(nameof(UIExtension));

// HelpText => Long and detailed description of the blueprint.
// BriefText => Short description, usually one line and usually describes counters (ie. Anti-infantry, Good vs. armour)
