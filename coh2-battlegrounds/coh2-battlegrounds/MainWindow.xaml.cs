using Battlegrounds;
using Battlegrounds.Online.Services;
using Battlegrounds.Steam;
using Battlegrounds.Game.Database;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Diagnostics;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        public MainWindow() {
            InitializeComponent();

            // Starts with Dashboard page opened
            DataContext = new DashboardViewModel();
        }

        // Open Dashboard page
        private void Dashboard_Click(object sender, RoutedEventArgs e) {
            DataContext = new DashboardViewModel();
        }

        // Open News page
        private void News_Click(object sender, RoutedEventArgs e) {
            DataContext = new NewsViewModel();
        }
    
        // Open Division Builder page
        private void DivisionBuilder_Click(object sender, RoutedEventArgs e) {
            DataContext = new DivisionBuilderViewModel();
        }

        // Open Campaign page
        private void Campaign_Click(object sender, RoutedEventArgs e) {
            DataContext = new CampaignViewModel();
        }

        // Open Game Browser page
        private void GameBrowser_Click(object sender, RoutedEventArgs e) {
            DataContext = new GameBrowserViewModel();
        }

        // Exit application
        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }   
}
