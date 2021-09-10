using System.Windows.Input;

using Battlegrounds.Locale;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.MVVM.Models {

    public class LobbyBrowserButton {
        public ICommand Click { get; init; }
        public LocaleKey Text { get; init; }
        public LocaleKey Tooltip { get; init; }
    }

    public class LobbyBrowserViewModel : IViewModel {

        public LobbyBrowserButton Refresh { get; }

        public LobbyBrowserButton Join { get; }

        public LobbyBrowserButton Host { get; }

        public bool SingleInstanceOnly => true;

        public LobbyBrowserViewModel() {

            // Create refresh
            this.Refresh = new() {
                Click = new RelayCommand(this.RefreshButton),
                Text = new("GameBrowserView_Refresh")
            };

            // Create join
            this.Join = new() {
                Click = new RelayCommand(this.JoinButton),
                Text = new("GameBrowserView_Join_Game")
            };

            // Create host
            this.Host = new() {
                Click = new RelayCommand(this.HostButton),
                Text = new("GameBrowserView_Host_Game")
            };

        }

        public void RefreshButton() {

        }

        public void HostButton() {

        }

        public void JoinButton() {

        }

    }

}
