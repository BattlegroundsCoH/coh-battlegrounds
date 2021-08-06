using System.Windows;
using System.Windows.Markup;

namespace BattlegroundsApp.Controls.Lobby.Components {

    /// <summary>
    /// Interaction logic for LobbyButton.xaml
    /// </summary>
    [ContentProperty("Text")]
    public partial class LobbyButton : LobbyControl {

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LobbyButton));

        public static readonly DependencyProperty OptionalTextProperty = DependencyProperty.Register("OptionalText", typeof(string), typeof(LobbyButton));

        public static readonly DependencyProperty IsHostOnlyProperty = DependencyProperty.Register("IsHostOnly", typeof(bool), typeof(LobbyButton));

        /// <summary>
        /// Occures when the <see cref="LobbyButton"/> is clicked while in the <see cref="SelfState"/>.
        /// </summary>
        public event RoutedEventHandler Click;

        /// <summary>
        /// Get or set the text that is displayed on the <see cref="LobbyButton"/> while in the <see cref="SelfState"/>.
        /// </summary>
        public string Text {
            get => this.GetValue(TextProperty) as string;
            set => this.SetValue(TextProperty, value);
        }

        /// <summary>
        /// Get or set the text that is displayed instead of the <see cref="LobbyButton"/> while in the <see cref="OtherState"/>.
        /// </summary>
        public string OptionalText {
            get => this.GetValue(OptionalTextProperty) as string;
            set => this.SetValue(OptionalTextProperty, value);
        }

        /// <summary>
        /// Get or set whether the <see cref="LobbyButton"/> should only be visible to the host.
        /// </summary>
        public bool IsHostOnly {
            get => (bool)this.GetValue(IsHostOnlyProperty);
            set => this.SetValue(IsHostOnlyProperty, value);
        }

        public LobbyButton() {
            this.InitializeComponent();
            this.DataContext = this;
            this.SelfButton.Click += this.HandleButton_Click;
            this.HostButton.Click += this.HandleButton_Click;
        }

        private void HandleButton_Click(object sender, RoutedEventArgs e) => this.Click?.Invoke(sender, e);

        protected override LobbyControlState OnStateChange(LobbyControlState newState, LobbyControlContext controlContext) {
            if (newState is SelfState && IsHostOnly && !controlContext.IsHost) {
                return this.ButtonOtherState;
            }
            return newState;
        }

    }

}
