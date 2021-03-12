using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using Battlegrounds.Campaigns;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Resources;
using static Battlegrounds.BattlegroundsInstance;

namespace BattlegroundsApp.Views.CampaignViews {
    
    /// <summary>
    /// Interaction logic for CampaignEngagementDialogView.xaml
    /// </summary>
    public partial class CampaignEngagementDialogView : CampaignDialogView, INotifyPropertyChanged {

        public const string AUTO = "auto";
        public const string WITHDRAW = "withdraw";
        public const string ENGAGE = "engage";

        public class CampaignEngagementRegiment {
            public Regiment Regiment { get; }
            public string Display { get; }
            public CampaignEngagementRegiment(Regiment r) {
                this.Regiment = r;
                this.Display = Localize.GetString(r.Name);
            }
            public override bool Equals(object obj) {
                if (obj is CampaignEngagementRegiment reg) {
                    return reg.Regiment == this.Regiment;
                } else {
                    return false;
                }
            }
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => this.Display;
        }

        public class CampaignEngagementRegimentalUnit {
            public Squad Squad { get; }
            public CampaignEngagementRegimentalUnit(Squad squad) {
                this.Squad = squad;
            }
            public override bool Equals(object obj) {
                if (obj is CampaignEngagementRegimentalUnit reg) {
                    return reg.Squad == this.Squad;
                } else {
                    return false;
                }
            }
            public override int GetHashCode() => base.GetHashCode();
            public override string ToString() => this.Squad.SBP.Name;
        }

        private List<Formation> m_formations;

        /*
        ***
        * Welcome to property hell
        ***
        */

        public CampaignMapNode MapNode { get; init; }

        public Scenario EngagementScenario { get; private set; }

        public ImageSource EngagementScenarioMinimap { get; private set; }

        public string EngagementScenarioTitle => GameLocale.GetString(this.EngagementScenario?.Name ?? "No valid node map!");

        public int SelectedPlayer { get; set; } = 0;

        public int SelectedRegimentUnit { get; set; }

        public int Players { get; init; } = 1;

        public string Header { get; init; } = "Attacking NAME OF NODE";

        public string Attackers { get; init; } = "Axis";

        public string Defenders { get; init; } = "Allies";

        public bool CanEngage => this.CanEngageMthd();

        public bool HasPlayer2 => this.Players >= 2;

        public bool HasPlayer3 => this.Players >= 3;

        public bool HasPlayer4 => this.Players == 4;

        public bool CanMoveAll 
            => this.RegimentalUnits.Count > 0 && this.IndexableUnits[this.SelectedPlayer].Count < Company.MAX_SIZE;

        public bool CanAddToCompany
            => this.CurrentRegiment is not null && this.IndexableUnits[this.SelectedPlayer].Count < Company.MAX_SIZE;

        public bool CanRemoveAll
            => this.IndexableUnits[this.SelectedPlayer].Count > 0;

        public bool CanRemoveFromCompany
            => this.CanRemoveFromCompanyMthd();

        public Visibility Player2Tab => HasPlayer2 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Player3Tab => HasPlayer3 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility Player4Tab => HasPlayer4 ? Visibility.Visible : Visibility.Collapsed;

        public CampaignEngagementRegimentalUnit CurrentCompanyUnitPlayer1 { get; set; }

        public CampaignEngagementRegimentalUnit CurrentCompanyUnitPlayer2 { get; set; }

        public CampaignEngagementRegimentalUnit CurrentCompanyUnitPlayer3 { get; set; }

        public CampaignEngagementRegimentalUnit CurrentCompanyUnitPlayer4 { get; set; }

        public CampaignEngagementRegiment CurrentRegiment { get; set; }

        public List<CampaignEngagementRegiment> AttackingRegimentalPool { get; }

        public ObservableCollection<CampaignEngagementRegimentalUnit> RegimentalUnits { get; }

        public ObservableCollection<CampaignEngagementRegimentalUnit> Player1Units { get; }
        public ObservableCollection<CampaignEngagementRegimentalUnit> Player2Units { get; }
        public ObservableCollection<CampaignEngagementRegimentalUnit> Player3Units { get; }
        public ObservableCollection<CampaignEngagementRegimentalUnit> Player4Units { get; }

        private ObservableCollection<CampaignEngagementRegimentalUnit>[] IndexableUnits;

        public event PropertyChangedEventHandler PropertyChanged;

        public CampaignEngagementDialogView() {

            // Create lists
            this.AttackingRegimentalPool = new List<CampaignEngagementRegiment>();
            this.RegimentalUnits = new ObservableCollection<CampaignEngagementRegimentalUnit>();
            this.Player1Units = new ObservableCollection<CampaignEngagementRegimentalUnit>();
            this.Player2Units = new ObservableCollection<CampaignEngagementRegimentalUnit>();
            this.Player3Units = new ObservableCollection<CampaignEngagementRegimentalUnit>();
            this.Player4Units = new ObservableCollection<CampaignEngagementRegimentalUnit>();
            this.m_formations = new List<Formation>();

            // Create indexable arrays
            this.IndexableUnits = new[] {
                Player1Units, Player2Units, Player3Units, Player4Units
            };

            // Init components
            InitializeComponent();

        }

