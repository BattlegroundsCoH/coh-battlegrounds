namespace Battlegrounds.ViewModels.Settings;

public sealed class SettingsSectionModel(string name, string description, int priority = int.MaxValue) {

    public string Name { get; } = name;

    public string Description { get; } = description;

    public int Priority { get; } = priority;

    public List<SettingsPropertyModel> Properties { get; } = [];

}
