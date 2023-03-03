using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.UI;

namespace Battlegrounds.Lobby.Components;

/// <summary>
/// Static container class for auxiliary models for the lobby implementation
/// </summary>
public static class Buttons {

    /// <summary>
    /// Basic immutable lobby button.
    /// </summary>
    /// <param name="Title">The tite of the button.</param>
    /// <param name="IsEnabled">Enabled state of the button.</param>
    /// <param name="Click">Click command.</param>
    /// <param name="Visible">Visibility of the button.</param>
    /// <param name="Tooltip">Associated tooltip of the button.</param>
    public record SimpleButton(string Title, bool IsEnabled, RelayCommand Click, Visibility Visible, string Tooltip);

    /// <summary>
    /// Mutable lobby button.
    /// </summary>
    /// <param name="Click">The click handler.</param>
    /// <param name="Visible">The basic visibility state (Immutable).</param>
    public record MutableButton(RelayCommand Click, Visibility Visible) : INotifyPropertyChanged {

        // Internal fields
        private bool m_isEnabled;
        private Visibility m_iconVisible;
        private Visibility m_visible;
        private string? m_tooltip;
        private string? m_title;

        // Property changed handler
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Get or set if the button is enabled.
        /// </summary>
        public bool IsEnabled {
            get => this.m_isEnabled;
            set {
                this.m_isEnabled = value;
                this.PropertyChanged?.Invoke(this, new(nameof(IsEnabled)));
            }
        }

        /// <summary>
        /// Get or set the associated tooltip of the button.
        /// </summary>
        public string? Tooltip {
            get => this.m_tooltip;
            set {
                this.m_tooltip = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Tooltip)));
            }
        }

        /// <summary>
        /// Get or set the title of the button.
        /// </summary>
        public string? Title {
            get => this.m_title;
            set {
                this.m_title = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Title)));
            }
        }

        /// <summary>
        /// Get or set the visibility of the button.
        /// </summary>
        /// <remarks>
        /// Bind to this if visibility can be changed.
        /// </remarks>
        public Visibility Visibility {
            get => this.m_visible;
            set {
                this.m_visible = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Visibility)));
            }
        }

        /// <summary>
        /// Get or set the visibility of a notification icon.
        /// </summary>
        public Visibility NotificationVisible {
            get => this.m_iconVisible;
            set {
                this.m_iconVisible = value;
                this.PropertyChanged?.Invoke(this, new(nameof(NotificationVisible)));
            }
        }

    }

}
