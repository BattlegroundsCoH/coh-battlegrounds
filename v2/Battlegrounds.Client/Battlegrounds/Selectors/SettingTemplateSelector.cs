using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Models.Lobbies;
using Battlegrounds.ViewModels;

namespace Battlegrounds.Selectors;

public sealed class SettingTemplateSelector : DataTemplateSelector {

    public DataTemplate BooleanSettingTemplate { get; set; } = null!;
    public DataTemplate IntegerSettingTemplate { get; set; } = null!;
    public DataTemplate SelectionSettingTemplate { get; set; } = null!;

    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
        if (item is LobbyViewModel.LobbySettingWrapper setting) {
            return setting.Setting.Type switch {
                LobbySettingType.Boolean => BooleanSettingTemplate,
                LobbySettingType.Integer => IntegerSettingTemplate,
                LobbySettingType.Selection => SelectionSettingTemplate,
                _ => base.SelectTemplate(item, container)!
            };
        }

        return base.SelectTemplate(item, container)!;
    }
}