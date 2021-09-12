﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Modding;
using Battlegrounds.Networking.Lobby;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyScenarioItem {
        private readonly string m_display;
        public Scenario Scenario { get; }
        public LobbyScenarioItem(Scenario scenario) {
            this.Scenario = scenario;
            this.m_display = this.Scenario.Name;
            if (this.Scenario.Name.StartsWith("$", false, CultureInfo.InvariantCulture) && uint.TryParse(this.Scenario.Name[1..], out uint key)) {
                this.m_display = GameLocale.GetString(key);
            }
        }
        public override string ToString()
            => this.m_display;
    }

    public class LobbyGamemodeItem {
        private readonly string m_display;
        public IGamemode Gamemode { get; }
        public LobbyGamemodeItem(IGamemode gamemode) {
            this.Gamemode = gamemode;
            this.m_display = GameLocale.GetString(gamemode.DisplayName);
        }
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj) => obj is LobbyGamemodeItem item && item.Gamemode == this.Gamemode;
        public override string ToString() => this.m_display;
    }

    public class LobbyGamemodeOptionItem {
        private readonly string m_display;
        public IGamemodeOption Option { get; }
        public LobbyGamemodeOptionItem(IGamemodeOption gamemodeOption) {
            this.Option = gamemodeOption;
            this.m_display = GameLocale.GetString(gamemodeOption.Title);
        }
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj) => obj is LobbyGamemodeOptionItem item && item.Option == this.Option;
        public override string ToString() => this.m_display;
    }

    public class LobbyBinaryOptionItem {
        public bool IsOn { get; }
        public LobbyBinaryOptionItem(bool isTrueOption) => this.IsOn = isTrueOption;
        public static ObservableCollection<LobbyBinaryOptionItem> CreateCollection()
            => new(new LobbyBinaryOptionItem[] { new(false), new(true) });
        public override string ToString() => this.IsOn ? "On" : "Off";
    }

    public class LobbyModPackageItem {
        public ModPackage Package { get; }
        public LobbyModPackageItem(ModPackage modPackage) => this.Package = modPackage;
        public override string ToString() => this.Package.PackageName;
    }

    public class LobbyModel : IViewModel, INotifyPropertyChanged {

        private static readonly ImageSource __mapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

        private readonly LobbyHandler m_handler;
        private ModPackage m_package;
        private bool m_hasSetDefaults;

        public event PropertyChangedEventHandler PropertyChanged;

        public LobbyButtonModel EditCompany { get; }

        public LobbyButtonModel ExitLobby { get; }

        public LobbyButtonModel StartMatch { get; }

        public ImageSource SelectedMatchScenario { get; set; }

        public LobbyDropdownModel<LobbyScenarioItem> ScenarioSelection { get; }

        public LobbyDropdownModel<LobbyGamemodeItem> GamemodeSelection { get; }

        public LobbyDropdownModel<LobbyGamemodeOptionItem> GamemodeOptionSelection { get; }

        public LobbyDropdownModel<LobbyBinaryOptionItem> WeatherSelection { get; }

        public LobbyDropdownModel<LobbyBinaryOptionItem> SupplySystemSelection { get; }

        public LobbyDropdownModel<LobbyModPackageItem> ModPackageSelection { get; }

        public LobbyTeam Allies { get; }

        public LobbyTeam Axis { get; }

        public bool SingleInstanceOnly => false;

        public LobbyModel(LobbyHandler handler) {

            // Set handler
            this.m_handler = handler;

            // Create edit company button
            this.EditCompany = new() {
                Click = new RelayCommand(this.EditSelfCompany),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_EditCompany")
            };

            // Create exit lobby button
            this.ExitLobby = new() {
                Click = new RelayCommand(this.LeaveLobby),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_LeaveLobby")
            };

            // Create start match button
            this.StartMatch = new() {
                Click = new RelayCommand(this.BeginMatch),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_StartMatch")
            };

            // Create package list
            List<LobbyModPackageItem> modPackages = new();
            ModManager.EachPackage(x => modPackages.Add(new(x)));

            // Create mod package dropdown
            this.ModPackageSelection = new(true, this.m_handler.IsHost) {
                Items = new(modPackages),
                OnSelectionChanged = this.OnPackageChanged
            };

            // Set package here
            if (this.m_handler.IsHost) {
                this.m_package = this.ModPackageSelection.Items[0].Package;
            }

            // Create scenario selection dropdown
            this.ScenarioSelection = new(true, this.m_handler.IsHost) {
                Items = new(ScenarioList.GetList()
                    .Where(x => x.IsVisibleInLobby)
                    .Select(x => new LobbyScenarioItem(x))),
                OnSelectionChanged = this.OnScenarioChanged
            };

            // Create gamemode selection dropdown
            this.GamemodeSelection = new(true, this.m_handler.IsHost) {
                Items = new(),
                OnSelectionChanged = OnGamemodeChanged
            };

            // Create gamemode option selection dropdown
            this.GamemodeOptionSelection = new(true, this.m_handler.IsHost) {
                Items = new(),
                OnSelectionChanged = this.OnGamemodeOptionChanged
            };

            // Create weather selection dropdown
            this.WeatherSelection = new(true, this.m_handler.IsHost) {
                Items = LobbyBinaryOptionItem.CreateCollection()
            };

            // Create supply selection dropdown
            this.SupplySystemSelection = new(true, this.m_handler.IsHost) {
                Items = LobbyBinaryOptionItem.CreateCollection()
            };

            // Init dropdown values (if host)
            if (handler.IsHost) {
                this.ScenarioSelection.SetSelection(x => x.Scenario.RelativeFilename == BattlegroundsInstance.LastPlayedMap);
            }

            // Create teams
            this.Allies = new();
            this.Axis = new();
            

        }

        private void EditSelfCompany() {

        }

        private void LeaveLobby() {

        }

        private void BeginMatch() {

        }

        private string LobbyServerTranslator(string modeKey, string modeValue) => modeValue;

        private int OnPackageChanged(int current, int next, LobbyModPackageItem item) {
            this.m_package = item.Package;
            return next;
        }

        private void TrySetMapSource(Scenario scenario) {

            // Set to default case
            this.SelectedMatchScenario = __mapNotFound;

            // Get Path
            string fullpath = Path.GetFullPath($"bin\\gfx\\map_icons\\{scenario.RelativeFilename}_map.tga");

            // Check if file exists
            if (File.Exists(fullpath)) {
                try {
                    this.SelectedMatchScenario = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                } catch (BadImageFormatException bife) {
                    Trace.WriteLine(bife, nameof(this.TrySetMapSource));
                }
            } else {
                fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.RelativeFilename}_map.tga");
                if (File.Exists(fullpath)) {
                    try {
                        this.SelectedMatchScenario = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                    } catch (BadImageFormatException bife) {
                        Trace.WriteLine(bife, nameof(this.TrySetMapSource));
                    }
                } else {
                    Trace.WriteLine($"Failed to locate file: {fullpath}", nameof(this.TrySetMapSource));
                }
            }

            // Inform view the map selection was changed
            this.PropertyChanged?.Invoke(this, new(nameof(this.SelectedMatchScenario)));

        }

        private int OnScenarioChanged(int current, int next, LobbyScenarioItem item) {

            // If item is not null
            if (item is not null) {

                // Update map
                _ = Application.Current.Dispatcher.BeginInvoke(() => this.TrySetMapSource(item.Scenario));

                // Update gamemode (and option)
                this.UpdateGamemodeAndOption(item.Scenario);

                // Update lobby
                if (this.m_handler.Lobby is HostedLobby lobby) {
                    lobby.SetMode(item.Scenario.RelativeFilename, string.Empty, string.Empty, this.LobbyServerTranslator);
                }

            }

            // Return selected value
            return next;

        }

        private void UpdateGamemodeAndOption(Scenario scenario) {

            // Get available gamemodes
            var guid = this.m_package.GamemodeGUID;
            List<LobbyGamemodeItem> available = (scenario.Gamemodes.Count > 0 ? WinconditionList.GetGamemodes(guid, scenario.Gamemodes) : WinconditionList.GetGamemodes(guid))
                .Select(x => new LobbyGamemodeItem(x)).ToList();

            // Update if there's any change in available gamemodes 
            if (this.GamemodeSelection.Items.Count != available.Count || available.Any(x => !this.GamemodeSelection.Items.Contains(x))) {

                // Clear current gamemode selection
                this.GamemodeSelection.Items.Clear();
                available.ForEach(x => this.GamemodeSelection.Items.Add(x));

                // Set default if not set
                if (!this.m_hasSetDefaults) {
                    this.GamemodeSelection.SetSelection(x => x.Gamemode.Name == BattlegroundsInstance.LastPlayedGamemode);
                } else {
                    this.GamemodeSelection.SetSelection(_ => true);
                }

            }

        }

        private int OnGamemodeChanged(int current, int next, LobbyGamemodeItem item) {

            // Get options
            var options = item.Gamemode.Options;

            // Clear available options
            this.GamemodeOptionSelection.Items.Clear();

            // Hide options
            if (options is null || options.Length is 0) {

                // Set options to hidden
                this.GamemodeOptionSelection.IsVisible = false;

            } else {

                // Update options
                _ = options.ForEach(x => this.GamemodeOptionSelection.Items.Add(new(x)));

                // Set default
                if (!this.m_hasSetDefaults) {
                    this.GamemodeOptionSelection.SetSelection(x => x.Option.Value == BattlegroundsInstance.LastPlayedGamemodeSetting);
                    this.m_hasSetDefaults = true;
                } else {
                    var defaultOption = item.Gamemode.Options[item.Gamemode.DefaultOptionIndex];
                    this.GamemodeOptionSelection.SetSelection(x => x.Option == defaultOption);
                }

                // Set options to hidden
                this.GamemodeOptionSelection.IsVisible = true;

            }

            // Update lobby
            if (this.m_handler.Lobby is HostedLobby lobby) {
                lobby.SetMode(string.Empty, item.Gamemode.Name, string.Empty, this.LobbyServerTranslator);
            }

            // Return selected
            return next;

        }

        private int OnGamemodeOptionChanged(int current, int next, LobbyGamemodeOptionItem item) {

            // Update lobby
            if (this.m_handler.Lobby is HostedLobby lobby && item is not null) {
                string optionVal = item.Option.Value.ToString(CultureInfo.InvariantCulture);
                lobby.SetMode(string.Empty, string.Empty, optionVal, this.LobbyServerTranslator);
            }

            // Return selected
            return next;

        }

    }

}