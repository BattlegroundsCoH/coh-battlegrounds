using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Battlegrounds.Locale;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyChannelModel {
        public LocaleKey Display { get; }
        public int ChannelID { get; }
        public LobbyChannelModel(LocaleKey display, int cid) {
            this.Display = display;
            this.ChannelID = cid;
        }
        public override string ToString() => Battlegrounds.BattlegroundsInstance.Localize.GetString(this.Display);
    }

    public class LobbyChatSpectatorModel : IViewModel, INotifyPropertyChanged {

        private readonly LobbyHandler m_handler;
        private readonly LocaleKey m_allFilter;
        private readonly LocaleKey m_teamFilter;
        private readonly FontFamily m_font = new FontFamily("Open Sans");

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SingleInstanceOnly => false;

        public Visibility SpectatorVisibility => this.Spectators.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public ObservableCollection<string> Spectators { get; set; }

        public FlowDocument MessageDocument { get; set; }

        public LobbyDropdownModel<LobbyChannelModel> SendFilter { get; }

        public LobbyButtonModel SendMessage { get; }

        public string MessageContent { get; set; }

        public int SelectedFilter { get; set; }

        public LobbyChatSpectatorModel(LobbyHandler lobbyHandler) {
            
            // Set internal handler
            this.m_handler = lobbyHandler;

            // Create locale keys
            this.m_allFilter = new("LobbyChat_FilterAll");
            this.m_teamFilter = new("LobbyChat_FilterTeam");

            // Create filter
            this.SendFilter = new(false, lobbyHandler.IsHost) {
                Items = new() {
                    new(this.m_allFilter, 0),
                    new(this.m_teamFilter, 1)
                },
                CurrentIndex = 0
            };

            // Create chat history
            Application.Current.Dispatcher.Invoke(() => {
                this.MessageDocument = new();
            });

            // Create lists
            this.Spectators = new();

            // Create sendmessage button
            this.SendMessage = new() {
                Text = new("LobbyChat_Send"),
                Enabled = true,
                Visible = Visibility.Visible,
                Click = new RelayCommand(this.Send)
            };

        }

        public void Send() {

            // Chat message
            Trace.WriteLine("Sending chat message: " + this.MessageContent);

            // Append message
            this.NewMessage("Me", this.MessageContent, Color.FromRgb(255, 255, 255));

            // Reset message content
            this.MessageContent = string.Empty;

            // Trigger refresh
            this.PropertyChanged?.Invoke(this, new(nameof(this.MessageContent)));

        }

        public void NewMessage(string from, string msg, Color colour) {

            // Create full message
            var fullMessage = $"{from}: {msg}";

            // Create paragraph to append
            Paragraph p = new() {
                Margin = new(0)
            };
            p.Inlines.Add(new Run(fullMessage) { 
                Foreground = new SolidColorBrush(colour),
                FontSize = 14.0,
                FontFamily = this.m_font
            });

            // Add to message history
            this.MessageDocument.Blocks.Add(p);

        }

    }

}
