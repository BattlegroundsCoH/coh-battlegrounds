using System;
using System.Collections.Generic;
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

namespace BattlegroundsApp.Views.ViewComponent {
    
    /// <summary>
    /// Interaction logic for PlayercardView.xaml
    /// </summary>
    public partial class PlayercardView : UserControl {

        static Dictionary<string, Uri> armyIconPaths = new Dictionary<string, Uri>() {
            ["aef"] = new Uri("pack://application:,,,/Resources/ingame/aef.png"),
            ["british"] = new Uri("pack://application:,,,/Resources/ingame/british.png"),
            ["soviet"] = new Uri("pack://application:,,,/Resources/ingame/soviet.png"),
            ["german"] = new Uri("pack://application:,,,/Resources/ingame/german.png"),
            ["west_german"] = new Uri("pack://application:,,,/Resources/ingame/west_german.png"),
        };

        public PlayercardView() {
            InitializeComponent();
        }

        public void SetPlayerdata(string name, string army, bool isClient) {
            this.PlayerName.Content = name;
            if (armyIconPaths.ContainsKey(army)) {
                this.PlayerArmyIcon.Source = new BitmapImage(armyIconPaths[army]);
            }
            if (isClient) {
                IsSelfPanel.Visibility = Visibility.Visible;
                IsNotSelfPanel.Visibility = Visibility.Collapsed;
            } else {
                IsNotSelfPanel.Visibility = Visibility.Visible;
                IsSelfPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void PlayerArmyIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

        }

    }

}
