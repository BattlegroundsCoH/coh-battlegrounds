using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Battlegrounds;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using BattlegroundsApp.Controls;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views.ViewComponent {
    
    public enum PlayercardViewstate {
        Occupied,
        Open,
        Locked,
    }

    public struct PlayercardArmyItem {
        public BitmapSource Icon { get; }
        public string DisplayName { get; }
        public string Name { get; }
        public PlayercardArmyItem(BitmapSource ico, string dsp, string name) {
            this.Icon = ico;
            this.DisplayName = dsp;
            this.Name = name;
        }
    }

    public struct PlayercardCompanyItem {
        public enum CompanyItemState {
            None,
            Company,
            Generate,
        }
        public CompanyItemState State { get; }
        public string Name { get; }
        public PlayercardCompanyItem(CompanyItemState state, string text) {
            this.State = state;
            this.Name = text;
        }
        public override string ToString() => this.Name;
    }

    /// <summary>
    /// Interaction logic for PlayercardView.xaml
    /// </summary>
    public partial class PlayercardView : UserControl {

        static PlayercardArmyItem[] alliedArmyItems = new PlayercardArmyItem[] {
            new PlayercardArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/aef.png")), "US Forces", "aef"),
            new PlayercardArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/british.png")), "UK Forces", "british"),
            new PlayercardArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/soviet.png")), "Soviet Union", "soviet"),
        };

        static PlayercardArmyItem[] axisArmyItems = new PlayercardArmyItem[] {
            new PlayercardArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/german.png")), "Wehrmacht", "german"),
            new PlayercardArmyItem(new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/west_german.png")), "Oberkommando West", "west_german"),
        };

        private PlayercardViewstate m_state;
        private AIDifficulty m_diff;
        private ulong m_steamID;
        private string m_army;
        private bool m_isAIPlayer;
        private bool m_isAllied;
        private bool m_isHost;

        public bool IsAI => this.m_isAIPlayer;

        public bool IsOccupied => this.m_state == PlayercardViewstate.Occupied;

        public string Playername => PlayerName.Content as string;

        public string Playerarmy => this.m_army;

        public string Playercompany => this.SelfCompanySelector.Text;

        public bool IsRegistered { get; set; }

        public PlayercardCompanyItem PlayerSelectedCompanyItem 
            => this.SelfCompanySelector.SelectedItem is not null ? (PlayercardCompanyItem)this.SelfCompanySelector.SelectedItem : default;

        public ulong Playerid => this.m_steamID;

        public AIDifficulty Difficulty => this.m_diff;

        public event Action<PlayercardView, string> OnPlayercardEvent;

        /// <summary>
        /// Is this instance of the <see cref="PlayercardView"/> considered to be the local client.
        /// </summary>
        public bool IsClient => this.m_steamID == BattlegroundsInstance.LocalSteamuser.ID;

        public PlayercardView() {
            InitializeComponent();
            this.m_state = PlayercardViewstate.Locked;
            this.m_isAIPlayer = true;
            this.PlayerArmySelection.SetItemSource(alliedArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName));
            this.IsRegistered = false;
        }

        private IconComboBoxItem CreateArmyItem(PlayercardArmyItem item) => new IconComboBoxItem(item.Icon, item.DisplayName) { Source = item };

        public void SetAvailableArmies(bool isAllies) => isAllies
            .Then(() => { this.PlayerArmySelection.SetItemSource(alliedArmyItems, this.CreateArmyItem); this.m_isAllied = true; })
            .Else(() => { this.PlayerArmySelection.SetItemSource(axisArmyItems, this.CreateArmyItem); this.m_isAllied = false; });

        public void SetPlayerdata(ulong id, string name, string army, bool isClient, bool isAIPlayer = false, bool isHost = false) {
            this.m_steamID = id;
            this.m_army = army;
            this.m_isAIPlayer = isAIPlayer;
            this.m_diff = AIDifficulty.Human;
            this.m_isHost = isHost;
            this.IsRegistered = !this.m_isAIPlayer;
            if (!string.IsNullOrEmpty(name)) {
                this.PlayerName.Content = name;
                if (isClient || (isAIPlayer && isHost)) {
                    IsSelfPanel.Visibility = Visibility.Visible;
                    IsNotSelfPanel.Visibility = Visibility.Collapsed;
                    this.PlayerArmySelection.SelectedIndex = this.m_isAllied ? 
                        alliedArmyItems.IndexOf(x => x.Name.CompareTo(army) == 0) : 
                        axisArmyItems.IndexOf(x => x.Name.CompareTo(army) == 0);
                    this.LoadSelfPlayerCompanies(army, isAIPlayer);
                } else {
                    IsNotSelfPanel.Visibility = Visibility.Visible;
                    IsSelfPanel.Visibility = Visibility.Collapsed;
                }
                this.SetCardState(PlayercardViewstate.Occupied);
                this.PlayerKickButton.Visibility = (isHost && id != BattlegroundsInstance.LocalSteamuser.ID) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void SetAIData(AIDifficulty difficulty, string army, bool isHost = true) {
            this.SetPlayerdata(0, difficulty.GetIngameDisplayName(), army, false, true, isHost);
            this.m_diff = difficulty;
        }

        public void SetCardState(PlayercardViewstate viewstate) {
            this.m_state = viewstate;
            switch (this.m_state) {
                case PlayercardViewstate.Occupied:
                    this.OpenDockState.Visibility = Visibility.Collapsed;
                    this.OccupiedStackState.Visibility = Visibility.Visible;
                    break;
                case PlayercardViewstate.Locked:
                    this.OpenDockState.Visibility = Visibility.Collapsed;
                    this.OccupiedStackState.Visibility = Visibility.Collapsed;
                    break;
                case PlayercardViewstate.Open:
                    this.OpenDockState.Visibility = Visibility.Visible;
                    this.OccupiedStackState.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void NewAIPlayerButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "AddAI");

        private void LoadSelfPlayerCompanies(string army, bool allowAutogen) {

            var availableCompanies = PlayerCompanies.FindAll(x => x.Army.Name.CompareTo(army) == 0)
                .Select(x => new PlayercardCompanyItem(PlayercardCompanyItem.CompanyItemState.Company, x.Name))
                .ToList();

            if (allowAutogen) {
                availableCompanies.Add(new PlayercardCompanyItem(PlayercardCompanyItem.CompanyItemState.Generate, "Generate Randomly"));
            }

            if (availableCompanies.Count == 0) {
                this.SelfCompanySelector.ItemsSource = new List<PlayercardCompanyItem>() { new PlayercardCompanyItem(PlayercardCompanyItem.CompanyItemState.None, "No Company Available") };
            } else {
                this.SelfCompanySelector.ItemsSource = availableCompanies;
            }

            this.SelfCompanySelector.SelectedIndex = 0;

        }

        private void SelfCompanySelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "ChangedCompany");

        private void PlayerKickButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "RemovePlayer");

        public void PlayerArmySelection_SelectionChanged() {
            if (this.PlayerArmySelection.SelectedItem.GetSource(out PlayercardArmyItem item)) {
                this.m_army = item.Name;
                if (this.IsClient || (this.m_isHost && this.m_isAIPlayer)) {
                    this.LoadSelfPlayerCompanies(item.Name, this.m_isAIPlayer);
                }
                this.OnPlayercardEvent?.Invoke(this, "ChangedArmy");
            }
        }

        public void UpdatePlayerID(ulong aiid) => this.m_steamID = aiid;

    }

}
