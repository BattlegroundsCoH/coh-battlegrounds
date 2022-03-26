using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds;
using Battlegrounds.Networking;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Steam;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.Resources;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.MVVM.Models;
using BattlegroundsApp.CompanyEditor.MVVM.Models;
using System.Threading.Tasks;
using Battlegrounds.ErrorHandling;

namespace BattlegroundsApp {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private static ulong __integrityHash;

        private static AppViewManager? __viewManager;
        private static ResourceHandler? __handler;
        private static Logger? __logger;

        private static LeftMenu? __lmenu;
        private static LobbyBrowserViewModel? __lobbyBrowser;
        private static CompanyBrowserViewModel? __companyBrowser;

        public static ulong IntegrityHash => __integrityHash;

        public static string IntegrityHashString => $"0x{IntegrityHash:X}";

        [NotNull] // "never" null; and invalid operation is throw if it is...
        public static ResourceHandler ResourceHandler 
            => IsStarted ? __handler : throw new InvalidOperationException("Cannot get resource handler before application window has initialised.");

        [NotNull] // "never" null; and invalid operation is throw if it is...
        public static AppViewManager ViewManager 
            => IsStarted ? __viewManager : throw new InvalidOperationException("Cannot get view manager before application window has initialised.");

        [MemberNotNullWhen(true, nameof(__viewManager), nameof(__logger), nameof(__handler))]
        public static bool IsStarted { get; private set; }

        [MemberNotNull(nameof(__viewManager), nameof(__logger), nameof(__handler))]
        private void App_Startup(object sender, StartupEventArgs e) {

            // Setup logger
            __logger = new();

            // Verify
            this.VerifyIntegrity();

            // Setup resource handler
            __handler = new();

            // Load BG .dll instance*
            BattlegroundsInstance.LoadInstance();

            // Load BG networking .dll instance*
            NetworkInterface.Setup();

            // Load locale
            LoadLocale();

            // Load databases (async)
            DatabaseManager.LoadAllDatabases(OnDatabasesLoaded);

            // Create window and hook into window events
            MainWindow window = new();
            window.Ready += this.MainWindow_Ready;
            window.Closed += this.MainWindow_Closed;

            // Create initial left/right views
            __lmenu = new();
            __lobbyBrowser = new();

            // Create app view manager
            __viewManager = new(window);
            __viewManager.SetDisplay(AppDisplayState.LeftRight, __lmenu, __lobbyBrowser); // TODO: Replace browser with dashboard when dashboard is implemented

            // Create other views that are directly accessible from LHS
            __companyBrowser = __viewManager.CreateDisplayIfNotFound<CompanyBrowserViewModel>(() => new()) ?? throw new Exception("Failed to create company browser view model!");
          
            // Set as started
            IsStarted = true;

            // Set main window and show
            this.MainWindow = window;
            this.MainWindow.Show();


        }

        private void VerifyIntegrity() {

            // Burn if checksum is not available
            if (!File.Exists("checksum.txt"))
                throw new FatalAppException();

            Task.Run(() => {

                // Grab self path
                string selfPath = Environment.ProcessPath ?? throw new FatalAppException();
                string dllPath = Environment.ProcessPath.Replace(".exe", "-bin.dll");
                string netPath = Environment.ProcessPath.Replace(".exe", "-networking.dll");

                // If dll is not found -> bail
                if (!File.Exists(dllPath))
                    throw new FatalAppException();

                // If net dll not found -> bail
                if (!File.Exists(netPath))
                    throw new FatalAppException();

                // Check self
                var selfCheck = ComputeChecksum(selfPath);
                var binCheck = ComputeChecksum(dllPath) + selfCheck;
                var netCheck = ComputeChecksum(netPath) + binCheck;

                // Check common
                __integrityHash = ComputeDirectoryChecksum(netCheck, "bg_common");

                // Check validity
                if (__integrityHash != Convert.ToUInt64(File.ReadAllText("checksum.txt"), 16)) {
#if DEBUG
                    Trace.WriteLine($"<DEBUG> Integrity check failed - UPDATE CHECKSUM.TXT (Checksum = {IntegrityHashString})", nameof(App));
#else
                    throw new FatalAppException();
#endif
                } else {

                    // Log success
                    Trace.WriteLine("Verified integrity of core files.", nameof(App));

                }

            });

        }

