using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game.DataCompany;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public class EquipmentSlotViewModel {
    
    public CompanyItem Item { get; }
    
    public EquipmentSlotViewModel(CompanyItem item) {
        this.Item = item;
    }

}
