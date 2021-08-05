using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.Resources;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    public partial class AvailableItemSlot : UserControl, INotifyPropertyChanged {

        private bool m_canAdd = true;

        public string SquadName { get; }

        public ImageSource SquadIcon { get; }

        public Blueprint Blueprint { get; }

        public ObjectHoverData HoverData { get; }

        public bool CanAdd {
            get => this.m_canAdd;
            set {
                this.m_canAdd = value;
                this.PropertyChanged?.Invoke(this, new(nameof(this.CanAdd)));
            }
        }

        public event Action<AvailableItemSlot, bool> OnHoverUpdate;

        public event PropertyChangedEventHandler PropertyChanged;

        public AvailableItemSlot(Blueprint bp) {
            this.DataContext = this;
            if (bp is SquadBlueprint sbp) {
                this.SquadName = GameLocale.GetString(sbp.UI.ScreenName);
                this.SquadIcon = App.ResourceHandler.GetIcon("unit_icons", sbp.UI.Icon);
                this.Blueprint = sbp;
                this.HoverData = new(sbp);
            } else if (bp is AbilityBlueprint abp) {
                this.SquadName = GameLocale.GetString(abp.UI.ScreenName);
                this.SquadIcon = App.ResourceHandler.GetIcon("ability_icons", abp.UI.Icon);
                this.Blueprint = abp;
                this.HoverData = new(abp);
            } else {
                throw new ArgumentException($"Invalid blueprint type '{bp.GetType().Name}'.", nameof(bp));
            }
            this.InitializeComponent();
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            base.OnMouseMove(e);

            if (!this.CanAdd) {
                return;
            }

            if (e.LeftButton is MouseButtonState.Pressed) {

                var obj = new DataObject();
                if (this.Blueprint is SquadBlueprint sbp) {
                    obj.SetData("Squad", sbp);
                } else if (this.Blueprint is AbilityBlueprint abp) {
                    obj.SetData("Ability", abp);
                }

                DragDrop.DoDragDrop(this, obj, DragDropEffects.Move);

            }

        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
            => this.OnHoverUpdate?.Invoke(this, true);

        private void OnMouseLeave(object sender, MouseEventArgs e)
            => this.OnHoverUpdate?.Invoke(this, false);

    }

}
