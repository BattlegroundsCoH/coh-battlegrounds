using System;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.Collections;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds;
using Battlegrounds.UI;
using Battlegrounds.UI.Modals;
using Battlegrounds.UI.Modals.Prompts;
using Battlegrounds.UI.Application;
using Battlegrounds.UI.Application.Pages;
using Battlegrounds.UI.Application.Modals;
using Battlegrounds.UI.Application.Components;
using Battlegrounds.Networking;
using Battlegrounds.Verification;
using Battlegrounds.Update;
using Battlegrounds.Functional;
using Battlegrounds.Util.Coroutines;
using Battlegrounds.Resources;
using Battlegrounds.DataLocal;
using Battlegrounds.DataLocal.Generator;
using Battlegrounds.Editor;
using Battlegrounds.Editor.Pages;
using Battlegrounds.Lobby.Pages;
using Battlegrounds.Lobby;
using Battlegrounds.Logging;

namespace BattlegroundsApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IResourceResolver, IViewController {

    private static readonly Logger logger = Logger.CreateLogger();

    private static AppViewManager? __viewManager;

    private static IUIModule? __editorModule;
    private static IUIModule? __lobbyModule;
    private static IUIModule? __appModule;

    private static LeftMenu? __lmenu;
    private static Settings? __settings;
    private static Dashboard? __dashboard;
    private static LobbyBrowser? __lobbyBrowser;
    private static CompanyBrowser? __companyBrowser;

    public static AppViewManager Views 
        => IsStarted ? __viewManager : throw new InvalidOperationException("Cannot get view manager before application window has initialised.");

    public AppViewManager ViewManager => __viewManager ?? throw new InvalidOperationException("Cannot get view manager before application window has initialised.");

    [MemberNotNullWhen(true, nameof(__viewManager))]
    public static bool IsStarted { get; private set; }

    public static new Dispatcher Dispatcher => App.Current.Dispatcher;

    [MemberNotNull(nameof(__viewManager))]
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

        // Attach fatal exception logger
        AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;

        // Verify
        VerifyIntegrity();

        // Load BG .dll instance*
        BattlegroundsContext.LoadInstance();

        // Setup resource handler
        LoadResources();

        // Load BG networking .dll instance*
        NetworkInterface.Setup();

        // Load locale
        LoadLocale();

        // Create window and hook into window events
        MainWindow window = new();
        window.Ready += this.MainWindow_Ready;
        window.Closed += this.MainWindow_Closed;

        // Create initial left/right views
        __dashboard = new();

        // Create app view manager
        __viewManager = new(window);

        // Create left menu
        __lmenu = new(__viewManager);
        
        // Create lobby module and register it
        __lobbyModule = new LobbyModule();
        __lobbyModule.RegisterMenuCallbacks(__lmenu);
        __lobbyModule.RegisterViewFactories(__viewManager);

        // Create editor module and register it
        __editorModule = new EditorModule();
        __editorModule.RegisterMenuCallbacks(__lmenu);
        __editorModule.RegisterViewFactories(__viewManager);

        // Create app module and register it
        __appModule = new ApplicationModule();
        __appModule.RegisterMenuCallbacks(__lmenu);
        __appModule.RegisterViewFactories(__viewManager);

        // Set view manager
        __viewManager.SetDisplay(AppDisplayState.LeftRight, __lmenu, __dashboard);

        // Set as started
        IsStarted = true;

        // Set the application version
        BattlegroundsContext.Version = new AppVersionFetcher();

        // Load
        if (!BattlegroundsContext.IsFirstRun) {
            this.LoadNext();
        }

        // Set main window and show
        this.MainWindow = window;
        this.MainWindow.Show();

        // Load filter
        //Task.Run(ProfanityFilter.LoadFilter);

        // Load more low priority stuff down here

