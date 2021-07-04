using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public class UnitSlotState : TabItem, IState {
        public virtual bool isOccupied { get; }
        public StateChangeRequestHandler StateChangeRequest { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public void StateOnFocus() { }
        public void StateOnLostFocus() => throw new NotImplementedException();
    }
}
