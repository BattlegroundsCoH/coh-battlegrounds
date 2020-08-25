using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Steam;

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
            DatabaseManager.LoadAllDatabases(null); // Important this is done (TODO: Add a callback handler)

            MainWindow = new MainWindow();
            MainWindow.Show();

        }

        // TODO: OnAppClose event (To save the instance)

    }

}
