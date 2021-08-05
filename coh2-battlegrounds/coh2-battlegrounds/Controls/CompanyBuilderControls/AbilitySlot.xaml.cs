using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Game.DataCompany;

using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    /// <summary>
    /// Interaction logic for AbilitySlot.xaml
    /// </summary>
    public partial class AbilitySlot : UserControl {
        
        public string AbilityName { get; }

        public ImageSource AbilityIcon { get; }

        public event Action<AbilitySlot> OnRemove;

        public AbilitySlot(SpecialAbility specialAbility) {

            // Set context
            this.DataContext = this;

            // Set ability name and icon
            this.AbilityName = GameLocale.GetString(specialAbility.ABP.UI.ScreenName);
            this.AbilityIcon = App.ResourceHandler.GetIcon("ability_icons", specialAbility.ABP.UI.Icon);

            // Init elements
            this.InitializeComponent();

            // Set cost
            this.CostDisplay.Cost = specialAbility.ABP.Cost;

        }

        private void RemoveAbility(object sender, RoutedEventArgs e) {
            if (YesNoDialogViewModel.ShowYesNoDialog("Remove Ability", "Are you sure? This action can not be undone.") is YesNoDialogResult.Confirm) {
                this.OnRemove?.Invoke(this);
            }
        }

    }

}
