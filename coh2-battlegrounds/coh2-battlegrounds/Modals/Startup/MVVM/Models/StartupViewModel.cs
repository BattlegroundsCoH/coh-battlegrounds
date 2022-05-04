using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds;
using Battlegrounds.Locale;
using Battlegrounds.Steam;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Utilities;

using Microsoft.Win32;

namespace BattlegroundsApp.Modals.Startup.MVVM.Models;

public class StartupViewModel : INotifyPropertyChanged {

    private bool m_browseSteam = true;
    private bool m_broswCoH = true;
    private bool m_isAutoAvailable;
    private Action? m_closeCallback;

    public Visibility IsValidatingSteam { get; set; } = Visibility.Collapsed;

    public Visibility IsValidatingCoH { get; set; } = Visibility.Collapsed;

    public Visibility IsUsernameVisible { get; set; } = Visibility.Collapsed;

    public string DetectedSteampath { get; set; } = string.Empty;

    public string DetectedCoHpath { get; set; } = string.Empty;

    public string LocalUsername { get; set; } = string.Empty;

    public bool IsAutoAvailable {
        get => this.m_isAutoAvailable;
        set {
            this.m_isAutoAvailable = value;
            this.PropertyChanged?.Invoke(this, new(nameof(IsAutoAvailable)));
        }
    }

    public bool IsBrowseSteamAvailable {
        get => this.m_browseSteam;
        set {
            this.m_browseSteam = value;
            this.PropertyChanged?.Invoke(this, new(nameof(IsBrowseSteamAvailable)));
        }
    }

    public bool IsBrowseCoHAvailable {
        get => this.m_broswCoH;
        set {
            this.m_broswCoH = value;
            this.PropertyChanged?.Invoke(this, new(nameof(IsBrowseCoHAvailable)));
        }
    }

    public bool IsContinuePossible => this.Steamstatus is SpinnerStatus.Accepted && this.CoHStatus is SpinnerStatus.Accepted;

    public SpinnerStatus Steamstatus { get; set; }

    public SpinnerStatus CoHStatus { get; set; }

