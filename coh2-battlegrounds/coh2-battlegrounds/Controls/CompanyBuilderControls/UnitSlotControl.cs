using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public class UnitSlotControl : TabControl, IStateMachine<UnitSlotState> {
        public UnitSlotState State { get; set; }

        public UnitSlotControl() {
            Style style = new Style();
            style.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
            this.ItemContainerStyle = style;
            this.Background = Brushes.Transparent;
            this.BorderBrush = Brushes.Transparent;
            this.BorderThickness = new Thickness(0);
        }

        public StateChangeRequestHandler GetRequestHandler() => StateChangeRequest;
        public void SetState(UnitSlotState state) {
            this.State?.StateOnLostFocus();
            this.Dispatcher.Invoke(() => {
                this.SelectedIndex = this.Items.IndexOf(state);
                this.State = state;
                this.State.StateOnFocus();
            });
        }
        public bool StateChangeRequest(object request) {
            if (request is UnitSlotStateType type) {
                if (type == UnitSlotStateType.Occupied) {
                    foreach (object state in this.Items) {
                        if (state is OccupiedState occupiedState) {
                            this.SetState(occupiedState);
                            return true;
                        }
                    }
                } else if (type == UnitSlotStateType.Add) {
                    foreach (object state in this.Items) {
                        if (state is AddState addState) {
                            this.SetState(addState);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
