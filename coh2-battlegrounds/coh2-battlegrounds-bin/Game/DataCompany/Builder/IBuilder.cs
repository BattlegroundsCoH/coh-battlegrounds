namespace Battlegrounds.Game.DataCompany.Builder;

/// <summary>
/// Interface for common functionality in builders.
/// </summary>
public interface IBuilder {

    /// <summary>
    /// Get if a change was made in the builder.
    /// </summary>
    bool IsChanged { get; }

    /// <summary>
    /// Get if there are any actions that can be undone.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Get if there are any actions that can be redone.
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// Undo most recently taken action.
    /// </summary>
    void Undo();

    /// <summary>
    /// Redo the most recently undone action.
    /// </summary>
    void Redo();

}
