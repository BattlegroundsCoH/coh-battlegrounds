using System.IO;
using System.Text.Json;

using Battlegrounds.Models;
using Battlegrounds.Models.Statistics;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

public sealed class StatisticsService(Configuration configuration, ILogger<StatisticsService> logger) : IStatisticsService {

    private readonly Configuration _configuration = configuration;
    private readonly ILogger<StatisticsService> _logger = logger;

    private readonly TaskCompletionSource _loadCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly RecordStore<MatchPlayed> _matchesPlayedStore = new("matches_played", logger);

    public Task IsLoaded => _loadCompletionSource.Task;

    public IReadOnlyList<MatchPlayed> GetPlayedMatches() => _matchesPlayedStore.GetRecords();

    public async Task LoadStatisticsAsync() {

        if (!Directory.Exists(_configuration.StatisticsPath)) {
            _logger.LogInformation("Statistics data directory does not exist. Creating new directory at {Path}", _configuration.StatisticsPath);
            Directory.CreateDirectory(_configuration.StatisticsPath);
        }

        await _matchesPlayedStore.LoadStore(_configuration.StatisticsPath);

        _loadCompletionSource.SetResult();

    }

    public async Task RegisterPlayedMatchAsync(MatchPlayed match) {
        _matchesPlayedStore.AddRecord(match);
        await _matchesPlayedStore.SaveStore(_configuration.StatisticsPath); // Immediately save after adding a new record to ensure data is not lost in case of a crash.
    }

    private class RecordStore<T>(string id, ILogger<StatisticsService> logger, Predicate<T>? retentionPredicate = null) {

        private readonly ILogger<StatisticsService> _log = logger;
        private readonly IList<T> _records = [];

        public async Task LoadStore(string storePath) {
            string filePath = Path.Combine(storePath, $"{id}.json");
            if (!File.Exists(filePath)) {
                return;
            }
            try {
                using var stream = File.OpenRead(filePath);
                var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, Configuration.JsonSerializerOptions) ?? throw new InvalidDataException($"Failed to deserialize data from {filePath}");
                _records.Clear();
                foreach (var record in data) {
                    _records.Add(record);
                }
            } catch (Exception ex) { 
                _log.LogError(ex, "Failed to load data for store {StoreId} from file {FilePath}", id, filePath);
            }
        }

        public async Task SaveStore(string storePath) {
            var toStore = _records;
            if (retentionPredicate is not null) {
                toStore = [.. _records.Where(r => retentionPredicate(r))];
            }
            string filePath = Path.Combine(storePath, $"{id}.json");
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, toStore, Configuration.JsonSerializerOptions);
        }

        public IReadOnlyList<T> GetRecords() => _records.AsReadOnly();

        public void AddRecord(T record) {
            _records.Add(record);
        }

    }

}
