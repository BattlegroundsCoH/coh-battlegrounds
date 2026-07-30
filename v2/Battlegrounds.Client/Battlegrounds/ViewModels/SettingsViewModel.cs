using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

using Battlegrounds.Models;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.Settings;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed class SettingsViewModel {

    private static bool IsDeveloperMode =>
#if DEBUG
        true;
#else
        false;
#endif

    private readonly Configuration _configuration;
    private readonly BattlegroundsApp _app;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly IUiScaleService _uiScaleService;
    private readonly ILogger<SettingsViewModel> _logger;

    public ObservableCollection<SettingsSectionModel> Sections { get; } = [];

    /// <summary>
    /// The signed-in user, surfaced here because the Account section is the only place the app
    /// shows identity now that the top bar is gone.
    /// </summary>
    public UserViewModel UserViewModel => _mainWindowViewModel.UserViewModel;

    public IRelayCommand SaveCommand { get; }

    public IAsyncRelayCommand LogoutCommand => _mainWindowViewModel.LogoutCommand;

    public SettingsViewModel(Configuration configuration, BattlegroundsApp app, MainWindowViewModel mainWindowViewModel, IUiScaleService uiScaleService, ILogger<SettingsViewModel> logger) {
        _configuration = configuration;
        _app = app;
        _mainWindowViewModel = mainWindowViewModel;
        _uiScaleService = uiScaleService;
        _logger = logger;

        SaveCommand = new RelayCommand(Save);

        BuildSections();
    }

    private void BuildSections() {

        Sections.Clear();

        var sectionMap = new Dictionary<string, SettingsSectionModel>();

        foreach (var property in typeof(Configuration).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {

            // Check if the property type is a nested configuration class with a section attribute
            var typeSectionAttr = property.PropertyType.GetCustomAttribute<ConfigurationSectionAttribute>();
            if (typeSectionAttr is not null) {

                if (!typeSectionAttr.IsVisible)
                    continue;

                if (typeSectionAttr.DeveloperModeOnly && !IsDeveloperMode)
                    continue;

                var nestedObj = property.GetValue(_configuration);
                if (nestedObj is null)
                    continue;

                var section = GetOrCreateSection(sectionMap, typeSectionAttr.Name, typeSectionAttr.Description, typeSectionAttr.Priority);
                AddPropertiesFromObject(section, nestedObj);
                continue;
            }

            // Check if the property itself has ConfigurationSection + ConfigurationProperty (flat property)
            var propertySectionAttr = property.GetCustomAttribute<ConfigurationSectionAttribute>();
            var propertyConfigAttr = property.GetCustomAttribute<ConfigurationPropertyAttribute>();
            if (propertySectionAttr is not null && propertyConfigAttr is not null) {

                if (!propertySectionAttr.IsVisible)
                    continue;

                if (propertySectionAttr.DeveloperModeOnly && !IsDeveloperMode)
                    continue;

                if (propertyConfigAttr.DeveloperModeOnly && !IsDeveloperMode)
                    continue;

                var section = GetOrCreateSection(sectionMap, propertySectionAttr.Name, propertySectionAttr.Description, propertySectionAttr.Priority);
                section.Properties.Add(new SettingsPropertyModel(property, _configuration, propertyConfigAttr));
            }

        }

        var sorted = sectionMap.Values.OrderBy(s => s.Priority).ToList();
        Sections.Clear();
        foreach (var s in sorted)
            Sections.Add(s);

    }

    private static void AddPropertiesFromObject(SettingsSectionModel section, object owner) {
        foreach (var prop in owner.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            var attr = prop.GetCustomAttribute<ConfigurationPropertyAttribute>();
            if (attr is null)
                continue;
            if (attr.DeveloperModeOnly && !IsDeveloperMode)
                continue;
            section.Properties.Add(new SettingsPropertyModel(prop, owner, attr));
        }
    }

    private static SettingsSectionModel GetOrCreateSection(Dictionary<string, SettingsSectionModel> map, string name, string description, int priority) {
        if (!map.TryGetValue(name, out var section)) {
            section = new SettingsSectionModel(name, description, priority);
            map[name] = section;
        }
        return section;
    }

    private void Save() {
        foreach (var section in Sections) {
            foreach (var property in section.Properties) {
                property.Apply();
            }
        }
        _app.SaveConfiguration();

        // Applied after the write so the persisted value and the live UI cannot disagree. This is a
        // no-op when the scale did not change, and takes effect without a restart because the theme
        // layer references its size tokens with {DynamicResource}.
        _uiScaleService.Apply(_configuration.UiScale);

        _logger.LogInformation("Configuration saved successfully.");
    }

}
