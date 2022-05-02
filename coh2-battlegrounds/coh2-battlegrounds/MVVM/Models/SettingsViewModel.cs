using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp.MVVM.Models;

public class SettingsViewModel : IViewModel {

    public bool SingleInstanceOnly => true;

    public bool KeepAlive => false;

    public void Swapback() {
    }

    public void UnloadViewModel(OnModelClosed onClosed, bool requestDestroyed) {
        onClosed(false);
    }

}
