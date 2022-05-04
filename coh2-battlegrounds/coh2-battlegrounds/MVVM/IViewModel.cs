using System.ComponentModel;

namespace BattlegroundsApp.MVVM;

/// <summary>
/// Delegate handler
/// </summary>
public delegate void OnModelClosed(bool isCancelled);

/// <summary>
/// Interface for a displayable view model that can interact nicely with the <see cref="AppViewManager"/>.
/// </summary>
public interface IViewModel : INotifyPropertyChanged {

    /// <summary>
    /// Get if only a single instance of the view model is allowed to axist.
    /// </summary>
    bool SingleInstanceOnly { get; }

    /// <summary>
    /// Get if the view model should be kept alive (notified when switched back)
    /// </summary>
    bool KeepAlive { get; }

    /// <summary>
    /// Notify the view model it's being unloaded.
    /// </summary>
    /// <param name="onClosed">Callback to be invoked when done unloading</param>
    void UnloadViewModel(OnModelClosed onClosed, bool requestDestroyed);

    /// <summary>
    /// Notify the view model we've switched back to it.
    /// </summary>
    void Swapback();

}
