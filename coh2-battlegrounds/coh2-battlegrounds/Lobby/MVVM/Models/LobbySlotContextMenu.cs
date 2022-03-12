using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.Locale;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbySlotContextMenu : INotifyPropertyChanged {

        private static readonly LocaleKey _LocKick = new("");
        private static readonly LocaleKey _LocRemove = new("");

        private readonly LobbySlot m_source;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ShowPlayerCard { get; set; }

        public ICommand KickOccupant { get; set; }

        public ICommand LockSlot { get; set; }

        public ICommand UnlockSlot { get; set; }

        public ICommand AddAIPlayer { get; set; }

        public Visibility LockVisibility
            => this.m_source.IsHost && this.m_source.IsOpen && !this.m_source.IsLocked ? Visibility.Visible : Visibility.Collapsed;

        public Visibility OpenVisibility
            => this.m_source.IsHost && this.m_source.IsLocked ? Visibility.Visible : Visibility.Collapsed;

        public Visibility StateSelectorVisibility
            => (this.LockVisibility is Visibility.Visible || this.OpenVisibility is Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility KickVisibility =>
            !this.m_source.IsSelf && this.m_source.IsHost && !this.m_source.IsOpen ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AIOptionsVisibility => this.m_source.IsOpen && this.m_source.IsHost ? Visibility.Visible : Visibility.Collapsed;

        public LobbySlotContextMenu(LobbySlot slotSource) => this.m_source = slotSource;

        public void RefreshAvailability() {
            this.PropertyChanged?.Invoke(this, new(nameof(this.LockVisibility)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.OpenVisibility)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.KickVisibility)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.AIOptionsVisibility)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.StateSelectorVisibility)));
        }

    }

}
