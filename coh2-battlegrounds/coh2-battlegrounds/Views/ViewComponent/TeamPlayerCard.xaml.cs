using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    public partial class TeamPlayerCard : LobbyControl, INotifyPropertyChanged {

        private static readonly TeamPlayerArmyItem[] AlliedArmyItems;
        private static readonly TeamPlayerArmyItem[] AxisArmyItems;

        static TeamPlayerCard() {
            try {
                AlliedArmyItems = new TeamPlayerArmyItem[] {
                    new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/aef.png")), "US Forces", "aef"),
                    new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/british.png")), "UK Forces", "british"),
                    new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/soviet.png")), "Soviet Union", "soviet"),
                };
                AxisArmyItems = new TeamPlayerArmyItem[] {
                    new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/german.png")), "Wehrmacht", "german"),
                    new TeamPlayerArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/west_german.png")), "Oberkommando West", "west_german"),
                };
            } catch {

            }
        }

        public const string OCCUPIEDSTATE = "OccupiedState";
        public const string OPENSTATE = "AvailableState";
        public const string OBSERVERSTATE = "ObserverState";
        public const string LOCKEDSTATE = "LockedState";
        public const string SELFSTATE = "SelfState";
        public const string AISTATE = "AIHostState";

        private string m_playerCompany;
        private LobbyHandler m_handler;
        private int m_teamSlotIndex;

        public LobbyTeamType TeamType { get; private set; }

        public ILobbyTeamSlot TeamSlot { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Playername { get; set; }

        public string Playercompany { get => this.m_playerCompany; set => this.m_playerCompany = string.IsNullOrEmpty(value) ? "No Company Selected" : value; }

        public string Playerarmy { get; set; }

        public ImageSource PlayerArmyIcon { get; set; }

        public bool IsAllies { get; set; }

        public string CardState => this.State.StateName;

        public ObservableCollection<TeamPlayerCompanyItem> PlayerCompanyItems { get; }

        public Action<TeamPlayerArmyItem> OnFactionChangedHandle { get; set; }

        public Action<TeamPlayerCompanyItem> OnCompanyChangedHandle { get; set; }

        public Action<TeamPlayerCard> RequestFullRefresh { get; set; }

        public Action NotifyLobby { get; set; }

        public TeamPlayerCard() {

            // Initialize component
            this.InitializeComponent();

            // Update data context
            this.DataContext = this;

            // Set default values
            this.Playername = "Connecting...";
            this.Playercompany = "No Company Selected";
            this.PlayerCompanyItems = new ObservableCollection<TeamPlayerCompanyItem>();

        }

        public void Init(LobbyHandler handler, LobbyTeamType teamType, int slotIndex) {
            this.m_handler = handler;
            this.TeamType = teamType;
            this.m_teamSlotIndex = slotIndex;
            this.TeamSlot = this.GetLobbyTeamFromType(this.TeamType)?.GetSlotAt(slotIndex) ?? null;
        }

        public void SetCardState(string statename) => this.TrySetStateByName(statename);

        public void RefreshArmyData() {

            // Disable events
            (this.CardState is SELFSTATE ? this.ArmySelector : this.AIArmySelector).EnableEvents = false;

            // Get index of selected army
            int toSelect = -1;

            // Update army selector
            try {
                if (this.IsAllies) {
                    (this.CardState is SELFSTATE ? this.ArmySelector : this.AIArmySelector).SetItemSource(AlliedArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName, x));
                    toSelect = AlliedArmyItems.IndexOf(x => x.Name == this.Playerarmy);
                } else {
                    (this.CardState is SELFSTATE ? this.ArmySelector : this.AIArmySelector).SetItemSource(AxisArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName, x));
                    toSelect = AxisArmyItems.IndexOf(x => x.Name == this.Playerarmy);
                }
            } catch (Exception) { }

            // Try set to selected army.
            if (toSelect != -1) {

                // Set selected index and Enable events
                if (this.CardState is SELFSTATE) {

                    this.ArmySelector.SelectedIndex = toSelect;
                    this.ArmySelector.EnableEvents = true;

                } else {

                    this.AIArmySelector.SelectedIndex = toSelect;
                    this.AIArmySelector.EnableEvents = true;

                }

                // Refresh Company data
                this.RefreshCompanyData();

            }

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
                    if (this.CardState is AISTATE) {
                        this.PlayerCompanyItems.Add(new TeamPlayerCompanyItem(CompanyItemState.Generate, "Generate Company", string.Empty));
                    }
                } else {
                    if (this.CardState is AISTATE) {
                        this.PlayerCompanyItems.Add(new TeamPlayerCompanyItem(CompanyItemState.Generate, "Generate Company", string.Empty));
                    } else {
                        this.PlayerCompanyItems.Add(new TeamPlayerCompanyItem(CompanyItemState.None, "No Companies Available", string.Empty));
                    }
                }

                // Set selected index
                (this.CardState is AISTATE ? this.AICompanySelector : this.CompanySelector).SelectedIndex = 0;

            }

        }

        public bool IsPlayReady() {
            if (this.CardState is SELFSTATE) {
                if (this.CompanySelector.SelectedItem is TeamPlayerCompanyItem companyItem) {
                    return companyItem.State is CompanyItemState.Company;
                }
            } else if (this.CardState is AISTATE) {
                if (this.AICompanySelector.SelectedItem is TeamPlayerCompanyItem companyItem) {
                    return companyItem.State is CompanyItemState.Company or CompanyItemState.Generate;
                }
            } else if (this.CardState is OCCUPIEDSTATE) {
                return this.Playercompany is not "No Company Selected";
            } else {
                return true;
            }
            return false;
        }

        public void SetArmyIconIfNotHost() {
            if (this.State.StateName is OCCUPIEDSTATE) {
                this.PlayerArmyIcon = (this.Playerarmy is "german" or "west_german" ? AxisArmyItems : AlliedArmyItems).FirstOrDefault(x => x.Name == this.Playerarmy)?.Icon ?? null;
                this.RefreshVisualProperty(nameof(this.PlayerArmyIcon));
            }
        }

        private static TeamPlayerCompanyItem CompanyItemFromCompany(Company company)
            => new TeamPlayerCompanyItem(CompanyItemState.Company, company.Name, company.Army.Name, company.Strength);

        private void ArmySelector_SelectionChanged(object sender, IconComboBoxSelectedChangedEventArgs args) {
            if (args.Item.GetSource(out TeamPlayerArmyItem item) && item.Name != this.Playerarmy) {
                this.Playerarmy = item.Name;
                this.OnFactionChangedHandle?.Invoke(item);
            }
        }

        private void CompanySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => this.OnCompanyChangedHandle?.Invoke((sender == this.CompanySelector ? this.CompanySelector.SelectedItem : this.AICompanySelector.SelectedItem) as TeamPlayerCompanyItem);

        private void ContextMenu_Opened(object sender, RoutedEventArgs e) {

            // Hide if right-clicked on self
            if (this.TeamSlot.SlotOccupant?.Equals(this.m_handler.Self) ?? false) { // Should be cached so participants will use this less
                e.Handled = true;
                this.ContextMenu.Visibility = Visibility.Collapsed;
            }

            // Set AI visibility data
            Visibility aiVsibility = (this.m_handler.IsHost && this.CardState is OPENSTATE && this.TeamType is not LobbyTeamType.Observers) ? Visibility.Visible : Visibility.Collapsed;
            this.ContextMenu_AISeperator.Visibility = aiVsibility;
            this.ContextMenu_EasyAI.Visibility = aiVsibility;
            this.ContextMenu_StandardAI.Visibility = aiVsibility;
            this.ContextMenu_HardAI.Visibility = aiVsibility;
            this.ContextMenu_ExpertAI.Visibility = aiVsibility;

            // Set lock/unlock
            this.ContextMenu_LockUnlock.Visibility = (this.CardState is OCCUPIEDSTATE or SELFSTATE or OBSERVERSTATE or AISTATE) ? Visibility.Collapsed : Visibility.Visible;
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
                this.TeamSlot.SlotState = LobbyTeamSlotState.OPEN;
            } else {
                this.TrySetStateByName(LOCKEDSTATE);
                this.TeamSlot.SlotState = LobbyTeamSlotState.LOCKED;
            }
        }

        private void ContextMenu_Position_Click(object sender, RoutedEventArgs e) {



        }

        private void ContextMenu_Kick_Click(object sender, RoutedEventArgs e) {



        }

        private void ContextMenu_AddAI_Click(object sender, RoutedEventArgs e) {

            // Determine difficulty
            byte difficulty = (sender as MenuItem).Name switch {
                "ContextMenu_EasyAI" => 1,
                "ContextMenu_StandardAI" => 2,
                "ContextMenu_HardAI" => 3,
                "ContextMenu_ExpertAI" => 4,
                _ => throw new ArgumentException("Cannot determine difficulty as origin was not a context menu.", nameof(sender))
            };

            // Get AI member
            if (this.m_handler.Lobby.JoinAIPlayer((int)this.TeamType, this.m_teamSlotIndex) is LobbyAIMember aiMember) {

                // Set difficulty
                aiMember.Difficulty = difficulty;
                aiMember.SetArmy(this.IsAllies ? "soviet" : "german");

                // Request refresh of card
                this.RequestFullRefresh(this);

                // Notify lobby that
                this.NotifyLobby?.Invoke();

            } else {

                Trace.WriteLine("Failed to add AI player to lobby!", nameof(TeamPlayerCard));

            }

        }

        public void RefreshVisualProperty(string property) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private ILobbyTeam GetLobbyTeamFromType(LobbyTeamType lobbyTeamType) => lobbyTeamType switch {
            LobbyTeamType.Allies => this.m_handler.Lobby.AlliesTeam,
            LobbyTeamType.Axis => this.m_handler.Lobby.AxisTeam,
            LobbyTeamType.Observers => this.m_handler.Lobby.SpectatorTeam,
            _ => throw new Exception()
        };

    }

}
