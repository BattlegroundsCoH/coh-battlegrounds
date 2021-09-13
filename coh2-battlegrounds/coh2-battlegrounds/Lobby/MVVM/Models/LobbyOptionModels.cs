using System.Collections.ObjectModel;
using System.Globalization;

using Battlegrounds.Game.Database;
using Battlegrounds.Modding;

using BattlegroundsApp.Resources;

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

}
