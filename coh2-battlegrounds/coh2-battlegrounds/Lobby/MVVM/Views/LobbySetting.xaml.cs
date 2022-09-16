using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using StringOrKey = Battlegrounds.Functional.Either<string, Battlegrounds.Locale.LocaleKey>;

namespace BattlegroundsApp.Lobby.MVVM.Views;

public delegate void SettingChanged(int newIndex, int oldIndex);

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

    public string Format {
        get;
        set;
    }

    public SettingChanged? UpdateIndex { get; set; }

    public LobbySetting() {
        this.InitializeComponent();
        this.Format = string.Empty;
    }

    public void ShowDropdown() {

        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Visible;
        this.SliderContainer.Visibility = Visibility.Collapsed;
        this.ParticipantValue.Visibility = Visibility.Collapsed;

        // Register dropdown
        this.DropdownOptions.SelectionChanged += this.DropdownOptions_SelectionChanged;

    }

    private void DropdownOptions_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        this.UpdateIndex?.Invoke(this.Selected, -1);
    }

    public void ShowSlider(int min, int max, int step, string format) {

        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Collapsed;
        this.SliderContainer.Visibility = Visibility.Visible;
        this.ParticipantValue.Visibility = Visibility.Collapsed;

        // Set slider
        this.SliderValue.Minimum = min;
        this.SliderValue.Maximum = max;
        this.SliderValue.TickFrequency = step;
        this.SliderValue.TickPlacement = TickPlacement.None;

        // Set format str
        this.Format = format;

        // Update text value
        this.SliderTextValue.Content = string.Format(format, min);

    }

    public void ShowValue() {
        
        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Collapsed;
        this.SliderContainer.Visibility = Visibility.Collapsed;
        this.ParticipantValue.Visibility = Visibility.Visible;

        // Show formatted value
        if (!string.IsNullOrEmpty(this.Format)) {
            this.Label = string.Format(this.Format, this.Label);
        }

    }

    private void SliderValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

        // Cast to int
        int num = (int)e.NewValue;

        // Report value
        this.UpdateIndex?.Invoke(num, -1);
        
        // Update text value
        this.SliderTextValue.Content = string.Format(this.Format, num);

    }

}

public class LobbySetting<T> : LobbySetting {

    public ObservableCollection<T> Items { get; init; }

    public LobbySetting() {
        this.Items = new();
        this.DropdownOptions.ItemsSource = this.Items;
    }

    public static LobbySetting<T> NewDropdown(StringOrKey settingName, ObservableCollection<T> options, SettingChanged? changedCallback = null, int defaultIndex = -2) {
        var setting = new LobbySetting<T>() {
            Items = options,
            UpdateIndex = changedCallback
        };
        setting.SettingName.LocKey = settingName;
        setting.DropdownOptions.ItemsSource = setting.Items;
        setting.ShowDropdown();
        if (defaultIndex != -2) {
            setting.DropdownOptions.SelectedIndex = defaultIndex;
        }
        return setting;
    }

    public static LobbySetting<T> NewSlider(StringOrKey settingName, int min, int max, int step, string format, SettingChanged? changedCallback = null) {
        var setting = new LobbySetting<T>() {
            UpdateIndex = changedCallback
        };
        setting.SettingName.LocKey = settingName;
        setting.ShowSlider(min, max, step, format);
        return setting;
    }

    public static LobbySetting<T> NewValue(StringOrKey settingName, string value, object? tag = null, string format = "") {
        var setting = new LobbySetting<T>() {
            Tag = tag,
            Label = value,
            Format = format
        };
        setting.SettingName.LocKey = settingName;
        setting.ShowValue();
        return setting;
    }

}