    public RelayCommand Continue { get; }
    public RelayCommand BeginAuto { get; }
    public RelayCommand BrowseSteampath { get; }
    public RelayCommand BrowserCoHpath { get; }
    public RelayCommand<int> LanguageButton { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public StartupViewModel() {
        this.m_isAutoAvailable = true;
        this.BeginAuto = new(this.TryAutoDetect);
        this.BrowseSteampath = new(this.BrowseSteam);
        this.BrowserCoHpath = new(this.BrowseCoH);
        this.LanguageButton = new(this.SetLanguage);
        this.Continue = new(this.SaveAndContinue);
    }

    public void OnClose(Action closeCallback) => this.m_closeCallback = closeCallback;

    public async void TryAutoDetect() {

        // Disable auto
        this.IsAutoAvailable = false;
        this.IsValidatingSteam = Visibility.Visible;
        this.PropertyChanged?.Invoke(this, new(nameof(IsValidatingSteam)));

        // Articifial delay
        await Task.Delay(500);

        // Try find steam
        this.DetectedSteampath = await GetSteam();
        this.PropertyChanged?.Invoke(this, new(nameof(DetectedSteampath)));

        // Articifial delay
        await Task.Delay(500);
        this.ValidateSteam();
        this.IsValidatingCoH = Visibility.Visible;
        this.PropertyChanged?.Invoke(this, new(nameof(IsValidatingCoH)));

        // Try find coh
        this.DetectedCoHpath = await GetCoH();
        this.PropertyChanged?.Invoke(this, new(nameof(DetectedCoHpath)));

        // Artificial delay
        await Task.Delay(500);
        this.ValidateCoH();

    }

    private static async Task<string> GetSteam()
        => await Task.Run(() => Pathfinder.GetOrFindSteamPath());

    private static async Task<string> GetCoH()
        => await Task.Run(() => Pathfinder.GetOrFindCoHPath());

    private void ValidateSteam() {

        // Show visually
        this.IsValidatingSteam = Visibility.Visible;
        this.PropertyChanged?.Invoke(this, new(nameof(IsValidatingSteam)));

        // Found check
        bool validPath = Pathfinder.VerifySteamPath(this.DetectedSteampath);
        bool userFound = false;

        // Collapse
        this.IsUsernameVisible = Visibility.Collapsed;
        this.PropertyChanged?.Invoke(this, new(nameof(IsUsernameVisible)));

        // Try fetch steam user
        try {
            if (validPath) {
                SteamUser? user = SteamInstance.FromLocalInstall();
                if (user is not null) {
                    BattlegroundsInstance.Steam.User = user;
                    userFound = true;
                    this.LocalUsername = BattlegroundsInstance.Localize.GetString("STARTUP_USER", user.Name);
                    this.IsUsernameVisible = Visibility.Visible;
                    this.PropertyChanged?.Invoke(this, new(nameof(LocalUsername)));
                    this.PropertyChanged?.Invoke(this, new(nameof(IsUsernameVisible)));
                }
            }
        } catch (Exception ex) {
            Trace.WriteLine(ex, nameof(ValidateSteam));
        }

        // Validate
        this.Steamstatus = (validPath && userFound) ? SpinnerStatus.Accepted : SpinnerStatus.Refused;
        this.PropertyChanged?.Invoke(this, new(nameof(Steamstatus)));

        // Set browsable
        this.IsBrowseSteamAvailable = this.Steamstatus is not SpinnerStatus.Accepted;
        this.PropertyChanged?.Invoke(this, new(nameof(IsBrowseSteamAvailable)));

        // And refresh continue
        this.PropertyChanged?.Invoke(this, new(nameof(IsContinuePossible)));

    }

    private void ValidateCoH() {

        // Show visually
        this.IsValidatingCoH = Visibility.Visible;
        this.PropertyChanged?.Invoke(this, new(nameof(IsValidatingCoH)));

        // Validate
        this.CoHStatus = Pathfinder.VerifyCoHPath(this.DetectedCoHpath) ? SpinnerStatus.Accepted : SpinnerStatus.Refused;
        this.PropertyChanged?.Invoke(this, new(nameof(CoHStatus)));

        // Set browsable
        this.IsBrowseCoHAvailable = this.CoHStatus is not SpinnerStatus.Accepted;
        this.PropertyChanged?.Invoke(this, new(nameof(IsBrowseCoHAvailable)));

        // And refresh continue
        this.PropertyChanged?.Invoke(this, new(nameof(IsContinuePossible)));

    }

    private void BrowseSteam() {

        // Create OFD
        OpenFileDialog ofd = new() {
            Title = "Select Steam Executable",
            CheckPathExists = true,
            CheckFileExists = true,
            Filter = "Steam Executable (Steam.exe)|Steam.exe",
            Multiselect = false
        };

        // Show it
        if (ofd.ShowDialog() is true) {

            // Grab select file
            this.DetectedSteampath = ofd.FileName;
            this.PropertyChanged?.Invoke(this, new(nameof(DetectedSteampath)));

            // Verify
            this.ValidateSteam();

        }

    }

    private void BrowseCoH() {

        // Create OFD
        OpenFileDialog ofd = new() {
            Title = "Select Company of Heroes 2 Executable",
            CheckPathExists = true,
            CheckFileExists = true,
            Filter = "Company of Heroes 2 Executable (RelicCoH2.exe)|RelicCoH2.exe",
            Multiselect = false
        };

        // Show it
        if (ofd.ShowDialog() is true) {

            // Grab select file
            this.DetectedCoHpath = Path.GetDirectoryName(ofd.FileName) ?? ofd.FileName;
            this.PropertyChanged?.Invoke(this, new(nameof(DetectedCoHpath)));

            // Verify
            this.ValidateCoH();

        }
    }

    private void SaveAndContinue() {

        // Save
        Pathfinder.SteamPath = this.DetectedSteampath;
        Pathfinder.CoHPath = this.DetectedCoHpath;

        // Set instance
        BattlegroundsInstance.SaveInstancePath(BattlegroundsPaths.STEAM_FOLDER, Pathfinder.SteamPath);
        BattlegroundsInstance.SaveInstancePath(BattlegroundsPaths.COH_FOLDER, Pathfinder.CoHPath);

        // Invoke callback
        this.m_closeCallback?.Invoke();

        // Close modal
        App.ViewManager.GetModalControl()?.CloseModal();

    }

    private void SetLanguage(int language) {
        var lang = (LocaleLanguage)language;
        // change
    }

}
