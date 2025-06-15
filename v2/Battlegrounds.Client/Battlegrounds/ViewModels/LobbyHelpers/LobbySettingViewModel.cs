using Battlegrounds.Models.Lobbies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed record LobbySettingViewModel(LobbySetting Setting, IAsyncRelayCommand<LobbySetting> SettingChangeCommand) { // TODO: Make bindings use the Setting object directly instead of duplicating references
    public string Name => Setting.Name;
    public LobbySettingType Type => Setting.Type;

    public bool BoolValue {
        get => Setting.Value != 0;
        set {
            if (value == (Setting.Value != 0)) return;
            Setting.Value = value ? 1 : 0;
            SettingChangeCommand.Execute(Setting);
        }
    }

    public int IntValue {
        get => Setting.Value;
        set {
            if (value == Setting.Value) return;
            Setting.Value = Math.Clamp(value, Setting.MinValue, Setting.MaxValue);
            SettingChangeCommand.Execute(Setting);
        }
    }

    public int SelectedOptionIndex {
        get => Setting.Value;
        set {
            if (value == Setting.Value) return;
            if (Setting.Options != null && value >= 0 && value < Setting.Options.Length) {
                Setting.Value = value;
                SettingChangeCommand.Execute(Setting);
            }
        }
    }

    public LobbySettingOption? SelectedOption =>
        Setting.Options != null && Setting.Value >= 0 && Setting.Value < Setting.Options.Length
            ? Setting.Options[Setting.Value]
            : null;

    public LobbySettingOption[]? Options => Setting.Options;

    public int MinValue => Setting.MinValue;
    public int MaxValue => Setting.MaxValue;
    public int Step => Setting.Step;
}
