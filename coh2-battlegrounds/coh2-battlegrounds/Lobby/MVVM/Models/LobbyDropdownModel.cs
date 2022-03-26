using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public delegate int LobbyDropdownModelSelectionChangedEvent<T>(int oldIndex, int newIndex, T newItem);

    public abstract class LobbyDropdownModel {

        public abstract string DropdownID { get; }

        public abstract string LabelContent { get; set; }

    }

    public class LobbyDropdownModel<T> : LobbyDropdownModel, INotifyPropertyChanged {

        private int m_index;
        private string m_label;
        private bool m_visible;

        private readonly bool m_isAllowedTriggers;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsHostOnly { get; }

        public bool IsHost { get; }

        public bool IsEnabled { get; set; }

        public bool IsVisible {
            get => this.m_visible;
            set {
                this.m_visible = value;
                this.PropertyChanged?.Invoke(this, new(nameof(this.IsVisible)));
                this.PropertyChanged?.Invoke(this, new(nameof(this.IsDropdownVisible)));
                this.PropertyChanged?.Invoke(this, new(nameof(this.IsLabelVisible)));
            }
        }

        public Visibility IsDropdownVisible => this.GetDropdownVisibility();

        public Visibility IsLabelVisible => this.GetLabelVisibility();

        public Visibility ComponentVisible => this.IsVisible ? Visibility.Visible : Visibility.Hidden;

        public override string LabelContent {
            get => this.m_label;
            set {
                this.m_label = value;
                this.PropertyChanged?.Invoke(this, new(nameof(this.LabelContent)));
            }
        }

        public ObservableCollection<T> Items { get; init; }

        public LobbyDropdownModelSelectionChangedEvent<T> OnSelectionChanged { get; init; }

        public override string DropdownID { get; }

        public int CurrentIndex {
            get => this.m_index;
            set => this.SelectionChanged(value);
        }

        public LobbyDropdownModel(bool hostOnly, bool isHost, string uid = "") {
            this.DropdownID = uid;
            this.IsHostOnly = hostOnly;
            this.IsHost = isHost;
            this.m_visible = true;
            this.m_isAllowedTriggers = (this.IsHostOnly && this.IsHost) || !this.IsHostOnly;
        }

        private Visibility GetDropdownVisibility() {

            // If not visible, hide
            if (!this.m_visible) {
                return Visibility.Hidden;
            }

            // Define host parameters
            bool hostEnabled = this.IsHost && this.IsHostOnly;
            bool any = !this.IsHostOnly;

            // Return visiblity
            return (hostEnabled || any) ? Visibility.Visible : Visibility.Collapsed;

        }

        private Visibility GetLabelVisibility() {

            // If not visible, hide
            if (!this.m_visible) {
                return Visibility.Hidden;
            }

            // Get dropdown
            var dropdown = this.GetDropdownVisibility();

            // Invert it
            return (dropdown is Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;

        }

        public void SetSelection(Predicate<T> firstMatch) {
            for (int i = 0; i < this.Items.Count; i++) {
                if (firstMatch(this.Items[i])) {
                    this.CurrentIndex = i;
                    this.PropertyChanged?.Invoke(this, new(nameof(this.CurrentIndex)));
                    return;
                }
            }
        }

        private void SelectionChanged(int index) {

            // Verify ability to trigger this
            if (!this.m_isAllowedTriggers) {
                return;
            }

            // Verify input
            if (index <= -1 || index >= this.Items.Count) {
                return;
            }

            // Update
            var item = this.Items[index];
            this.m_index = this.OnSelectionChanged?.Invoke(this.m_index, index, item) ?? index;

        }

        public T GetSelected() {

            // Return element if any selected
            if (this.m_index >= 0 && this.m_index <= this.Items.Count) {
                return this.Items[this.m_index];
            }

            // Return a default value
            return this.Items.Count > 0 ? this.Items[0] : default;

        }

    }

}
