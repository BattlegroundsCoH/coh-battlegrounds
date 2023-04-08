using Battlegrounds.Game.Blueprints.Extensions;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Interface for a <see cref="Blueprint"/> with an <see cref="UIExtension"/> property.
/// </summary>
public interface IUIBlueprint {

    /// <summary>
    /// Get the UI extension of the blueprint.
    /// </summary>
    UIExtension UI { get; }

}
