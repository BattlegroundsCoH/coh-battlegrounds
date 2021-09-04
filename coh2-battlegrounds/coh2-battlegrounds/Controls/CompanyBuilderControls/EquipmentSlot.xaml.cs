using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Locale;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    /// <summary>
    /// Interaction logic for EquipmentSlot.xaml
    /// </summary>
    public partial class EquipmentSlot : UserControl {

        public string EquipmentName { get; }

        public ImageSource EquipmentImage { get; }

        public Blueprint Equipment { get; }

        public event Action<EquipmentSlot> OnRemove;
        public event Action<EquipmentSlot, SquadBlueprint> OnEquipped;

        public EquipmentSlot(Blueprint blueprint) {

            // Set datacontext
            this.DataContext = this;

            // Save blueprint
            this.Equipment = blueprint;

            // Get UI blueprint
            IUIBlueprint uibp = blueprint as IUIBlueprint;

            // Get display data
            this.EquipmentName = GameLocale.GetString(uibp.UI.ScreenName);
            this.EquipmentImage = uibp switch {
                SlotItemBlueprint => App.ResourceHandler.GetIcon("item_icons", uibp.UI.Icon),
                EntityBlueprint => App.ResourceHandler.GetIcon("unit_icons", uibp.UI.Icon),
                _ => null
            };

            // Init
            this.InitializeComponent();

            // Set cost
            if (blueprint is EntityBlueprint ebp) {
                this.CostDisplay.Cost = ebp.Cost;
            }

        }

        private void RemoveEquipment(object sender, RoutedEventArgs e) {
            if (YesNoDialogViewModel.ShowYesNoDialog(new LocaleKey("EquipmentSlot_YesNoDialog_Remove_Equipment_Title"), new LocaleKey("EquipmentSlot_YesNoDialog_Remove_Equipment_Message")) is YesNoDialogResult.Confirm) {
                this.OnRemove?.Invoke(this);
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {

            // Make sure we're equipping with a squad
            if (e.Data.GetData("Squad") is SquadBlueprint sbp) {

                // Trigger event in main builder
                this.OnEquipped?.Invoke(this, sbp);

                // Mark handled
                e.Effects = DragDropEffects.Move;
                e.Handled = true;

            }

        }

    }

}
