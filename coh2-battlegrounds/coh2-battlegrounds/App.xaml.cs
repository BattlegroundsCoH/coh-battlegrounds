using System;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.Collections;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds;
using Battlegrounds.Networking;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.ErrorHandling;
using Battlegrounds.Verification;
using Battlegrounds.Functional;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.Resources;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.MVVM.Models;
using BattlegroundsApp.CompanyEditor.MVVM.Models;
using BattlegroundsApp.Dashboard.MVVM.Models;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Startup.MVVM.Models;

namespace BattlegroundsApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    private static AppViewManager? __viewManager;
    private static ResourceHandler? __handler;
    private static Logger? __logger;

    private static LeftMenu? __lmenu;
    private static DashboardViewModel? __dashboard;
    private static LobbyBrowserViewModel? __lobbyBrowser;
    private static CompanyBrowserViewModel? __companyBrowser;

    public static ResourceHandler ResourceHandler 
        => IsStarted ? __handler : throw new InvalidOperationException("Cannot get resource handler before application window has initialised.");

    public static AppViewManager ViewManager 
        => IsStarted ? __viewManager : throw new InvalidOperationException("Cannot get view manager before application window has initialised.");

    [MemberNotNullWhen(true, nameof(__viewManager), nameof(__logger), nameof(__handler))]
    public static bool IsStarted { get; private set; }

    [MemberNotNull(nameof(__viewManager), nameof(__logger), nameof(__handler))]
    private void App_Startup(object sender, StartupEventArgs e) {

        // Check args
        if (e.Args.Length > 0) {
            if (e.Args.Any(x => x is "-report-error")) {
                ErrorReporter err = new();
                err.Show();
#pragma warning disable CS8774 // Member must have a non-null value when exiting. (We disable this in this very specific scenario!)
                return;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
            }
        }

        // Setup logger
        __logger = new();

        // Attach fatal exception logger
        AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;

        // Verify
        VerifyIntegrity();

        // Setup resource handler
        __handler = new();

        // Load BG .dll instance*
        BattlegroundsInstance.LoadInstance();

        // Load BG networking .dll instance*
        NetworkInterface.Setup();

        // Load locale
        LoadLocale();

        // Load
        if (!BattlegroundsInstance.IsFirstRun) {
            this.LoadNext();
        }

        // Create window and hook into window events
        MainWindow window = new();
        window.Ready += this.MainWindow_Ready;
        window.Closed += this.MainWindow_Closed;

        // Create initial left/right views
        __lmenu = new();
        __dashboard = new();

        // Create app view manager
        __viewManager = new(window);
        __viewManager.SetDisplay(AppDisplayState.LeftRight, __lmenu, __dashboard);

        // Create other views that are directly accessible from LHS
        __companyBrowser = __viewManager.CreateDisplayIfNotFound<CompanyBrowserViewModel>(() => new()) ?? throw new Exception("Failed to create company browser view model!");
        __lobbyBrowser = __viewManager.CreateDisplayIfNotFound<LobbyBrowserViewModel>(() => new()) ?? throw new Exception("Failed to create lobby browser view model!");

        // Set as started
        IsStarted = true;

        // Set main window and show
        this.MainWindow = window;
        this.MainWindow.Show();

        // Load filter
        Task.Run(ProfanityFilter.LoadFilter);

        // Load more low priority stuff down here

    }

    private static void VerifyIntegrity() {

        // Burn if checksum is not available
        if (!File.Exists("checksum.txt"))
            throw new FatalAppException("No checksum file found!");

        // Run async
        Task.Run(() => Integrity.CheckIntegrity(Environment.ProcessPath ?? throw new FatalAppException("Process path not found -> very fatal!")));

    }

    private void MainWindow_Ready(MainWindow window) {

        // Do first-time startup
        if (BattlegroundsInstance.IsFirstRun) {

            // Grab modal control
            if (ViewManager.GetModalControl() is not ModalControl fullModal) {
                MessageBox.Show("The application failed to launch properly and will now exit.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }

            // Create sstartup
            var startup = new StartupViewModel();
            startup.OnClose(() => {

                // Load next
                this.LoadNext();

                // Save all changes
                BattlegroundsInstance.SaveInstance();

            });

            // Show modal
            fullModal.ShowModal(startup);
            
        }

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

    private void LoadNext() {

        // Set network user
        NetworkInterface.SelfIdentifier = BattlegroundsInstance.Steam.User.ID;

        // Load databases (async)
        DatabaseManager.LoadAllDatabases(OnDatabasesLoaded);

    }

    private void MainWindow_Closed(object? sender, EventArgs e) {

        // Nothing to do, we have not even started...
        if (!IsStarted) {
            return;
        }

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

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {

        // Update
        if (sender is null)
            sender = "<<NULL>>";

        // Log exception
        Trace.WriteLine($"\n\n\n\t*** FATAL APP EXIT ***\n\nException trigger:\n{sender}\n\nException Info:\n{e.ExceptionObject}\n");

        // Try launch self in error mode
        try {
            string ppath = Environment.ProcessPath ?? "";
            if (!string.IsNullOrEmpty(ppath)) {
                ProcessStartInfo pinfo = new(ppath, "-report-error");
                if (Process.Start(pinfo) is null) {
                    Trace.WriteLine("Failed to open error reporter...");
                }
            }
        } catch {
            Trace.WriteLine("Failed to launch self in error mode!");
        }

        // Close logger with exit code
        __logger?.SaveAndClose(int.MaxValue);

    }

    /// <summary>
    /// Try find a data template from <see cref="Application"/> resources.
    /// </summary>
    /// <param name="type">The type to try and find data template for.</param>
    /// <returns>If found, the linked <see cref="DataTemplate"/> instance; Otherwise <see langword="null"/> if not found.</returns>
    public static DataTemplate? TryFindDataTemplate(Type type) => TryFindDataTemplate(Current.Resources, type);

    private static DataTemplate? TryFindDataTemplate(ResourceDictionary dictionary, Type type) {

        // Loop through basic entry
        foreach (DictionaryEntry res in dictionary) {
            if (res.Value is DataTemplate template && template.DataType.Equals(type)) {
                return template;
            }
        }

        // Loop through merged
        foreach (var subdic in dictionary.MergedDictionaries) {
            if (TryFindDataTemplate(subdic, type) is DataTemplate tmpl) {
                return tmpl;
            }
        }

        // Return nothing
        return null;

    }

}
