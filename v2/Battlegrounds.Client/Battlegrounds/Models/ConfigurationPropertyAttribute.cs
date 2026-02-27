namespace Battlegrounds.Models;

public enum ConfigurationPropertyType {
    String,
    Integer,
    Boolean,
    FilePath,
    DirectoryPath,
    Selection
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ConfigurationPropertyAttribute(string name, string? description = null, string? tooltip = null, bool developerModeOnly = false, ConfigurationPropertyType propertyType = ConfigurationPropertyType.String) : Attribute {

    public string Name { get; } = name;

    public string? Description { get; } = description;

    public string? Tooltip { get; } = tooltip;

    public bool DeveloperModeOnly { get; } = developerModeOnly;

    public ConfigurationPropertyType PropertyType { get; } = propertyType;

    /// <summary>
    /// Gets or sets the available options for selection-type properties.
    /// </summary>
    public string[] Options { get; set; } = [];

}
