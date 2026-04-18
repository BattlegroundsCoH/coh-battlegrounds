using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Doctrines;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

public sealed class DoctrineService(IBlueprintService blueprintService, Configuration configuration, ILogger<DoctrineService> logger) : IDoctrineService {

    private readonly IBlueprintService _blueprintService = blueprintService;
    private readonly Configuration _configuration = configuration;
    private readonly ILogger<DoctrineService> _logger = logger;

    private readonly Dictionary<string, DoctrineDefinition> _doctrines = [];
    private readonly TaskCompletionSource _loadCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task DoctrinesLoaded => _loadCompletionSource.Task;

    public DoctrineDefinition GetDoctrineById(string identifier) => _doctrines[identifier];

    public IEnumerable<DoctrineDefinition> GetDoctrinesForFaction(string gameId, string faction) {
        return from doctrine in _doctrines.Values
               where doctrine.Faction == faction
               select doctrine;
    }

    public async Task<int> LoadDoctrines(CancellationToken cancellationToken) {
        _logger.LogInformation("Loading doctrines...");

        try {

            string coh3DocumentsPath = EnsureCoH3DoctrinePathExists();
            string[] coh3DoctrineDefinitions = Directory.GetFiles(coh3DocumentsPath, "*.yaml", SearchOption.AllDirectories);
            if (coh3DoctrineDefinitions.Length is 0) {
                _logger.LogInformation("No CoH3 doctrine definitions found in path: {Path}", coh3DocumentsPath);
                coh3DoctrineDefinitions = Directory.GetFiles("Assets/Factions/coh3/doctrines", "*.yaml", SearchOption.AllDirectories);
                for (int i = 0; i < coh3DoctrineDefinitions.Length; i++) {
                    string sourceFile = coh3DoctrineDefinitions[i];
                    string targetFile = Path.Combine(coh3DocumentsPath, Path.GetFileName(sourceFile));
                    File.Copy(sourceFile, targetFile, overwrite: true);
                    _logger.LogInformation("Copied default doctrine definition from {Source} to {Target}", sourceFile, targetFile);
                    coh3DoctrineDefinitions[i] = targetFile;
                }
            }

            var doctrineParser = new DoctrineParser<CoH3>(_blueprintService, this);
            for (int i = 0; i < coh3DoctrineDefinitions.Length; i++) {
                string doctrineFile = coh3DoctrineDefinitions[i];
                try {
                    var doctrineDefinition = await doctrineParser.ParseDoctrineAsync(doctrineFile, _blueprintService, cancellationToken);
                    if (doctrineDefinition is null) {
                        _logger.LogWarning("Failed to load doctrine definition from file: {File}", doctrineFile);
                        continue;
                    }
                    _logger.LogInformation("Loaded doctrine definition: {Id} (Version {Version})", doctrineDefinition.Id, doctrineDefinition.Version);
                    if (_doctrines.ContainsKey(doctrineDefinition.Id)) {
                        _logger.LogWarning("Doctrine definition with ID {Id} already exists. Skipping file {File}.", doctrineDefinition.Id, doctrineFile);
                        continue;
                    }
                    _doctrines[doctrineDefinition.Id] = doctrineDefinition;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Failed to load doctrine definition from file: {File}", doctrineFile);
                }
            }

        } catch (IOException ioex) {
            _logger.LogError(ioex, "Failed to load doctrines due to an IO error.");

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to load doctrines due to an unexpected error.");
        }

        return _doctrines.Count;
    
    }

    private string EnsureCoH3DoctrinePathExists() {
        string path = Path.Combine(_configuration.DoctrinesPath, "coh3");
        try {

            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to ensure CoH3 doctrine path exists.");
        }
        return path;
    }

}
