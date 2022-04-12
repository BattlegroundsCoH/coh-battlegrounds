using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Battlegrounds;
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

        public record ChatButton(string Title, RelayCommand Click);
        public record ChatFilterDropdown(ObservableCollection<LobbyChannelModel> Channels) {
            private int m_currentIndex;
            public int CurrentIndex {
                get => m_currentIndex;
                set {
                    this.m_currentIndex = value;
                }
            }
        }

        private static readonly Func<string> LOCSTR_SEND = () => BattlegroundsInstance.Localize.GetString("LobbyChat_Send");

        private readonly LobbyAPI m_handle;
        private readonly LocaleKey m_allFilter;
        private readonly LocaleKey m_teamFilter;
        private readonly FontFamily m_font = new FontFamily("Open Sans");

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool SingleInstanceOnly => false;

        public Visibility SpectatorVisibility => this.Spectators.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public ObservableCollection<string> Spectators { get; set; }

        public FlowDocument? MessageDocument { get; set; }

        public ChatFilterDropdown SendFilter { get; }

        public ChatButton SendMessage { get; }

        public EventCommand<KeyEventArgs> EnterKey { get; }

        public string? MessageContent { get; set; }

        public LobbyChatSpectatorModel(LobbyAPI lobbyHandler) {
            
            // Set internal handler
            this.m_handle = lobbyHandler;
            this.m_handle.OnChatMessage += this.OnChatMessage;
            this.m_handle.OnSystemMessage += this.OnSystemMessage;

            // Create locale keys
            this.m_allFilter = new("LobbyChat_FilterAll");
            this.m_teamFilter = new("LobbyChat_FilterTeam");

            // Create filter
            this.SendFilter = new(new(new List<LobbyChannelModel> { new(this.m_allFilter, 0), new(this.m_teamFilter, 1)}));

            // Create chat history
            Application.Current.Dispatcher.Invoke(() => {
                this.MessageDocument = new();
            });

            // Create lists
            this.Spectators = new();

            // Create sendmessage button
            this.SendMessage = new(LOCSTR_SEND(), new(() => this.Send()));

            // Create enter key
            this.EnterKey = new EventCommand<KeyEventArgs>(this.SendEnter);

        }

        private void OnChatMessage(LobbyAPIStructs.LobbyMessage msg) {
            
            // Convert lobby message colour to System.Colour
            var colour = (Color)ColorConverter.ConvertFromString(msg.Colour);

            // Invoke dispatcher
            Application.Current.Dispatcher.Invoke(() => {

                // Display
                this.NewMessage(msg.Sender.Trim(), msg.Message.Trim(), colour, msg.Timestamp, msg.Channel);

            });

        }

        private void OnSystemMessage(ulong who, string name, string context) {
            switch (context) {
                case "JOIN":
                    this.SystemMessage($"{name} joined the lobby", Colors.Yellow);
                    break;
                case "LEFT":
                    this.SystemMessage($"{name} has left the lobby", Colors.Yellow);
                    break;
                case "KICK":
                    this.SystemMessage($"{name} was kicked from the lobby", Colors.Yellow);
                    break;
                default:
                    Trace.WriteLine($"Unknown context triggered by '{who}' ({name}) ctxt = {context}", nameof(LobbyChatSpectatorModel));
                    break;
            }
        }

        public void SendEnter(object sender, KeyEventArgs args) {
            if (sender is not BGTextbox tb) {
                return;
            }
            if (args.Key is Key.Enter) {
                this.Send(tb.Text);
            }
        }

        public void Send(string? content = null) {

            // Set message content
            if (content is null) {
                content = this.MessageContent;
            }

            // Trim content
            content = content?.Trim() ?? string.Empty;

            // Make sure there's actually content to send.
            if (string.IsNullOrWhiteSpace(content)) {
                return;
            }

            // Chat message
            Trace.WriteLine("Sending chat message: " + content);

            // Forward declare
            string channel = "?";
            string timestamp = DateTime.Now.ToShortTimeString();

            // Send using broker
            switch (this.SendFilter.CurrentIndex) {
                case 0: // All
                    this.m_handle.GlobalChat(this.m_handle.Self.ID, content);
                    channel = "All";
                    break;
                case 1: // Team
                    this.m_handle.TeamChat(this.m_handle.Self.ID, content);
                    channel = "Team";
                    break;
                default:
                    break;
            }

            // Append message
            this.NewMessage(this.m_handle.Self.Name, content, Color.FromRgb(255, 255, 255), timestamp, channel);

            // Reset message content
            this.MessageContent = string.Empty;

            // Trigger refresh
            this.PropertyChanged?.Invoke(this, new(nameof(this.MessageContent)));

        }

        public void NewMessage(string from, string msg, Color colour, string time, string channel) {

            // Create full message
            var fullMessage = $"[{time}][{channel}] {from}: {msg}";

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
            this.MessageDocument?.Blocks.Add(p);

        }

        public void SystemMessage(string message, Color colour) {

            // Run on GUI thread
            Application.Current.Dispatcher.Invoke(() => {

                // Create full message
                var fullMessage = $"[System] {message}";

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
                this.MessageDocument?.Blocks.Add(p);

            });

        }

        public bool UnloadViewModel() => true;

    }

}
