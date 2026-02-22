using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using Battlegrounds.Models;
using Battlegrounds.Models.Statistics;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

public sealed class StatisticsService(Configuration configuration, ILogger<StatisticsService> logger) : IStatisticsService {

    private readonly Configuration _configuration = configuration;
    private readonly ILogger<StatisticsService> _logger = logger;

    private readonly TaskCompletionSource _loadCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly RecordStore<MatchPlayed> _matchesPlayedStore = new("matches_played");

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

    public Task RegisterPlayedMatchAsync(MatchPlayed match) {
        throw new NotImplementedException();
    }

    private class RecordStore<T>(string id, Predicate<T>? retentionPredicate = null) {

        private IList<T> _records = [];

        public async Task LoadStore(string storePath) {
            string filePath = Path.Combine(storePath, $"{id}.json");
            if (!File.Exists(filePath)) {
                return;
            }
            using var stream = File.OpenRead(filePath);
            _records = await JsonSerializer.DeserializeAsync<List<T>>(stream, Configuration.JsonSerializerOptions) ?? throw new InvalidDataException($"Failed to deserialize data from {filePath}");
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

    }

}
