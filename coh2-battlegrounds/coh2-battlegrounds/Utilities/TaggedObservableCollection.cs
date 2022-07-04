using System.Collections.ObjectModel;

namespace BattlegroundsApp.Utilities;

public class TaggedObservableCollection<T> : ObservableCollection<T> {
    public object? Tag { get; set; }
}
