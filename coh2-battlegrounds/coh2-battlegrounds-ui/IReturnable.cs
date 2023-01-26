namespace Battlegrounds.UI;

/// <summary>
/// Interface for returning a <see cref="IViewModel"/> to another under certain conditions.
/// </summary>
public interface IReturnable {
    
    /// <summary>
    /// Get or set the <see cref="IViewModel"/> the implementer shall return view control to.
    /// </summary>
    IViewModel? ReturnTo { get; set; }

}
