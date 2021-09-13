using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using BattlegroundsApp.Lobby.MVVM.Models;

namespace BattlegroundsApp.Lobby.MVVM.Views {

    /// <summary>
    /// Interaction logic for LobbyPlayerSlot.xaml
    /// </summary>
    public partial class LobbyPlayerSlot : UserControl {

        public LobbySlot SlotData {
            get => this.GetValue(SlotDataProperty) as LobbySlot;
            set => this.SetValue(SlotDataProperty, value);
        }

        public static readonly DependencyProperty SlotDataProperty
            = DependencyProperty.Register(nameof(SlotData), typeof(LobbySlot), typeof(LobbyPlayerSlot), new PropertyMetadata(null, OnSlotDataPropertyChanged));

        public ObservableCollection<LobbyCompanyItem> CompanyItemsSource {
            get => this.GetValue(CompanyItemsSourceProperty) as ObservableCollection<LobbyCompanyItem>;
            set => this.SetValue(CompanyItemsSourceProperty, value);
        }

        public static readonly DependencyProperty CompanyItemsSourceProperty
            = DependencyProperty.Register(nameof(CompanyItemsSource),
                typeof(ObservableCollection<LobbyCompanyItem>),
                typeof(LobbyPlayerSlot),
                new PropertyMetadata(new ObservableCollection<LobbyCompanyItem>()));

        private static void OnSlotDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            LobbyPlayerSlot self = d as LobbyPlayerSlot;
            self.SlotData = e.NewValue as LobbySlot;
            self.rootObj.DataContext = self.SlotData;
            self.SlotContextMenuObj.DataContext = self.SlotData.SlotContextMenu;
        }

        public LobbyPlayerSlot() => this.InitializeComponent(); // Init component

    }

}
