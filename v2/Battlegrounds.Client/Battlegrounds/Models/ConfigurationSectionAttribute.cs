namespace Battlegrounds.Models;

/// <summary>
/// Specifies metadata for a configuration section or property, including its display name, description, and visibility
/// settings.
/// </summary>
/// <remarks>Apply this attribute to a class or property to provide additional metadata for configuration
/// management tools and user interfaces. Use the visibility settings to control which sections are exposed to end users
/// or restricted to developers.</remarks>
/// <param name="name">The unique name of the configuration section or property. This value is used to identify the section within the
/// configuration system and must not be null or empty.</param>
/// <param name="description">A brief description of the configuration section or property. This text is typically displayed in user interfaces or
/// documentation to help users understand the purpose of the section.</param>
/// <param name="isVisible">A value indicating whether the configuration section or property is visible in standard configuration tools. The
/// default is <see langword="true"/>.</param>
/// <param name="developerModeOnly">A value indicating whether the configuration section or property should only be visible when the application is
/// running in developer mode. The default is <see langword="false"/>.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class ConfigurationSectionAttribute(string name, string description, bool isVisible = true, bool developerModeOnly = false, int priority = int.MaxValue) : Attribute {

    /// <summary>
    /// Gets the name associated with this instance.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the description associated with this instance.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// Gets a value indicating whether the current object is visible.
    /// </summary>
    public bool IsVisible { get; } = isVisible;

    /// <summary>
    /// Gets a value indicating whether the feature or setting is intended for use only in developer mode.
    /// </summary>
    /// <remarks>Use this property to determine if the associated functionality should be exposed only in
    /// development environments. This can help prevent accidental use in production scenarios.</remarks>
    public bool DeveloperModeOnly { get; } = developerModeOnly;

    /// <summary>
    /// Gets the display priority of this section. Lower values appear first. Defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    public int Priority { get; } = priority;

}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigurationIncludeAttribute : Attribute {}
