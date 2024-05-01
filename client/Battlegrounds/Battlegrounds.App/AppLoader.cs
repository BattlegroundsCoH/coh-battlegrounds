using Battlegrounds.Core.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.App;

public class AppLoader(ILogger<AppLoader> logger, IBlueprintService blueprintService, IScenarioService scenarioService) {

    private readonly ILogger<AppLoader> _logger = logger;
    private readonly IScenarioService _scenarioService = scenarioService;
    private readonly IBlueprintService _blueprintprintService = blueprintService;

    public async Task LoadAppDataAsync() {

        _logger.LogInformation("Loading blueprints");
        if (!await LoadBlueprints()) {
            return;
        }
        _logger.LogInformation("Successfully loading blueprints");

        _logger.LogInformation("Loading scenarios");
        if (!await LoadScenarios()) {
            return;
        }
        _logger.LogInformation("Successfully loaded scenarios");

    }

    private async Task<bool> LoadBlueprints() {

        var loaded = true;

        var coh3_blueprints = await FileSystem.OpenAppPackageFileAsync("data/coh3/blueprints.yml");
        if (!await _blueprintprintService.LoadBlueprintsFromStream(coh3_blueprints)) {
            _logger.LogCritical("Failed loading blueprints for Company of Heroes 3");
            loaded = false;
        }

        return loaded;

    }

    private async Task<bool> LoadScenarios() {

        var loaded = true;

        var coh3_scenarios = await FileSystem.OpenAppPackageFileAsync("data/coh3/scenarios.yml");
        if (!await _scenarioService.LoadFromStreamAsync(coh3_scenarios)) {
            _logger.LogCritical("Failed loading scenario for Company of Heroes 3");
            loaded = false;
        }

        return loaded;

    }
    
}
