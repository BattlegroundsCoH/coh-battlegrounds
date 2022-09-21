using System;
using System.Collections.ObjectModel;

namespace BattlegroundsApp.Utilities;

public static class ObsCollectionsExt {

    public static void ClearTo<T>(this ObservableCollection<T> obs, T item) {
        obs.Add(item);
        while (obs.Count > 1) {
            obs.RemoveAt(0);
        }
    }

    public static void RemoveAll<T>(this ObservableCollection<T> obs, Predicate<T> predicate) {
        for (int i = 0; i < obs.Count; i++) { 
            if (predicate(obs[i])) {
                obs.RemoveAt(i--);
            }
        }
    }

}
