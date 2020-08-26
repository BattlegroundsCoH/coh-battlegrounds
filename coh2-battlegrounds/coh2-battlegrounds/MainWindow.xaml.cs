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
using BattlegroundsApp.Views;
using System.Windows.Threading;
using Battlegrounds.Game.Battlegrounds;

namespace BattlegroundsApp {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        public MainWindow() {

            // Initialize components etc...
            InitializeComponent();

            // Starts with Dashboard page opened
            DataContext = new DashboardView();
        }

        // Open Dashboard page
        private void Dashboard_Click(object sender, RoutedEventArgs e) {
            DataContext = new DashboardView();
        }

        // Open News page
        private void News_Click(object sender, RoutedEventArgs e) {
            DataContext = new NewsView();
        }
    
        // Open Division Builder page
        private void CompanyBuilder_Click(object sender, RoutedEventArgs e) {
            DataContext = new CompanyBuilderView();
        }

        // Open Campaign page
        private void Campaign_Click(object sender, RoutedEventArgs e) {
            DataContext = new CampaignView();
        }

        // Open Game Browser page
        private void GameBrowser_Click(object sender, RoutedEventArgs e) {
            DataContext = new GameBrowserView(this);
        }

        // Exit application
        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        // Helper method to update the view
        public Dispatcher SetView(object view) {
            this.Dispatcher.Invoke(() => {
                this.DataContext = view;
                this.InvalidateVisual();
            });
            return this.Dispatcher;
        }

    }   
}
