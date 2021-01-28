using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for CompanyBuilderView.xaml
    /// </summary>
    public partial class CompanyBuilderView : ViewState {

        private CompanyBuilder builder;
        public override void StateOnFocus() { }
        public override void StateOnLostFocus() { }

        public CompanyBuilderView() {
            InitializeComponent();
        }

        public CompanyBuilderView(Company company) : this() {
            builder = new CompanyBuilder().DesignCompany(company);
        }

        public CompanyBuilderView(string companyName, Faction faction, CompanyType type) : this() {
            builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type);
        }

        private void InfantryButton_Click(object sender, RoutedEventArgs e) {

        }

        private void SupportButton_Click(object sender, RoutedEventArgs e) {

        }

        private void VehicleButton_Click(object sender, RoutedEventArgs e) {

        }

        private void CapcuredButton_Click(object sender, RoutedEventArgs e) {

        }

        private void AbilitiesButton_Click(object sender, RoutedEventArgs e) {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            var comapny = builder.Commit().Result;
            PlayerCompanies.SaveCompany(comapny);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            this.StateChangeRequest(new CompanyView());
        }
    }
}
