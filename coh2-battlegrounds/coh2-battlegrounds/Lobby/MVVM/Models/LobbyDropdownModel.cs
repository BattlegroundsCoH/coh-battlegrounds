using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public delegate int LobbyDropdownModelSelectionChangedEvent<T>(int oldIndex, int newIndex, T newItem);

    public class LobbyDropdownModel<T> {

        private int m_index;

        public bool IsHostOnly { get; }

        public bool IsHost { get; }

        public bool IsEnabled { get; set; }

        public Visibility IsDropdownVisible => (this.IsHost && this.IsHostOnly) || !this.IsHostOnly ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsLabelVisible => this.IsDropdownVisible == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        public ObservableCollection<T> Items { get; init; }

        public LobbyDropdownModelSelectionChangedEvent<T> OnSelectionChanged { get; init; }

        public int CurrentIndex {
            get => this.m_index;
            set => this.SelectionChanged(value);
        }

        public LobbyDropdownModel(bool hostOnly, bool isHost) {
            this.IsHostOnly = hostOnly;
            this.IsHost = isHost;
        }

        public void SetSelection(Predicate<T> firstMatch) {
            for (int i = 0; i < this.Items.Count; i++) {
                if (firstMatch(this.Items[i])) {
                    this.CurrentIndex = i;
                    return;
                }
            }
        }

        private void SelectionChanged(int index) {

            // Verify input
            if (index < -1 || index >= this.Items.Count) {
                return;
            }

            // Determine action
            if (index is -1) {
                this.m_index = this.OnSelectionChanged?.Invoke(this.m_index, -1, default) ?? -1;
            } else {
                var item = this.Items[index];
                this.m_index = this.OnSelectionChanged?.Invoke(this.m_index, index, item) ?? -1;
            }

        }

    }

}
