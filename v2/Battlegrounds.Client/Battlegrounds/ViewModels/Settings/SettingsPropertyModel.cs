using System.ComponentModel;
using System.Reflection;

using Battlegrounds.Models;

namespace Battlegrounds.ViewModels.Settings;

public sealed class SettingsPropertyModel : INotifyPropertyChanged {

    private readonly PropertyInfo _propertyInfo;
    private readonly object _owner;
    private object? _value;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; }

    public string? Description { get; }

    public string? Tooltip { get; }

    public ConfigurationPropertyType PropertyType { get; }

    public string[] Options { get; }

    public object? Value {
        get => _value;
        set {
            if (Equals(_value, value))
                return;
            _value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    public SettingsPropertyModel(PropertyInfo propertyInfo, object owner, ConfigurationPropertyAttribute attr) {
        _propertyInfo = propertyInfo;
        _owner = owner;
        Name = attr.Name;
        Description = attr.Description;
        Tooltip = attr.Tooltip;

        // Resolve editor type: prefer attribute, but infer from CLR type when defaulted to String
        if (attr.PropertyType != ConfigurationPropertyType.String) {
            PropertyType = attr.PropertyType;
        } else if (propertyInfo.PropertyType == typeof(int)) {
            PropertyType = ConfigurationPropertyType.Integer;
        } else if (propertyInfo.PropertyType == typeof(bool)) {
            PropertyType = ConfigurationPropertyType.Boolean;
        } else {
            PropertyType = attr.PropertyType;
        }

        Options = attr.Options;
        _value = propertyInfo.GetValue(owner);
    }

    /// <summary>
    /// Writes the current <see cref="Value"/> back to the underlying configuration property.
    /// </summary>
    public void Apply() {
        var targetType = _propertyInfo.PropertyType;
        if (targetType == typeof(int)) {
            if (_value is int intVal) {
                _propertyInfo.SetValue(_owner, intVal);
            } else if (_value is string s && int.TryParse(s, out var parsed)) {
                _propertyInfo.SetValue(_owner, parsed);
            }
        } else if (targetType == typeof(bool)) {
            if (_value is bool b) {
                _propertyInfo.SetValue(_owner, b);
            }
        } else {
            _propertyInfo.SetValue(_owner, _value?.ToString() ?? string.Empty);
        }
    }

}
