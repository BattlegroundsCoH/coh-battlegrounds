using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    /// <summary>
    /// Interaction logic for AbilitySlot.xaml
    /// </summary>
    public partial class AbilitySlot : UserControl, INotifyPropertyChanged {

        public string AbilityName { get; }

        public string AbilityDescription { get; }

        public ImageSource AbilityIcon { get; }

        public Visibility IsRemoveAvailable { get; set; }

        public Visibility IsGranted { get; set; }

        public event Action<AbilitySlot> OnRemove;

        public event PropertyChangedEventHandler PropertyChanged;

        public Ability Ability { get; }

        public AbilitySlot(Ability specialAbility) {

            // Set context
            this.DataContext = this;

            // Set target
            this.Ability = specialAbility;

            // Set ability name and icon
            this.AbilityName = GameLocale.GetString(specialAbility.ABP.UI.ScreenName);
            this.AbilityDescription = GameLocale.GetString(specialAbility.ABP.UI.LongDescription);
            this.AbilityIcon = App.ResourceHandler.GetIcon("ability_icons", specialAbility.ABP.UI.Icon);

            // Set as removable
            this.IsRemoveAvailable = Visibility.Visible;
            this.IsGranted = Visibility.Collapsed;

            // Init elements
            this.InitializeComponent();

            // Set cost
            this.CostDisplay.Cost = specialAbility.ABP.Cost;

        }

        public void UpdateUnitData(ModPackage.FactionData.UnitAbility unitData) {

            // Get blueprint
            var sbp = BlueprintManager.FromBlueprintName<SquadBlueprint>(unitData.Blueprint);
            if (sbp is null) {
                return; // Bail
            }

            // Update removability
            this.IsRemoveAvailable = Visibility.Collapsed;
            this.IsGranted = Visibility.Visible;

            // Update icon
            this.GranteeIcon.IconName = sbp.UI.Icon;
            this.GranteeIcon.ToolTip = $"This ability is made available by {GameLocale.GetString(sbp.UI.ScreenName)}.";

            // Update UI
            this.PropertyChanged?.Invoke(this, new(nameof(this.IsRemoveAvailable)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.IsGranted)));

        }

        private void RemoveAbility(object sender, RoutedEventArgs e) {
            if (YesNoDialogViewModel.ShowYesNoDialog("Remove Ability", "Are you sure? This action can not be undone.") is YesNoDialogResult.Confirm) {
                this.OnRemove?.Invoke(this);
            }
        }

    }

}
