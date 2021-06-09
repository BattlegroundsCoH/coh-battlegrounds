using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using BattlegroundsApp.Controls;
using BattlegroundsApp.Controls.Lobby;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views.ViewComponent {

    public record TeamPlayerCompanyItem(CompanyItemState State, string Name, string Army, double Strength = TeamPlayerCompanyItem.UNDEFINEDSTRENGTH) {
        public const double UNDEFINEDSTRENGTH = -1.0;
        public override string ToString() => this.Name;
    }

    public record TeamPlayerArmyItem(BitmapSource Icon, string DisplayName, string Name) {
        public override string ToString() => this.DisplayName;
    }

    /// <summary>
    /// Interaction logic for TeamPlayerCard.xaml
    /// </summary>
    public partial class TeamPlayerCard : LobbyControl {

        private static TeamPlayerArmyItem[] AlliedArmyItems = new TeamPlayerArmyItem[] {
            new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/aef.png")), "US Forces", "aef"),
            new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/british.png")), "UK Forces", "british"),
            new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/soviet.png")), "Soviet Union", "soviet"),
        };

        private static TeamPlayerArmyItem[] AxisArmyItems = new TeamPlayerArmyItem[] {
            new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/german.png")), "Wehrmacht", "german"),
            new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/west_german.png")), "Oberkommando West", "west_german"),
        };

        private static ObservableCollection<TeamPlayerCompanyItem> StaticPlayerCompanyItems = new ObservableCollection<TeamPlayerCompanyItem>();

        public const string OCCUPIEDSTATE = "OccupiedState";
        public const string OPENSTATE = "AvailableState";
        public const string OBSERVERSTATE = "ObserverState";
        public const string LOCKEDSTATE = "LockedState";
        public const string SELFSTATE = "SelfState";

        private string m_playerCompany;

        public string Playername { get; set; }

        public string Playercompany { get => this.m_playerCompany; set => this.m_playerCompany = string.IsNullOrEmpty(value) ? "No Company Selected" : value; }

        public string Playerarmy { get; set; }

        public bool IsAllies { get; set; }

        public ObservableCollection<TeamPlayerCompanyItem> PlayerCompanyItems => StaticPlayerCompanyItems;

        public Action<TeamPlayerArmyItem> OnFactionChangedHandle { get; set; }

        public Action<TeamPlayerCompanyItem> OnCompanyChangedHandle { get; set; }

        public TeamPlayerCard() {

            // Initialize component
            this.InitializeComponent();

            // Update data context
            this.DataContext = this;

            // Set default values
            this.Playername = "Connecting...";
            this.Playercompany = "No Company Selected";

        }

        public void SetCardState(string statename) => this.TrySetStateByName(statename);

        public void SetSelfDataIfNone() {

            // If company items are not available
            if (this.PlayerCompanyItems.Count == 0 || this.PlayerCompanyItems.All(x => x.Army != this.Playerarmy)) {

                // Clear list
                this.PlayerCompanyItems.Clear();

                // Find all faction companies
                List<Company> all = PlayerCompanies.FindAll(x => x.Army.Name == this.Playerarmy);
                if (all.Count > 0) {
                    all.ForEach(x => this.PlayerCompanyItems.Add(CompanyItemFromCompany(x)));
                    this.CompanySelector.SelectedIndex = 0;
                } else {
                    this.PlayerCompanyItems.Add(new TeamPlayerCompanyItem(CompanyItemState.None, "No Companies Available", string.Empty));
                }

            }

            // Update army selector
            if (this.IsAllies) {
                this.ArmySelector.SetItemSource(AlliedArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName, x));
            } else {
                this.ArmySelector.SetItemSource(AxisArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName, x));
            }

            // Set selected index
            this.ArmySelector.SelectedIndex = AlliedArmyItems.IndexOf(x => x.Name == this.Playerarmy);

        }

        private static TeamPlayerCompanyItem CompanyItemFromCompany(Company company)
            => new TeamPlayerCompanyItem(CompanyItemState.Company, company.Name, company.Army.Name, company.Strength);

        private void ArmySelector_SelectionChanged(object sender, IconComboBoxItem newItem)
            => this.OnFactionChangedHandle?.Invoke(newItem.Source as TeamPlayerArmyItem);

        private void CompanySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => this.OnFactionChangedHandle?.Invoke(CompanySelector.SelectedItem as TeamPlayerArmyItem);

    }

}
