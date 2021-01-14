using System;
using System.Windows;

using Battlegrounds;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Steam;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp {
    
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private void App_Startup(object sender, StartupEventArgs e) {

            // Load BG .dll instance*
            BattlegroundsInstance.LoadInstance();
            BattlegroundsInstance.LocalSteamuser = SteamUser.FromLocalInstall(); // TODO: Properly read the steam user (with permission and all that)

            // Load databases (async)
            DatabaseManager.LoadAllDatabases(OnDatabasesLoaded); // Important this is done (TODO: Add a callback handler)

            MainWindow = new MainWindow();
            MainWindow.Show();
            MainWindow.Closed += this.MainWindow_Closed;

        }

        private void MainWindow_Closed(object sender, EventArgs e) {

            // Save all changes
            BattlegroundsInstance.SaveInstance();

            // Exit
            Environment.Exit(0);

        }

        private void OnDatabasesLoaded(int loaded, int failed) {

            if (failed > 0) {
                // TODO: handle
            }

            // Load all companies used by the player
            PlayerCompanies.LoadAll();

        }

    }

}
