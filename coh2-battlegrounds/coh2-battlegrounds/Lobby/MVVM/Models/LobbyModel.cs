using System;
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

    }

    public class LobbyGamemodeOptionItem {

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

        public bool SingleInstanceOnly => false;

        public LobbyModel(LobbyHandler handler) {

            // Set handler
            this.m_handler = handler;


            // Create package list
            List<LobbyModPackageItem> modPackages = new();
            ModManager.EachPackage(x => modPackages.Add(new(x)));

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

            // Create scenario selection dropdown
            this.ScenarioSelection = new(true, this.m_handler.IsHost) {
                Items = new(ScenarioList.GetList()
                    .Where(x => x.IsVisibleInLobby)
                    .Select(x => new LobbyScenarioItem(x))),
                OnSelectionChanged = this.OnScenarioChanged
            };

            // Create gamemode selection dropdown
            this.GamemodeSelection = new(true, this.m_handler.IsHost) {
            };

            // Create gamemode option selection dropdown
            this.GamemodeOptionSelection = new(true, this.m_handler.IsHost) {
            };

            // Create weather selection dropdown
            this.WeatherSelection = new(true, this.m_handler.IsHost) {
                Items = LobbyBinaryOptionItem.CreateCollection()
            };

            // Create supply selection dropdown
            this.SupplySystemSelection = new(true, this.m_handler.IsHost) {
                Items = LobbyBinaryOptionItem.CreateCollection()
            };

            // Create mod package dropdown
            this.ModPackageSelection = new(true, this.m_handler.IsHost) {
                Items = new(modPackages)
            };

            // Init dropdown values (if host)
            if (handler.IsHost) {
                this.ScenarioSelection.SetSelection(x => x.Scenario.RelativeFilename == BattlegroundsInstance.LastPlayedMap);
            }

        }

        private void EditSelfCompany() {

        }

        private void LeaveLobby() {

        }

        private void BeginMatch() {

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
            if (item is not null) {
                _ = Application.Current.Dispatcher.BeginInvoke(() => this.TrySetMapSource(item.Scenario));
            }
            return next;
        }

    }

}
