using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Functional;
using Battlegrounds.Game;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Controls.Lobby;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views.ViewComponent {
    
    public enum PlayercardViewstate {
        Occupied,
        Open,
        Locked,
    }

    public enum CompanyItemState {
        None,
        Company,
        Generate,
    }

    public record PlayercardArmyItem(BitmapSource Icon, string DisplayName, string Name) {
        public override string ToString() => this.DisplayName;
    }

    public record PlayercardCompanyItem(CompanyItemState State, string Name, double Strength = PlayercardCompanyItem.UNDEFINED_STRENGTH) {
        public const double UNDEFINED_STRENGTH = -1.0;
        public override string ToString() => this.Name;
    }

    /// <summary>
    /// Interaction logic for PlayercardView.xaml
    /// </summary>
    public partial class PlayerCardView : LobbyControl, INotifyPropertyChanged {

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
        private string m_name;
        private bool m_isAIPlayer;
        private bool m_isAllied;
        private bool m_isHost;

        public Visibility ShowRemove 
            => (this.m_isHost && this.m_steamID == BattlegroundsInstance.LocalSteamuser.ID) ? Visibility.Collapsed : Visibility.Visible;

        public bool IsHost => this.m_isHost;

        public bool IsAI => this.m_isAIPlayer;

        public bool IsOccupied => this.m_state == PlayercardViewstate.Occupied;

        public string PlayerName => this.m_name;
        
        public string PlayerArmy => this.m_army;

        public bool IsRegistered { get; set; }

        public PlayercardCompanyItem PlayerSelectedCompanyItem 
            => this.PlayerCompanySelection.SelectedItem is not null ? (PlayercardCompanyItem)this.PlayerCompanySelection.SelectedItem : default;

        public ulong PlayerSteamID => this.m_steamID;

        public AIDifficulty Difficulty => this.m_diff;

        public event Action<PlayerCardView, string> OnPlayercardEvent;

        public event PropertyChangedEventHandler PropertyChanged;

        public PlayerCardView() {
            this.InitializeComponent();
            this.SetCardState(PlayercardViewstate.Locked);
            this.m_isAIPlayer = true;
            this.PlayerArmySelection.SetItemSource(alliedArmyItems, x => new IconComboBoxItem(x.Icon, x.DisplayName));
            this.IsRegistered = false;
        }

        private IconComboBoxItem CreateArmyItem(PlayercardArmyItem item) => new IconComboBoxItem(item.Icon, item.DisplayName) { Source = item };

        public void SetAvailableArmies(bool isAllies) => isAllies
            .Then(() => { this.PlayerArmySelection.SetItemSource(alliedArmyItems, this.CreateArmyItem); this.m_isAllied = true; })
            .Else(() => { this.PlayerArmySelection.SetItemSource(axisArmyItems, this.CreateArmyItem); this.m_isAllied = false; });
    
        public void SetPlayerData(string name, string army, PlayercardCompanyItem company) {
            this.m_diff = AIDifficulty.Human;
            this.IsRegistered = !this.m_isAIPlayer;
            this.SetPlayerName(name);
            this.SetPlayerFaction(army);
            this.SetPlayerCompany(this.m_steamID == BattlegroundsInstance.LocalSteamuser.ID, this.IsAI, army, company);
        }

        public void SetPlayerName(string name) {
            this.m_name = name;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.PlayerName)));
        }

        public void SetPlayerFaction(string army) {
            if (this.m_army?.CompareTo(army) == 0) {
                return;
            }
            this.m_army = army;
            this.PlayerArmySelection.EnableEvents = false;
            this.PlayerArmySelection.SelectedIndex = this.m_isAllied ?
                alliedArmyItems.IndexOf(x => x.Name.CompareTo(army) == 0) :
                axisArmyItems.IndexOf(x => x.Name.CompareTo(army) == 0);
            this.PlayerArmySelection.EnableEvents = true;
        }

        public void SetAIData(AIDifficulty difficulty, string army) {
            this.SetPlayerData(difficulty.GetIngameDisplayName(), army, null);
            this.m_diff = difficulty;
        }

        public void SetCardState(PlayercardViewstate viewstate) {
            this.m_state = viewstate;
            switch (this.m_state) {
                case PlayercardViewstate.Locked:
                    this.State = this.m_lockedCardState;
                    break;
                case PlayercardViewstate.Occupied:
                    this.State = this.m_occupiedCardState;
                    break;
                case PlayercardViewstate.Open:
                    this.State = this.m_freeCardState;
                    break;
            }
            if (this.State is PlayerCardState cardState) {
                cardState.SetStateIdentifier(this.m_steamID, this.m_isAIPlayer, this.m_isHost);
            } else {
                this.State.SetStateIdentifier(this.m_steamID, this.m_isAIPlayer);
            }
        }

        public void SetPlayerCompany(bool isClient, bool allowAutoGen, string army, PlayercardCompanyItem company) {
            if (this.m_army.CompareTo(army) == 0 && company is not null && this.PlayerCompanySelection.SelectedItem as PlayercardCompanyItem == company) {
                return; // No reason to update
            }
            if (!this.PlayerCompanySelection.HasItemsSource && (isClient || (this.m_isHost && this.m_isAIPlayer))) {
                this.LoadSelfPlayerCompanies(army, allowAutoGen);
            } else {
                if (company is null) {
                    return;
                }
                if (company.State == CompanyItemState.Company || company.State == CompanyItemState.Generate) {
                    this.PlayerCompanySelection.EnableEvents = false;
                    this.PlayerCompanySelection.SelectedItem = company;
                    this.PlayerCompanySelection.EnableEvents = true;
                }
            }
        }

        private void LoadSelfPlayerCompanies(string army, bool allowAutogen) {
            
            var availableCompanies = PlayerCompanies.FindAll(x => x.Army.Name.CompareTo(army) == 0)
                .Select(x => new PlayercardCompanyItem(CompanyItemState.Company, x.Name, x.GetStrength()))
                .ToList();

            if (allowAutogen) {
                availableCompanies.Add(new PlayercardCompanyItem(CompanyItemState.Generate, "Generate Randomly"));
            }

            // Disable events while updating
            this.PlayerCompanySelection.EnableEvents = false;

            // Set proper item source
            if (availableCompanies.Count == 0) {
                this.PlayerCompanySelection.ItemsSource = new List<PlayercardCompanyItem>() { new PlayercardCompanyItem(CompanyItemState.None, "No Company Available") };
            } else {
                this.PlayerCompanySelection.ItemsSource = availableCompanies;
            }

            // Re-enable events
            this.PlayerCompanySelection.EnableEvents = true;

            // Set selected index
            this.PlayerCompanySelection.SelectedIndex = 0;

        }

        public void UpdatePlayerID(ulong aiid) => this.m_steamID = aiid;

        public override void SetStateBasedOnContext(bool isHost, bool isAI, ulong selfID) {
            base.SetStateBasedOnContext(isHost, isAI, selfID);
            this.m_isHost = isHost;
            this.m_isAIPlayer = isAI;
            this.m_steamID = selfID;
            this.PlayerCompanySelection.SetStateBasedOnContext(isHost, isAI, selfID);
            this.PlayerArmySelection.SetStateBasedOnContext(isHost, isAI, selfID);
            this.LockSlotButton.SetStateBasedOnContext(isHost, isAI, selfID);
            this.UnlockSlotButton.SetStateBasedOnContext(isHost, isAI, selfID);
            this.AddAIButton.SetStateBasedOnContext(isHost, isAI, selfID);
            this.RemovePlayerButton.SetStateBasedOnContext(isHost, isAI, selfID);
        }

        private void LockSlotButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "LockSlot");

        private void UnlockSlotButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "UnlockSlot");

        private void AddAIButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "AddAI");

        private void MoveHereButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "MoveTo");

        private void RemovePlayerButton_Click(object sender, RoutedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "RemovePlayer");

        private void PlayerCompanySelection_SelectionChanged(object sender, SelectionChangedEventArgs e) => this.OnPlayercardEvent?.Invoke(this, "ChangedCompany");

        private void PlayerArmySelection_SelectionChanged(object sender, IconComboBoxItem newItem) {
            if (newItem is not null && newItem.GetSource(out PlayercardArmyItem item)) {
                this.SetPlayerFaction(item.Name);
                this.SetPlayerCompany(this.m_steamID == BattlegroundsInstance.LocalSteamuser.ID, this.m_isAIPlayer, item.Name, null);
                this.OnPlayercardEvent?.Invoke(this, "ChangedArmy");
            }
        }

    }

}
