using Battlegrounds.Game.Database;

using BattlegroundsApp.Resources;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public partial class SquadSlotSmall : UserControl, INotifyPropertyChanged {

        private bool m_canAdd = true;

        public string SquadName { get; }

        public ImageSource SquadIcon { get; }

        public SquadBlueprint Squad { get; }

        public ObjectHoverData HoverData { get; }

        public bool CanAdd {
            get => this.m_canAdd;
            set {
                this.m_canAdd = value;
                this.PropertyChanged?.Invoke(this, new(nameof(this.CanAdd)));
            }
        }

        public event Action<SquadSlotSmall, bool> OnHoverUpdate;

        public event PropertyChangedEventHandler PropertyChanged;

        public SquadSlotSmall(SquadBlueprint squad) {
            this.DataContext = this;
            this.SquadName = GameLocale.GetString(squad.UI.ScreenName);
            this.SquadIcon = App.ResourceHandler.GetIcon("unit_icons", squad.UI.Icon);
            this.Squad = squad;
            this.HoverData = new(this.Squad);
            this.InitializeComponent();
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            base.OnMouseMove(e);

            if (!this.CanAdd) {
                return;
            }

            if (e.LeftButton is MouseButtonState.Pressed) {

                DataObject obj = new DataObject();
                obj.SetData("Squad", this.Squad);

                DragDrop.DoDragDrop(this, obj, DragDropEffects.Move);

            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
            => this.OnHoverUpdate?.Invoke(this, true);

        private void OnMouseLeave(object sender, MouseEventArgs e)
            => this.OnHoverUpdate?.Invoke(this, false);

    }
}
