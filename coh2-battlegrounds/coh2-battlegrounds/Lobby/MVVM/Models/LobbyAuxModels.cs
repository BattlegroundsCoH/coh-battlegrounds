using System.ComponentModel;
using System.Windows;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public static class LobbyAuxModels {

    public record LobbyButton(string Title, bool IsEnabled, RelayCommand Click, Visibility Visible, string Tooltip);

    public record LobbyMutButton(RelayCommand Click, Visibility Visible) : INotifyPropertyChanged {
        private bool m_isEnabled;
        private Visibility m_iconVisible;
        private Visibility m_visible;
        private string? m_tooltip;
        private string? m_title;
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsEnabled {
            get => this.m_isEnabled;
            set {
                this.m_isEnabled = value;
                this.PropertyChanged?.Invoke(this, new(nameof(IsEnabled)));
            }
        }
        public string? Tooltip {
            get => this.m_tooltip;
            set {
                this.m_tooltip = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Tooltip)));
            }
        }
        public string? Title {
            get => this.m_title;
            set {
                this.m_title = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Title)));
            }
        }
        public Visibility Visibility {
            get => this.m_visible;
            set {
                this.m_visible = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Visibility)));
            }
        }
        public Visibility NotificationVisible {
            get => this.m_iconVisible;
            set {
                this.m_iconVisible = value;
                this.PropertyChanged?.Invoke(this, new(nameof(NotificationVisible)));
            }
        }
    }

}
