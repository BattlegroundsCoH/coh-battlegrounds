using Battlegrounds.Core.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.App;

public class AppLoader(ILogger<AppLoader> logger, IScenarioService scenarioService) {

    private readonly ILogger<AppLoader> _logger = logger;
    private readonly IScenarioService _scenarioService = scenarioService;

    public async Task LoadAppDataAsync() {

        _logger.LogInformation("Loading scenarios");
        if (!await LoadScenarios()) {
            return;
        }
        _logger.LogInformation("Successfully loaded scenarios");

    }

    private async Task<bool> LoadScenarios() {

        var coh3_scenarios = await FileSystem.OpenAppPackageFileAsync("data/coh3/scenarios.yml");
        if (!await _scenarioService.LoadFromStreamAsync(coh3_scenarios)) {
            _logger.LogCritical("Failed loading scenario from source");
            return false;
        }

        return true;

    }
    
}
