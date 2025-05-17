namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record UIExtension(string ScreenName, string BriefText, string ExtraText, string IconName, string SymbolIconName, string PortraitName)
    : BlueprintExtension(nameof(UIExtension));
