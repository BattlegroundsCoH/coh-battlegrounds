using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using Battlegrounds.Functional;

namespace BattlegroundsApp.Controls.Lobby.Components {
    
    /// <summary>
    /// Interaction logic for LobbyDropdown.xaml
    /// </summary>
    [ContentProperty("Items")]
    public partial class LobbyDropdown : LobbyControl, INotifyPropertyChanged {

        private IEnumerable m_itemSource;
        private object m_setSelectedValue;

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(LobbyDropdown));

        public static readonly DependencyProperty DescriptionWidthProperty = DependencyProperty.Register(nameof(DescriptionWidth), typeof(double), typeof(LobbyDropdown));

        public static readonly DependencyProperty DescriptionMinWidthProperty = DependencyProperty.Register(nameof(DescriptionMinWidth), typeof(double), typeof(LobbyDropdown));

        public static readonly DependencyProperty DescriptionMaxWidthProperty = DependencyProperty.Register(nameof(DescriptionMaxWidth), typeof(double), typeof(LobbyDropdown));

        public static readonly DependencyProperty OtherSelectedItemProperty = DependencyProperty.Register(nameof(OtherSelectedItem), typeof(object), typeof(LobbyDropdown));

        public new static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(LobbyDropdown));

        public new static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(LobbyDropdown));

        /// <summary>
        /// Get the <see cref="ItemCollection"/> used to generate the conents of the <see cref="LobbyDropdown"/>. Default is an empty collection.
        /// </summary>
        public new ItemCollection Items => this.SelfOptions.Items;

        /// <summary>
        /// Get or set the source of the items to display in the <see cref="LobbyDropdown"/>.
        /// </summary>
        public new IEnumerable ItemsSource {
            get => this.m_itemSource;
            set => this.UpdateItemSource(value);
        }

        /// <summary>
        /// Get or set the description of the <see cref="LobbyDropdown"/>.
        /// </summary>
        public string Description {
            get => this.GetValue(DescriptionProperty) as string; 
            set => this.SetValue(DescriptionProperty, value); 
        }

        public double DescriptionWidth {
            get => (double)this.GetValue(DescriptionWidthProperty);
            set => this.SetValue(DescriptionWidthProperty, value);
        }

        public double DescriptionMinWidth {
            get => (double)this.GetValue(DescriptionMinWidthProperty);
            set => this.SetValue(DescriptionMinWidthProperty, value);
        }

        public double DescriptionMaxWidth {
            get => (double)this.GetValue(DescriptionMaxWidthProperty);
            set => this.SetValue(DescriptionMaxWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the index of the selected item in the <see cref="LobbyDropdown"/>. Default value is -1.
        /// </summary>
        public new int SelectedIndex { 
            get => this.GetSelectedIndex(); 
            set => this.SetSelectedIndex(value); 
        }

        /// <summary>
        /// Get or set the selected item in the <see cref="LobbyDropdown"/>.
        /// </summary>
        public new object SelectedItem {
            get => this.GetSelectedValue();
            set => this.SetSelectedValue(value);
        }

        /// <summary>
        /// Get the selected value to be displayed in the "other" category
        /// </summary>
        public object OtherSelectedItem => this.GetSelectedValue();

        /// <summary>
        /// Get or set whether events should be triggered.
        /// </summary>
        public bool EnableEvents { get; set; }

        /// <summary>
        /// Checks if there are any items in the <see cref="LobbyDropdown"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if there are more than 0 elements in the <see cref="LobbyDropdown"/>. Otherwise <see langword="false"/>.
        /// </returns>
        public bool HasItemsSource => this.CountOfElements() > 0;

        /// <summary>
        /// Occurs when the selected object in the <see cref="LobbyDropdown"/> has changed.
        /// </summary>
        public event SelectionChangedEventHandler SelectedItemChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public LobbyDropdown() {
            InitializeComponent();
            this.EnableEvents = true;
            this.m_setSelectedValue = "Unknown";
            this.SelfOptions.SelectionChanged += this.SelfOptions_SelectionChanged;
        }

        private void SelfOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => EnableEvents.Then(() => this.SelectedItemChanged?.Invoke(sender, e));

        private void UpdateItemSource(IEnumerable source) {
            this.m_itemSource = source;
            this.SelfOptions.ItemsSource = source;
        }

        private object GetSelectedValue() {
            if (this.State is SelfState) {
                return this.SelfOptions.SelectedItem;
            } else if (this.State is OtherState) {
                return this.m_setSelectedValue;
            } else {
                return "Unknown";
            }
        }

        private void SetSelectedValue(object value) {

            if (this.State is SelfState) {
                this.SelfOptions.SelectedIndex = this.SelfOptions.Items.IndexOf(value);
            } else if (this.State is OtherState) {
                this.OtherSelected.Content = this.m_setSelectedValue = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.OtherSelectedItem)));
            }
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedItem)));
        }

        private int GetSelectedIndex() {
            if (this.State is SelfState) {
                return this.SelfOptions.SelectedIndex;
            } else {
                return 0;
            }
        }

        private void SetSelectedIndex(int value) {
            if (this.State is SelfState) {
                this.SelfOptions.SelectedIndex = value;
            } else if (this.State is OtherState) {
                if (this.m_itemSource is not null && CountOfElements() > 0) {
                    this.SetSelectedValue(this.SelfOptions.Items[0]);
                } else {
                    Trace.WriteLine("Unable to update selected index for client (Item list empty)", "LobbyDropdown");
                }
            }
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SelectedIndex)));
        }

        private int CountOfElements() {
            if (this.m_itemSource is null) {
                return 0;
            }
            var e = this.m_itemSource.GetEnumerator();
            int cntr = 0;
            while (e.MoveNext()) {
                cntr++;
            }
            return cntr;
        }

    }

}
