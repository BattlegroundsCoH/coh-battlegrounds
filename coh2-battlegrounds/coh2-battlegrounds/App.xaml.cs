using System;
using System.Diagnostics;
using System.Windows;

using Battlegrounds;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Steam;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp {
    
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private static Logger __logger;

        private void App_Startup(object sender, StartupEventArgs e) {

            // Setup logger
            __logger = new Logger();

            // Load BG .dll instance*
            BattlegroundsInstance.LoadInstance();

            // Load databases (async)
            DatabaseManager.LoadAllDatabases(OnDatabasesLoaded); // Important this is done (TODO: Add a callback handler)

            // Create window and hook into window events
            var window = new MainWindow();
            window = new MainWindow();
            window.Ready += MainWindow_Ready;
            window.Closed += this.MainWindow_Closed;

            // Set main window and show
            this.MainWindow = window;
            this.MainWindow.Show();

        }

        private void MainWindow_Initialized(object sender, EventArgs e) => throw new NotImplementedException();

        private void MainWindow_Ready(MainWindow window) {

            // Verify we have a user
            if (!BattlegroundsInstance.Steam.HasUser) {
                GetSteamUserWithPermission(window); // We don't so try and get one
            }

        }

        private static void GetSteamUserWithPermission(MainWindow window) {
            if (window.AllowGetSteamUser()) {
                Trace.WriteLine("No steam user was found - user has given permission to get steam user.", "App");
                if (SteamInstance.IsSteamRunning()) {
                    if (BattlegroundsInstance.Steam.GetSteamUser()) {

                        // Log the found user
                        Trace.WriteLine($"Found steam user: {BattlegroundsInstance.Steam.User.ID} \"{BattlegroundsInstance.Steam.User.Name}\"", "App");
                        
                        // Save all changes
                        BattlegroundsInstance.SaveInstance();

                    } else {
                        MessageBox.Show("Unable to detect the current Steam user!", "No steam user found!", MessageBoxButton.OK, MessageBoxImage.Error);
                        Trace.WriteLine("", "App");
                        Environment.Exit(0);
                    }
                } else {
                    MessageBox.Show("Unable to find a running instance of Steam. Please start Steam and try again.", "No steam instance running!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Trace.WriteLine("", "App");
                    Environment.Exit(0);
                }
            } else {
                Environment.Exit(0);
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e) {

            // Save all changes
            BattlegroundsInstance.SaveInstance();

            // Save log
            __logger.SaveAndClose(0);

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
