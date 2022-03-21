namespace Battlegrounds.Game.DataCompany.Builder;

/// <summary>
/// Interface for an editing action that can be applied and undone.
/// </summary>
public interface IEditAction<T> {

    /// <summary>
    /// Apply action on <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The target to apply action on.</param>
    /// <returns>A new <typeparamref name="T"/> where action is applied.</returns>
    T Apply(T target);

    /// <summary>
    /// Undo the performaed action.
    /// </summary>
    /// <param name="target">The target to undo action on.</param>
    /// <returns>A new <typeparamref name="T"/> where action has been undone.</returns>
    T Undo(T target);

}
