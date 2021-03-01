using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

using static Battlegrounds.BattlegroundsInstance;

namespace BattlegroundsApp.Views.CampaignViews {
    
    /// <summary>
    /// Interaction logic for CampaignEngagementDialogView.xaml
    /// </summary>
    public partial class CampaignEngagementDialogView : CampaignDialogView, INotifyPropertyChanged {

        public class CampaignEngagementRegiment {
            public Regiment Regiment { get; }
            public string Display { get; }
            public CampaignEngagementRegiment(Regiment r) {
                this.Regiment = r;
                this.Display = Localize.GetString(r.Name);
            }
            public override string ToString() => this.Display;
        }

        public class CampaignEngagementRegimentalUnit {
            public Squad Squad { get; }
            public CampaignEngagementRegimentalUnit(Squad squad) {
                this.Squad = squad;
            }
            public override string ToString() => this.Squad.SBP.Name;
        }

        private List<Formation> m_formations;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Header { get; init; } = "Attacking NAME OF NODE";

        public string Attackers { get; init; } = "Axis";

        public string Defenders { get; init; } = "Allies";

        public CampaignEngagementRegiment CurrentRegiment { get; set; }

        public List<CampaignEngagementRegiment> AttackingRegimentalPool { get; }

        public ObservableCollection<CampaignEngagementRegimentalUnit> RegimentalUnits { get; }

        public CampaignEngagementDialogView() {

            // Create lists
            this.AttackingRegimentalPool = new List<CampaignEngagementRegiment>();
            this.RegimentalUnits = new ObservableCollection<CampaignEngagementRegimentalUnit>();
            this.m_formations = new List<Formation>();

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

        protected override void OnOpened() {
        }

        private void Regiment_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.RegimentalUnits.Clear();
            if (this.CurrentRegiment.Regiment.FirstCompany() is Regiment.Company company) {
                company.Units.ForEach(x => this.RegimentalUnits.Add(new CampaignEngagementRegimentalUnit(x)));
            }
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegimentalUnits)));
        }

        private void MoveAll_Click(object sender, RoutedEventArgs e) {

        }
        
        private void MoveThis_Click(object sender, RoutedEventArgs e) {

        }
        
        private void ReturnAll_Click(object sender, RoutedEventArgs e) {

        }
        
        private void ReturnThis_Click(object sender, RoutedEventArgs e) {

        }

        private void Withdraw_Click(object sender, RoutedEventArgs e) {

        }

        private void AutoResolve_Click(object sender, RoutedEventArgs e) {

        }

        private void Engage_Click(object sender, RoutedEventArgs e) {

        }

    }

}
