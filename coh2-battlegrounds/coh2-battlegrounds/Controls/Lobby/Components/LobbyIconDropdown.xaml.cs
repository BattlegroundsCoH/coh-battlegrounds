using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Battlegrounds.Functional;

namespace BattlegroundsApp.Controls.Lobby.Components {
    
    /// <summary>
    /// Interaction logic for LobbyIconDropdown.xaml
    /// </summary>
    public partial class LobbyIconDropdown : LobbyControl {

        private List<IconComboBoxItem> m_items;
        private int m_selectedIndex;

        public bool EnableEvents {
            get => this.SelfCombobox.EnableEvents;
            set => this.SelfCombobox.EnableEvents = value;
        }

        public new int SelectedIndex {
            get => this.m_selectedIndex;
            set => this.SetSelectedIndex(value);
        }

        public event SelectedItemChangedHandler SelectedItemChanged;

        public LobbyIconDropdown() {
            InitializeComponent();
            this.m_selectedIndex = -1;
            this.SelfCombobox.SelectionChanged += this.SelfCombobox_SelectionChanged;
        }

        private void SelfCombobox_SelectionChanged(object sender, IconComboBoxItem newItem) => this.EnableEvents.Then(() => this.SelectedItemChanged?.Invoke(sender, newItem));

        public void SetItemSource<T>(IEnumerable<T> obj, Func<T, IconComboBoxItem> converter) {
            this.m_items = obj.Select(x => converter(x)).ToList();
            this.SelfCombobox.SetItemSource(this.m_items);
        }

        private void SetSelectedIndex(int index) {
            if (this.State is SelfState) {
                this.m_selectedIndex = this.SelfCombobox.SelectedIndex = index;
            } else if (this.State is OtherState) {
                if (this.m_selectedIndex != index) {
                    this.m_selectedIndex = index;
                    this.OtherIcon.Source = this.m_items[index].Icon;
                    if (this.m_items[index].HasText) {
                        this.OtherIcon.ToolTip = new ToolTip() { Content = this.m_items[index].Text };
                    }
                }
            }
        }

    }

}
