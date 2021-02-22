using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BattlegroundsApp.Controls.Lobby.Chatting {
    
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl {

        private IChatController m_chat;

        /// <summary>
        /// Get or set the controller for handling chat interactions.
        /// </summary>
        public IChatController Chat { 
            get => this.m_chat; 
            set {
                this.m_chat = value;
                this.m_chat.OnReceived += this.ChatMessageReceived;
            } 
        }

        public ChatView() {
            InitializeComponent();
        }

        private void MessageText_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.Button_Click(sender, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            // Send message (if any)
            if (this.messageText.Text.Length > 0) {
                
                // Actually send message
                this.m_chat?.SendChatMessage(this.messageText.Text);

                // Display message
                this.DisplayMessage($"{this.m_chat?.Self ?? ""}: {this.messageText.Text}\n");
                this.ChatHistory.ScrollToEnd();

                // Clear input text
                this.messageText.Text = string.Empty;

            }

        }

        private void ChatMessageReceived(string sender, string message)
            => this.DisplayMessage($"{sender} {message}\n");

        public void DisplayMessage(string full) => this.Dispatcher.Invoke(() => {
            this.ChatHistory.AppendText(full);
        });

    }

}
