namespace Battlegrounds.Models.Blueprints.Extensions;

public sealed record HoldExtension(bool CanTow, bool EnablePassengers) : BlueprintExtension(nameof(HoldExtension)) {
    public HoldExtension() : this(false, false) { }
}