        private ulong ComputeDirectoryChecksum(ulong check, string dir) {

            if (dir.EndsWith("map_icons"))
                return 0;

            string[] files = Directory.GetFiles(dir);
            ulong fileSum = 0;
            foreach (var f in files)
                fileSum += this.ComputeChecksum(f);

            ulong dsum = 0;
            string[] dirs = Directory.GetDirectories(dir);
            foreach (var d in dirs)
                dsum += ComputeDirectoryChecksum(0, d);

            return check + dsum + fileSum;

        }

        private ulong ComputeChecksum(string filepath) {

            ulong check = 0;
            using var fs = File.OpenRead(filepath);
            while (fs.Position < fs.Length) {
                check += (ulong)fs.ReadByte();
            }

            return check;

        }

        private void MainWindow_Ready(MainWindow window) {

            // Verify we have a user
            if (!BattlegroundsInstance.Steam.HasUser) {
                GetSteamUserWithPermission(window); // We don't so try and get one
            }

            // Set network user
            NetworkInterface.SelfIdentifier = BattlegroundsInstance.Steam.User.ID;

            // Trigger discord setup
            this.SetupDiscord();

        }

        private void SetupDiscord() {

            try {

                // Create discord instance

                // Trace.WriteLine($"Successfully initialised Discord connection with user: {DiscordInstance.GetUserManager().GetCurrentUser().Id}", "DiscordAPI");

            } catch (Exception dex) {

                Trace.WriteLine($"Failed to initialise discord API: {dex}", "DiscordAPI");

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
                        Trace.WriteLine("Unable to detect the current Steam user.", "App");
                        Environment.Exit(0);
                    }
                } else {
                    MessageBox.Show("Unable to find a running instance of Steam. Please start Steam and try again.", "No steam instance running!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Trace.WriteLine("Unable to find a running instance of Steam.", "App");
                    Environment.Exit(0);
                }
            } else {
                Environment.Exit(0);
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e) {

            // Nothing to do, we have not even started...
            if (!IsStarted) {
                return;
            }

            // TODO: Close active view (in case it's in modifier state, like the company builder)

            // Save all changes
            BattlegroundsInstance.SaveInstance();

            // Close networking
            NetworkInterface.Shutdown();

            // Save log
            __logger.SaveAndClose(0);

            // Set started flag
            IsStarted = false;

            // Exit
            Environment.Exit(0);

        }

        private static void LoadLocale() {

            string lang = BattlegroundsInstance.Localize.Language.ToString().ToLower(CultureInfo.InvariantCulture);
            if (lang == "default") {
                lang = "english";
            }

            string filepath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, $"locale\\{lang}.loc");
            if (File.Exists(filepath)) {
                _ = BattlegroundsInstance.Localize.LoadLocaleFile(filepath);
            } else {
                Trace.WriteLine($"Failed to locate locale file: {filepath}", "AppStartup");
            }

        }

        private void OnDatabasesLoaded(int loaded, int failed) {

            if (failed > 0) {
                // TODO: handle
                Trace.WriteLine($"Failed to load {failed} databases!", nameof(App));
            }

            // Load all companies used by the player
            PlayerCompanies.LoadAll();

            // Load all installed and active campaigns
            PlayerCampaigns.GetInstalledCampaigns();
            PlayerCampaigns.LoadActiveCampaigns();

        }

        /// <summary>
        /// Try find a data template from <see cref="Application"/> resources.
        /// </summary>
        /// <param name="type">The type to try and find data template for.</param>
        /// <returns>If found, the linked <see cref="DataTemplate"/> instance; Otherwise <see langword="null"/> if not found.</returns>
        public static DataTemplate? TryFindDataTemplate(Type type) {

            foreach (DictionaryEntry res in Current.Resources) {
                if (res.Value is DataTemplate template && template.DataType.Equals(type)) {
                    return template;
                }
            }

            return null;

        }

    }

}
