using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace BattlegroundsApp.Controls.Lobby.Chatting {

    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl {

        /// <summary>
        /// Property handling the list of channels available in the observable collection.
        /// </summary>
        public static readonly DependencyProperty ChannelsProperty
            = DependencyProperty.Register(nameof(Channels), typeof(ObservableCollection<ChatChannel>), typeof(ChatView), new PropertyMetadata(new ObservableCollection<ChatChannel>()));

        public ObservableCollection<ChatChannel> Channels => this.GetValue(ChannelsProperty) as ObservableCollection<ChatChannel>;

        public int SelectedChannel => this.ChatChannelSelector.SelectedIndex >= 0 ? this.ChatChannelSelector.SelectedIndex : 0;

        /// <summary>
        /// Get or set the controller for handling chat interactions.
        /// </summary>
        public IChatController Chat { get; set; }

        public ChatView() {

            // Initialize component
            this.InitializeComponent();

            // Set datacontext
            this.DataContext = this;

        }

        private Color GetChannelColour(int index) => this.Channels.Count == 0 ? Colors.Black : this.Channels[index].ChannelColour;

        private void MessageText_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.Button_Click(sender, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            // Send message (if any)
            if (this.messageText.Text.Length > 0) {

                // Actually send message
                this.Chat?.OnSend(this.SelectedChannel, this.messageText.Text);

                // Clear input text
                this.messageText.Text = string.Empty;

            }

        }

        public void DisplayMessage(string fullMessage, int channel = 0, bool scrollToEnd = true) => this.Dispatcher.Invoke(() => {

            // Add message
            TextRange range = new TextRange(this.ChatHistory.Document.ContentEnd, this.ChatHistory.Document.ContentEnd) {
                Text = $"{fullMessage}\n"
            };
            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(this.GetChannelColour(channel)));

            // goto end
            if (scrollToEnd) {
                this.ChatHistory.ScrollToEnd();
            }

        });

    }

}
