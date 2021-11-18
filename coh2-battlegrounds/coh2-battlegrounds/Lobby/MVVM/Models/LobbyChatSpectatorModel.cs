using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Locale;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Controls;
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

        public EventCommand<KeyEventArgs> EnterKey { get; }

        public string MessageContent { get; set; }

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
                }
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
                Click = new RelayCommand(() => this.Send())
            };

            // Create enter key
            this.EnterKey = new EventCommand<KeyEventArgs>(this.SendEnter);

        }

        public void SendEnter(object sender, KeyEventArgs args) {
            if (sender is not BGTextbox tb) {
                return;
            }
            if (args.Key is Key.Enter) {
                this.Send(tb.Text);
            }
        }

        public void Send(string content = null) {

            // Set message content
            if (content is null) {
                content = this.MessageContent;
            }

            // Make sure there's actually content to send.
            if (string.IsNullOrWhiteSpace(content)) {
                return;
            }

            // Chat message
            Trace.WriteLine("Sending chat message: " + content);

            // Append message
            this.NewMessage(this.m_handler.Self.Name, content, Color.FromRgb(255, 255, 255));

            // Send using broker
            switch (this.SendFilter.CurrentIndex) {
                case 0: // All
                    this.m_handler.BrokerHandler.SendChatMessage(null, content);
                    break;
                case 1: // Team
                    if (this.m_handler.Lobby.Allies.IsTeamMember(this.m_handler.Self)) {
                        this.m_handler.BrokerHandler.SendChatMessage(this.m_handler.Lobby.Allies.Slots.Where(x => x.SlotState is TeamSlotState.Occupied)
                            .Select(x => x.SlotOccupant.Id).ToArray(), content);
                    } else if (this.m_handler.Lobby.Axis.IsTeamMember(this.m_handler.Self)) {
                        this.m_handler.BrokerHandler.SendChatMessage(this.m_handler.Lobby.Axis.Slots.Where(x => x.SlotState is TeamSlotState.Occupied)
                            .Select(x => x.SlotOccupant.Id).ToArray(), content);
                    } else {
                        // TODO: Implement
                    }
                    break;
            }

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
