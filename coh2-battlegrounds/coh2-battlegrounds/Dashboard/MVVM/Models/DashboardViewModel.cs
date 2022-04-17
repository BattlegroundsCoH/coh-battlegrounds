using BattlegroundsApp.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp.Dashboard.MVVM.Models;

public class DashboardViewModel : IViewModel {
    public bool SingleInstanceOnly => true;

    public bool KeepAlive => true;

    public bool UnloadViewModel() => true;
    public void Swapback() {

    }

}
