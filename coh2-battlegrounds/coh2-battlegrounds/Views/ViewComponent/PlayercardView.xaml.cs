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
using Battlegrounds.Game.Battlegrounds;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views.ViewComponent {
    
    public enum PlayercardViewstate {
        Occupied,
        Open,
        Locked,
    }

    /// <summary>
    /// Interaction logic for PlayercardView.xaml
    /// </summary>
    public partial class PlayercardView : UserControl {

        static Dictionary<string, Uri> armyIconPaths = new Dictionary<string, Uri>() { // TODO: Cache icons instead of simply storing their paths
            ["aef"] = new Uri("pack://application:,,,/Resources/ingame/aef.png"),
            ["british"] = new Uri("pack://application:,,,/Resources/ingame/british.png"),
            ["soviet"] = new Uri("pack://application:,,,/Resources/ingame/soviet.png"),
            ["german"] = new Uri("pack://application:,,,/Resources/ingame/german.png"),
            ["west_german"] = new Uri("pack://application:,,,/Resources/ingame/west_german.png"),
        };

        private PlayercardViewstate m_state;
        private ulong m_steamID;
        private string m_army;
        private bool m_isAIPlayer;

        public bool IsAI => this.m_isAIPlayer;

        public bool IsOccupied => this.m_state == PlayercardViewstate.Occupied;

        public string Playername => PlayerName.Content as string;

        public string Playerarmy => this.m_army;

        public string Playercompany => this.SelfCompanySelector.Text;

        public ulong Playerid => this.m_steamID;

        public PlayercardView() {
            InitializeComponent();
            this.m_state = PlayercardViewstate.Locked;
            this.m_isAIPlayer = true;
        }

        public void SetPlayerdata(ulong id, string name, string army, bool isClient, bool isAIPlayer = false, bool isHost = false) {
            this.m_steamID = id;
            this.m_army = army;
            this.m_isAIPlayer = isAIPlayer;
            if (!string.IsNullOrEmpty(name)) {
                this.PlayerName.Content = name;
                if (armyIconPaths.ContainsKey(army)) {
                    this.PlayerArmyIcon.Source = new BitmapImage(armyIconPaths[army]);
                }
                if (isClient || (isAIPlayer && isHost)) {
                    IsSelfPanel.Visibility = Visibility.Visible;
                    IsNotSelfPanel.Visibility = Visibility.Collapsed;
                    this.LoadSelfPlayerCompanies(army, isAIPlayer);
                } else {
                    IsNotSelfPanel.Visibility = Visibility.Visible;
                    IsSelfPanel.Visibility = Visibility.Collapsed;
                }
                this.SetCardState(PlayercardViewstate.Occupied);
            }
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

        private void PlayerArmyIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

        }

        private void NewAIPlayerButton_Click(object sender, RoutedEventArgs e) {

        }

        private void LoadSelfPlayerCompanies(string army, bool allowAutogen) {

            List<string> availableCompanies = PlayerCompanies.FindAll(x => x.Army.Name.CompareTo(army) == 0).Select(x => x.Name).ToList();
            if (allowAutogen) {
                availableCompanies.Add("Generate Randomly");
            }

            this.SelfCompanySelector.ItemsSource = availableCompanies;
            this.SelfCompanySelector.SelectedIndex = 0;

        }

    }

}
