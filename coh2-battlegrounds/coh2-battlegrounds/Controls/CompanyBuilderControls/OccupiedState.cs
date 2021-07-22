using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public class OccupiedState : UnitSlotState {
        public string UnitName { get; set; }
        public string UnitIcon { get; set; }
        public CostExtension UnitCost { get; set; }
        public byte UnitVeterancy { get; set; }
        public bool UnitIsTransported { get; set; }

        public override bool isOccupied => true;

        public void SetUnit(Squad squad) {
            UnitName = GameLocale.GetString(uint.Parse(squad.SBP.LocaleName));
            UnitIcon = $"pack://application:,,,/Resources/ingame/unit_icons/{squad.SBP.Icon}.png";
            UnitCost = squad.SBP.Cost;
            UnitVeterancy = squad.VeterancyRank;
            UnitIsTransported = squad.SupportBlueprint is not null;
        }
    }
}
