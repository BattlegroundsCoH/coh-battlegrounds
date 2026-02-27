using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Models;
using Battlegrounds.ViewModels.Settings;

namespace Battlegrounds.Selectors;

public sealed class ConfigurationPropertyTemplateSelector : DataTemplateSelector {

    public DataTemplate StringTemplate { get; set; } = null!;

    public DataTemplate IntegerTemplate { get; set; } = null!;

    public DataTemplate BooleanTemplate { get; set; } = null!;

    public DataTemplate FilePathTemplate { get; set; } = null!;

    public DataTemplate DirectoryPathTemplate { get; set; } = null!;

    public DataTemplate SelectionTemplate { get; set; } = null!;

    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
        if (item is SettingsPropertyModel property) {
            return property.PropertyType switch {
                ConfigurationPropertyType.Integer => IntegerTemplate,
                ConfigurationPropertyType.Boolean => BooleanTemplate,
                ConfigurationPropertyType.FilePath => FilePathTemplate,
                ConfigurationPropertyType.DirectoryPath => DirectoryPathTemplate,
                ConfigurationPropertyType.Selection => SelectionTemplate,
                _ => StringTemplate,
            };
        }
        return base.SelectTemplate(item, container)!;
    }

}
