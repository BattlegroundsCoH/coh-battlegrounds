using Battlegrounds.Game.Database.Extensions;

namespace Battlegrounds.Game.Database;

/// <summary>
/// Interface for a <see cref="Blueprint"/> with an <see cref="UIExtension"/> property.
/// </summary>
public interface IUIBlueprint {

    /// <summary>
    /// Get the UI extension of the blueprint.
    /// </summary>
    UIExtension UI { get; }

}