        var updateFolderContents = Directory.GetFiles(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.UPDATE_FOLDER));
        // Remove update folder
        if (updateFolderContents.Length > 0) {
            updateFolderContents.ForEach(File.Delete);
        }

        // Check for new updates
        Task.Run(CheckForUpdate);

    }

    private static void LoadResources() {

        // Load Resources
        //ResourceHandler.LoadAllResources(null);
        ResourceHandler.LoadAllResources(Assembly.GetExecutingAssembly());

        // Load additional resources
        ResourceLoader.LoadAllPackagedResources();

    }

    private static void VerifyIntegrity() {

        // Burn if checksum is not available
        if (!File.Exists("checksum.txt"))
            throw logger.Fatal("No checksum file found!");

        // Run async
        Task.Run(() => Integrity.CheckIntegrity(Environment.ProcessPath ?? throw logger.Fatal("Process path not found -> very fatal!")));

    }

    private void MainWindow_Ready(MainWindow window) {

        // Do first-time startup
        if (BattlegroundsContext.IsFirstRun) {

            // Grab modal control
            if (Views.GetModalControl() is not ModalControl fullModal) {
                MessageBox.Show("The application failed to launch properly and will now exit.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }

            // Create sstartup
            var startup = new Startup();
            startup.OnClose(() => {

                // Load next
                this.LoadNext();

                // Save all changes
                BattlegroundsContext.SaveInstance();

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

            logger.Error($"Failed to initialise discord API: {dex}");

        }

    }

    private void LoadNext() {

        // Set network user
        NetworkInterface.SelfIdentifier = BattlegroundsContext.Steam.User.ID;

        // Load databases (async)
        BattlegroundsContext.DataSource.LoadDatabases(OnDatabasesLoaded);

        // Verify view manager
        if (!IsStarted) {
            return;
        }

        __dashboard!.UpdateSteamUser();

        // Create other views that are directly accessible from LHS
        __companyBrowser = __viewManager.CreateDisplayIfNotFound<CompanyBrowser>(() => new()) ?? throw new Exception("Failed to create company browser view model!");
        __lobbyBrowser = __viewManager.CreateDisplayIfNotFound<LobbyBrowser>(() => new()) ?? throw new Exception("Failed to create lobby browser view model!");
        __settings = __viewManager.CreateDisplayIfNotFound<Settings>(() => new()) ?? throw new Exception("Failed to create settings view model!");

    }

    private void MainWindow_Closed(object? sender, EventArgs e) {

        // Nothing to do, we have not even started...
        if (!IsStarted) {
            return;
        }

        // Close networking
        NetworkInterface.Shutdown();

        // Close log
        BattlegroundsContext.Log?.SaveAndClose(0);

        // Save all changes
        BattlegroundsContext.SaveInstance();

        // Set started flag
        IsStarted = false;

        // Exit
        Environment.Exit(0);

    }

    private static void LoadLocale() {

        string lang = BattlegroundsContext.Localize.Language.ToString().ToLower(CultureInfo.InvariantCulture);
        if (lang == "default") {
            lang = "english";
        }

        string filepath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, $"locale\\{lang}.loc");
        if (File.Exists(filepath)) {
            _ = BattlegroundsContext.Localize.LoadLocaleFile(filepath);
        } else {
            logger.Error($"Failed to locate locale file: {filepath}");
        }

    }

    private void OnDatabasesLoaded(int loaded, int failed) {

        if (failed > 0) {
            // TODO: handle
            logger.Error($"Failed to load {failed} databases!");
        }

        // Load all companies used by the player
        Companies.LoadAll();

        // If initial, create companies
        if (BattlegroundsContext.IsFirstRun || Companies.GetAllCompanies().Count is 0) {
            InitialCompanyCreator.Init();
        }

    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {

        // Update
        sender ??= "<<NULL>>";

        // Log exception
        Trace.WriteLine($"\n\n\n\t*** FATAL APP EXIT ***\n\nException trigger:\n{sender}\n\nException Info:\n{e.ExceptionObject}\n");

        // Try launch self in error mode
        try {
            string ppath = Environment.ProcessPath ?? "";
            if (!string.IsNullOrEmpty(ppath)) {
                ProcessStartInfo pinfo = new(ppath, "-report-error");
                if (Process.Start(pinfo) is null) {
                    logger.Error("Failed to open error reporter...");
                }
            }
        } catch {
            logger.Error("Failed to launch self in error mode!");
        }

        // Close logger with exit code
        BattlegroundsContext.Log?.SaveAndClose(int.MaxValue);

    }

    /// <summary>
    /// Try find a data template from <see cref="Application"/> resources.
    /// </summary>
    /// <param name="type">The type to try and find data template for.</param>
    /// <returns>If found, the linked <see cref="DataTemplate"/> instance; Otherwise <see langword="null"/> if not found.</returns>
    public DataTemplate? TryFindDataTemplate(Type type) => this.TryFindDataTemplate(Current.Resources, type);

    private DataTemplate? TryFindDataTemplate(ResourceDictionary dictionary, Type type) {

        // Loop through basic entry
        foreach (DictionaryEntry res in dictionary) {
            if (res.Value is DataTemplate template && template.DataType is not null && template.DataType.Equals(type)) {
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

    private static void CheckForUpdate() {

        // Check for newer version
        if (!Update.IsNewVersion()) 
            return;

        // Null check
        if (App.Views.GetModalControl() is not ModalControl mControl) {
            return;
        }

        Current.Dispatcher.Invoke(() => {

            // Do modal
            YesNoPrompt.Show(mControl, (vm, resault) => {

                // Check return value
                if (resault is not ModalDialogResult.Confirm) {
                    return;
                }

                Coroutine.StartCoroutine(RunUpdate(mControl));

            }, "New Update", "New update detected. Do you want to download?");

        });

    }

    private static IEnumerator RunUpdate(ModalControl control) {

        yield return new WaitTimespan(TimeSpan.FromSeconds(0.5));
        Current.Dispatcher.Invoke(() => {
            // Create downloadInProgress
            var downloadInProgress = new UpdateDownloader();

            // Show modal
            control.ShowModal(downloadInProgress);
        });
        yield return new WaitTimespan(TimeSpan.FromSeconds(1.0));
        Update.UpdateApplication();

    }

}