        public void SetAttackingFormations(List<Formation> formations) {
            this.m_formations.AddRange(formations);
            formations.ForEach(x => {
                x.Regiments.ForEach(r => {
                    if (r.Strength > 0) {
                        this.AttackingRegimentalPool.Add(new CampaignEngagementRegiment(r));
                    }
                });
            });
        }

        protected override void OnOpened() {}

        private void Regiment_SelectionChanged(object sender, SelectionChangedEventArgs e) => this.RefreshDisplayedDivision();

        private void RefreshDisplayedDivision() {
            this.RegimentalUnits.Clear();
            if (this.CurrentRegiment.Regiment.FirstCompany() is Regiment.Company company) {
                company.Units.ForEach(x => {
                    var reg = new CampaignEngagementRegimentalUnit(x);
                    if (!Player1Units.Contains(reg) && !Player2Units.Contains(reg) && !Player3Units.Contains(reg) && !Player4Units.Contains(reg)) {
                        this.RegimentalUnits.Add(reg);
                    }
                });
            }
        }

        private void RegimentUnitList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => this.MoveThis_Click(sender, new RoutedEventArgs());

        private void RefreshProperties() {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanEngage)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanMoveAll)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddToCompany)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRemoveAll)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRemoveFromCompany)));
        }

        private void MoveAll_Click(object sender, RoutedEventArgs e) {
            int count = 0;
            while (count < this.RegimentalUnits.Count && this.IndexableUnits[this.SelectedPlayer].Count < Company.MAX_SIZE) {
                var u = this.RegimentalUnits[count];
                this.IndexableUnits[this.SelectedPlayer].Add(u);
                this.RegimentalUnits.RemoveAt(count);
                count++;
            }
            this.RefreshProperties();
        }
        
        private void MoveThis_Click(object sender, RoutedEventArgs e) {
            this.IndexableUnits[this.SelectedPlayer].Add(this.RegimentalUnits[this.SelectedRegimentUnit]);
            this.RegimentalUnits.RemoveAt(this.SelectedRegimentUnit);
            this.RefreshProperties();
        }
        
        private void ReturnAll_Click(object sender, RoutedEventArgs e) {
            this.IndexableUnits[this.SelectedPlayer].Clear();
            this.RefreshDisplayedDivision();
            this.RefreshProperties();
        }
        
        private void ReturnThis_Click(object sender, RoutedEventArgs e) {
            if (sender is ListView lv) {
                this.IndexableUnits[this.SelectedPlayer].RemoveAt(lv.SelectedIndex);
                this.RefreshDisplayedDivision();
            }
            this.RefreshProperties();
        }

        private void CompanyList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => this.ReturnThis_Click(sender, new RoutedEventArgs());

        private void Withdraw_Click(object sender, RoutedEventArgs e) => this.CloseDialog(WITHDRAW);

        private void AutoResolve_Click(object sender, RoutedEventArgs e) => this.CloseDialog(AUTO);

        private void Engage_Click(object sender, RoutedEventArgs e) => this.CloseDialog(ENGAGE);

        private bool CanEngageMthd() {
            bool p1 = Player1Units.Count > 0;
            bool p2 = !HasPlayer2 || Player2Units.Count > 0;
            bool p3 = !HasPlayer3 || Player3Units.Count > 0;
            bool p4 = !HasPlayer4 || Player4Units.Count > 0;
            return p1 && p2 && p3 && p4;
        }

        private bool CanRemoveFromCompanyMthd() => this.SelectedPlayer switch {
            0 => this.CurrentCompanyUnitPlayer1 is not null,
            1 => this.CurrentCompanyUnitPlayer2 is not null,
            2 => this.CurrentCompanyUnitPlayer3 is not null,
            3 => this.CurrentCompanyUnitPlayer4 is not null,
            _ => false
        };

        private void RegimentUnitList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddToCompany)));

        private void Company_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => this.RefreshProperties();

        public void SetupMatchData(ActiveCampaign campaign) {
            this.EngagementScenario = campaign.PickScenario(this.MapNode.Maps);
            if (this.EngagementScenario is not null) {
                string gamePath = Path.GetFullPath($"bin\\gfx\\map_icons\\{this.EngagementScenario.RelativeFilename}_map.tga");
                string userPath = Path.GetFullPath($"usr\\mods\\map_icons\\{this.EngagementScenario.RelativeFilename}_map.tga");
                this.EngagementScenarioMinimap = null;
                try {
                    if (File.Exists(gamePath)) {
                        this.EngagementScenarioMinimap = TgaImageSource.TargaBitmapSourceFromFile(gamePath);
                    } else if (File.Exists(userPath)) {
                        this.EngagementScenarioMinimap = TgaImageSource.TargaBitmapSourceFromFile(userPath);
                    }
                } catch {
                } finally {
                    if (this.EngagementScenarioMinimap is null) {
                        this.EngagementScenarioMinimap = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));
                    }
                }
            } else {
                Trace.WriteLine($"Node '{this.MapNode.NodeName}' has no valid engagement map!", nameof(CampaignEngagementDialogView));
            }
        }

    }

}
