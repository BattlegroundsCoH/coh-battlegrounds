using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Networking.Lobby;
using BattlegroundsApp.Controls;
using BattlegroundsApp.Controls.Lobby;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Models;

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
        public const string AISTATE = "AIHostState";

        private string m_playerCompany;
        private LobbyHandler m_handler;
        private LobbyTeamType m_team;
        private int m_teamSlotIndex;

        public string Playername { get; set; }

        public string Playercompany { get => this.m_playerCompany; set => this.m_playerCompany = string.IsNullOrEmpty(value) ? "No Company Selected" : value; }

        public string Playerarmy { get; set; }

        public bool IsAllies { get; set; }

        public string CardState => this.State.StateName;

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

        public void Init(LobbyHandler handler, LobbyTeamType teamType, int slotIndex) {
            this.m_handler = handler;
            this.m_team = teamType;
            this.m_teamSlotIndex = slotIndex;
        }

        public void SetCardState(string statename) => this.TrySetStateByName(statename);

        public void RefreshArmyData() {

            // Disable events
            this.ArmySelector.EnableEvents = false;

            // Update army selector
            if (this.IsAllies) {
                this.ArmySelector.SetItemSource(AlliedArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName, x));
            } else {
                this.ArmySelector.SetItemSource(AxisArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName, x));
            }

            // Set selected index
            this.ArmySelector.SelectedIndex = AlliedArmyItems.IndexOf(x => x.Name == this.Playerarmy);

            // Enable events
            this.ArmySelector.EnableEvents = true;

            // Refresh Company data
            this.RefreshCompanyData();

        }

        public void RefreshCompanyData() {

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
                    this.CompanySelector.SelectedIndex = 0;
                }

            }

        }

        public bool IsPlayReady() {
            if (this.CardState is OCCUPIEDSTATE) {

            } else if (this.CardState is SELFSTATE) {
                if (this.CompanySelector.SelectedItem is TeamPlayerCompanyItem companyItem) {
                    return companyItem.State is CompanyItemState.Company or CompanyItemState.Generate;
                }
            } else {
                return true;
            }
            return false;
        }

        private static TeamPlayerCompanyItem CompanyItemFromCompany(Company company)
            => new TeamPlayerCompanyItem(CompanyItemState.Company, company.Name, company.Army.Name, company.Strength);

        private void ArmySelector_SelectionChanged(object sender, IconComboBoxItem newItem) {
            if (newItem.GetSource(out TeamPlayerArmyItem item) && item.Name != this.Playerarmy) {
                this.Playerarmy = item.Name;
                this.OnFactionChangedHandle?.Invoke(item);
            }
        }

        private void CompanySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => this.OnFactionChangedHandle?.Invoke((sender == this.CompanySelector ? this.CompanySelector.SelectedItem : this.AICompanySelector.SelectedItem) as TeamPlayerArmyItem);

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {
            
            // Hide if right-clicked on self
            if (this.GetLobbyTeamFromType(this.m_team).GetSlotAt(this.m_teamSlotIndex).SlotOccupant?.Equals(this.m_handler.Self) ?? false) { // Should be cached so participants will use this less
                e.Handled = true;
                this.ContextMenu.Visibility = Visibility.Collapsed;
            }

            // Set AI visibility data
            Visibility aiVsibility = (this.m_handler.IsHost && this.CardState is OPENSTATE or LOCKEDSTATE) ? Visibility.Visible : Visibility.Collapsed;
            this.ContextMenu_AISeperator.Visibility = aiVsibility;
            this.ContextMenu_EasyAI.Visibility = aiVsibility;
            this.ContextMenu_StandardAI.Visibility = aiVsibility;
            this.ContextMenu_HardAI.Visibility = aiVsibility;
            this.ContextMenu_ExpertAI.Visibility = aiVsibility;

            // Set lock/unlock
            this.ContextMenu_LockUnlock.Visibility = (this.CardState is OCCUPIEDSTATE or SELFSTATE or OBSERVERSTATE) ? Visibility.Collapsed : Visibility.Visible;
            this.ContextMenu_LockUnlock.Header = this.CardState is LOCKEDSTATE ? "Unlock Slot" : "Lock Slot";

            // Set kick
            this.ContextMenu_Kick.Visibility = (this.m_handler.IsHost && this.CardState is OBSERVERSTATE or OCCUPIEDSTATE or AISTATE) ? Visibility.Visible : Visibility.Collapsed;
            this.ContextMenu_Kick.Header = (this.m_handler.IsHost && this.CardState is AISTATE) ? "Remove AI" : "Kick Player";

            // Set move
            this.ContextMenu_Position.Visibility = this.CardState is OPENSTATE ? Visibility.Visible : Visibility.Collapsed;

        }

        private void ContextMenu_LockUnlock_Click(object sender, RoutedEventArgs e) {
            if (this.CardState is LOCKEDSTATE) {
                this.TrySetStateByName(OPENSTATE);
                this.GetLobbyTeamFromType(this.m_team).GetSlotAt(this.m_teamSlotIndex).SlotState = LobbyTeamSlotState.OPEN;
            } else {
                this.TrySetStateByName(LOCKEDSTATE);
                this.GetLobbyTeamFromType(this.m_team).GetSlotAt(this.m_teamSlotIndex).SlotState = LobbyTeamSlotState.LOCKED;
            }
        }

        private void ContextMenu_Position_Click(object sender, RoutedEventArgs e) {

        }

        private void ContextMenu_Kick_Click(object sender, RoutedEventArgs e) {

        }

        private void ContextMenu_AddAI_Click(object sender, RoutedEventArgs e) {

        }

        private ILobbyTeam GetLobbyTeamFromType(LobbyTeamType lobbyTeamType) => lobbyTeamType switch {
            LobbyTeamType.Allies => this.m_handler.Lobby.AlliesTeam,
            LobbyTeamType.Axis => this.m_handler.Lobby.AxisTeam,
            LobbyTeamType.Observers => this.m_handler.Lobby.SpectatorTeam,
            _ => throw new Exception()
        };

    }

}
