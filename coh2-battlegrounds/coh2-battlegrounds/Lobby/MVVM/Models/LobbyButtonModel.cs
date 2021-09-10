using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.Locale;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyButtonModel : INotifyPropertyChanged {

        private Visibility m_visiblity = Visibility.Visible;
        private bool m_enabled = true;

        private LocaleKey m_text;
        private LocaleKey m_tooltip;

        public ICommand Click { get; init; }

        public Visibility Visible { get => this.m_visiblity; set { this.m_visiblity = value; this.PropertyChanged?.Invoke(this, new(nameof(this.Visible))); } }

        public bool Enabled { get => this.m_enabled; set { this.m_enabled = value; this.PropertyChanged?.Invoke(this, new(nameof(this.Enabled))); } }

        public LocaleKey Text { get => this.m_text; set { this.m_text = value; this.PropertyChanged?.Invoke(this, new(nameof(this.Text))); } }

        public LocaleKey Tooltip { get => this.m_tooltip; set { this.m_tooltip = value; this.PropertyChanged?.Invoke(this, new(nameof(this.Tooltip))); } }

        public event PropertyChangedEventHandler PropertyChanged;

    }

}
