using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BattlegroundsApp.MVVM;

public abstract class ViewModelBase : IViewModel {

    public abstract bool SingleInstanceOnly { get; }

    public abstract bool KeepAlive { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void Swapback() { }

    public virtual void UnloadViewModel(OnModelClosed onClosed, bool requestDestroyed) {
        onClosed(false);
    }

    public void Notify([CallerMemberName] string name = "") => this.PropertyChanged?.Invoke(this, new(name));

}
