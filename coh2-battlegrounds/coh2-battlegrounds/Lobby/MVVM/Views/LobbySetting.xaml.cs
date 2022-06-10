using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Lobby.MVVM.Views;

/// <summary>
/// Interaction logic for LobbySetting.xaml
/// </summary>
public partial class LobbySetting : UserControl {
    
    public int Selected {
        get => this.DropdownOptions.SelectedIndex;
        set => this.DropdownOptions.SelectedIndex = value;
    }

    public object Label {
        get => this.ParticipantValue.Content;
        set => this.ParticipantValue.Content = value;
    }

    public Action<int,int>? UpdateIndex { get; set; }

    public LobbySetting() {
        this.InitializeComponent();
    }

    public void ShowDropdown(object[] options) {

        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Visible;
        this.SliderContainer.Visibility = Visibility.Collapsed;
        this.ParticipantValue.Visibility = Visibility.Collapsed;

    }

    public void ShowSlider(int min, int max, int step, string format) {

        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Collapsed;
        this.SliderContainer.Visibility = Visibility.Visible;
        this.ParticipantValue.Visibility = Visibility.Collapsed;

    }

    public void ShowValue() {
        
        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Collapsed;
        this.SliderContainer.Visibility = Visibility.Collapsed;
        this.ParticipantValue.Visibility = Visibility.Visible;


    }

}

public class LobbySetting<T> : LobbySetting {

    public ObservableCollection<T> Items { get; init; }

    public LobbySetting() {
        this.Items = new();
        this.DropdownOptions.ItemsSource = this.Items;
    }

    public static LobbySetting<T> NewDropdown(string settingName, ObservableCollection<T> options, Action<int, int> changedCallback) {
        var setting = new LobbySetting<T>() {
            Items = options,
            UpdateIndex = changedCallback
        };
        setting.SettingName.LocKey = settingName;
        setting.DropdownOptions.ItemsSource = setting.Items;
        return setting;
    }

    public static LobbySetting<T> NewSlider(string settingName, int min, int max, int step, string format) {
        var setting = new LobbySetting<T>();
        setting.SettingName.LocKey = settingName;
        setting.ShowSlider(min, max, step, format);
        return setting;
    }

    public static LobbySetting<T> NewValue(string settingName, string value) {
        var setting = new LobbySetting<T>();
        setting.SettingName.LocKey = settingName;
        setting.ShowValue();
        return setting;
    }

}
