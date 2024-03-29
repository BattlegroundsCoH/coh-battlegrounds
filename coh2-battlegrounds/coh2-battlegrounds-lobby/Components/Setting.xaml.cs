﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Battlegrounds.Functional;
using Battlegrounds.Locale;

namespace Battlegrounds.Lobby.Components;

using StringOrKey = Either<string, LocaleKey>;

/// <summary>
/// 
/// </summary>
/// <param name="newIndex"></param>
/// <param name="oldIndex"></param>
/// <param name="value"></param>
public delegate void SettingChanged(int newIndex, int oldIndex, object value);

/// <summary>
/// Interaction logic for Setting.xaml
/// </summary>
public partial class Setting : UserControl {

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

    public Setting() {
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
        this.UpdateIndex?.Invoke(this.Selected, -1, this.Selected);
    }

    public void ShowSlider(int min, int max, int val, int step, string format) {

        // Set visibilities
        this.DropdownOptions.Visibility = Visibility.Collapsed;
        this.SliderContainer.Visibility = Visibility.Visible;
        this.ParticipantValue.Visibility = Visibility.Collapsed;

        // Set slider
        this.SliderValue.Minimum = min;
        this.SliderValue.Maximum = max;
        this.SliderValue.Value = val;
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
        this.UpdateIndex?.Invoke(num, -1, this.SliderValue.Value);

        // Update text value
        this.SliderTextValue.Content = string.Format(this.Format, num);

    }

}

public sealed class Setting<T> : Setting {

    public ObservableCollection<T> Items { get; init; }

    public Setting() {
        this.Items = new();
        this.DropdownOptions.ItemsSource = this.Items;
    }

    public static Setting<T> NewDropdown(StringOrKey settingName, ObservableCollection<T> options, SettingChanged? changedCallback = null, int defaultIndex = -2) {
        var setting = new Setting<T>() {
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

    public static Setting<T> NewSlider(StringOrKey settingName, int min, int max, int step, int def, string format, SettingChanged? changedCallback = null) {
        var setting = new Setting<T>() {
            UpdateIndex = changedCallback,
        };
        setting.SettingName.LocKey = settingName;
        setting.ShowSlider(min, max, def, step, format);
        return setting;
    }

    public static Setting<T> NewValue(StringOrKey settingName, string value, object? tag = null, string format = "") {
        var setting = new Setting<T>() {
            Tag = tag,
            Label = value,
            Format = format
        };
        setting.SettingName.LocKey = settingName;
        setting.ShowValue();
        return setting;
    }

}
