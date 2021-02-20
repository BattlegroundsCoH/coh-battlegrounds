using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public class OccupiedState : UnitSlotState {
        public string UnitName { get; set; }
        public override bool isOccupied => true;

        public void SetUnit(Squad squad) {
            UnitName = squad.SBP.Name;
        }
    }
}
