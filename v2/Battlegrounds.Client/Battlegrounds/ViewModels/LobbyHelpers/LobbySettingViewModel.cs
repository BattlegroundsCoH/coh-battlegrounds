using Battlegrounds.Models.Lobbies;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed class LobbySettingViewModel : ObservableObject {

    private readonly Func<LobbySetting, Task> _applySetting;
    private int _draftValue;

    public LobbySettingViewModel(LobbySetting setting, Func<LobbySetting, Task> applySetting) {
        Setting = setting;
        _applySetting = applySetting;
        _draftValue = setting.Value;
        ApplyCommand = new AsyncRelayCommand(ApplyAsync, () => IsDirty);
    }

    public LobbySetting Setting { get; }

    public IAsyncRelayCommand ApplyCommand { get; }

    public string Name => Setting.Name;
    public LobbySettingType Type => Setting.Type;

    public bool BoolValue => Setting.Value != 0;

    public bool DraftBoolValue {
        get => _draftValue != 0;
        set => SetDraftValue(value ? 1 : 0);
    }

    public int IntValue => Setting.Value;

    public int DraftIntValue {
        get => _draftValue;
        set => SetDraftValue(Math.Clamp(value, Setting.MinValue, Setting.MaxValue));
    }

    public int SelectedOptionIndex => Setting.Value;

    public int DraftSelectedOptionIndex {
        get => _draftValue;
        set {
            if (Setting.Options != null && value >= 0 && value < Setting.Options.Length) {
                SetDraftValue(value);
            }
        }
    }

    public LobbySettingOption? SelectedOption =>
        Setting.Options != null && Setting.Value >= 0 && Setting.Value < Setting.Options.Length
            ? Setting.Options[Setting.Value]
            : null;

    public bool IsDirty => _draftValue != Setting.Value;

    public LobbySettingOption[]? Options => Setting.Options;

    public int MinValue => Setting.MinValue;
    public int MaxValue => Setting.MaxValue;
    public int Step => Setting.Step;

    private void SetDraftValue(int value) {
        if (_draftValue == value) {
            return;
        }
        _draftValue = value;
        OnPropertyChanged(nameof(DraftBoolValue));
        OnPropertyChanged(nameof(DraftIntValue));
        OnPropertyChanged(nameof(DraftSelectedOptionIndex));
        OnPropertyChanged(nameof(IsDirty));
        ApplyCommand.NotifyCanExecuteChanged();
    }

    private Task ApplyAsync() {
        var requestedSetting = new LobbySetting {
            Name = Setting.Name,
            Priority = Setting.Priority,
            Value = _draftValue,
            Type = Setting.Type,
            Options = Setting.Options is null ? null : [.. Setting.Options],
            MinValue = Setting.MinValue,
            MaxValue = Setting.MaxValue,
            Step = Setting.Step
        };
        return _applySetting(requestedSetting);
    }

}
