namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Interface for a blueprint.
/// </summary>
public interface IBlueprint {

    /// <summary>
    /// The game this blueprint is for.
    /// </summary>
    GameCase Game { get; }

}
