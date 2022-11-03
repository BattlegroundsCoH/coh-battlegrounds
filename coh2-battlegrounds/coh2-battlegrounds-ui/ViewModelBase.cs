using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Battlegrounds.UI;

/// <summary>
/// Abstract class that represents the basics of a view model.
/// </summary>
public abstract class ViewModelBase : IViewModel {

    public abstract bool SingleInstanceOnly { get; }

    public abstract bool KeepAlive { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void Swapback() { }

    public virtual void UnloadViewModel(OnModelClosed onClosed, bool requestDestroyed) {
        onClosed(false);
    }

    /// <summary>
    /// Notify any <see cref="INotifyPropertyChanged"/> listeners that a propery was updated.
    /// </summary>
    /// <param name="name">The name of the property that has been changed. If empty string, the name of the caller is used.</param>
    public void Notify([CallerMemberName] string name = "") => this.PropertyChanged?.Invoke(this, new(name));

    /// <summary>
    /// Invoke the action on the main GUI thread.
    /// </summary>
    /// <param name="action">Action to invoke on the main thread.</param>
    protected static void MainThread(Action action) => Application.Current.Dispatcher.Invoke(action);

}
