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

/// <summary>
/// Interface for common functionality in builders of <typeparamref name="T"/> instances.
/// </summary>
/// <typeparam name="T">The instance to build.</typeparam>
public interface IBuilder<T> : IBuilder {

    /// <summary>
    /// Get the result of the build action.
    /// </summary>
    /// <remarks>
    /// Requires call to <see cref="Commit"/>.
    /// </remarks>
    T Result { get; }

    /// <summary>
    /// Commit all actions to the builder and construct <see cref="Result"/>.
    /// </summary>
    /// <param name="arg">Optional argument to give while building.</param>
    /// <returns>Calling <see cref="IBuilder{T}"/> instance.</returns>
    IBuilder<T> Commit(object? arg = null);

}
