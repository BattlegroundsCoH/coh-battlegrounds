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

        private static void OnSlotDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            LobbyPlayerSlot self = d as LobbyPlayerSlot;
            self.SlotData = e.NewValue as LobbySlot;
            self.rootObj.DataContext = self.SlotData;
        }

        public LobbyPlayerSlot() => this.InitializeComponent(); // Init component

    }

}
