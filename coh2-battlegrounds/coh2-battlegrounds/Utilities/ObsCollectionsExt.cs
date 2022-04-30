using System.Collections.ObjectModel;

namespace BattlegroundsApp.Utilities;

public static class ObsCollectionsExt {

    public static void ClearTo<T>(this ObservableCollection<T> obs, T item) {
        obs.Add(item);
        while (obs.Count > 1) {
            obs.RemoveAt(0);
        }
    }

}
