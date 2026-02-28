using System.Diagnostics;
using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

public sealed class GameLocaleService(ILogger<GameLocaleService> logger) : IGameLocaleService {

    private readonly ILogger<GameLocaleService> _logger = logger;

    private Dictionary<uint, string> _localeStringsCoH3 = [];
    private Dictionary<uint, string> _localeStringsCoH2 = [];

    private readonly Dictionary<Type, string> _identifiers = new() {
        { typeof(CoH3), CoH3.GameId },
    };

    public string Language { get; set; } = Consts.UCS_LANG_ENGLISH; // Default language

    public LocaleString FromGame<T>(uint key) where T : Game => _identifiers[typeof(T)] switch {
        CoH3.GameId => new LocaleString(key, ResolveCoH3),
        _ => throw new NotSupportedException($"Game {typeof(T).Name} is not supported for locale resolution."),
    };

    public async Task<bool> LoadLocalesAsync() { // TODO: Maybe move into a separate method for language selection

        LocaleParser localeParser = new();

        // Load CoH3 locale strings
        try {
            var stopwatch = Stopwatch.StartNew();
            await ParseResourceAsync(localeParser, "Assets/Factions/coh3/locale.yaml", _localeStringsCoH3);
            await ParseResourceAsync(localeParser, "Assets/Scenarios/coh3/locale.yaml", _localeStringsCoH3);
            stopwatch.Stop();
            _logger.LogInformation("Loaded {Count} CoH3 locale strings in {ElapsedMilliseconds} ms.", _localeStringsCoH3.Count, stopwatch.ElapsedMilliseconds);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to load CoH3 locale strings.");
            return false;
        }

        // TODO: Load CoH2 locale strings if needed

        return true;

    }

    private async Task ParseResourceAsync(LocaleParser localeParser, string resource, Dictionary<uint, string> targetDic) {
        if (!File.Exists(resource)) {
            _logger.LogWarning("Locale resource file not found: {Resource}", resource);
            return;
        }
        using var coh3localeStream = File.OpenRead(resource);
        var coh3Locales = await localeParser.ParseLocalesAsync(coh3localeStream);
        if (!coh3Locales.TryGetValue(Language, out var coh3entries)) {
            if (!coh3Locales.ContainsKey(Consts.UCS_LANG_ENGLISH)) {
                _logger.LogWarning("Requested language '{Language}' and fallback language '{FallbackLanguage}' not found in locale resource: {Resource}", Language, Consts.UCS_LANG_ENGLISH, resource);
                return;
            }
            coh3entries = coh3Locales[Consts.UCS_LANG_ENGLISH]; // Fallback to English if the requested language is not available
        }
        foreach (var entry in coh3entries) {
            targetDic[entry.Key] = entry.Value;
        }
    }

    public string ResolveLocaleString<T>(uint key, params object[] args) where T : Game {
        string gameId = _identifiers[typeof(T)];
        return gameId switch {
            CoH3.GameId => ResolveCoH3(key, args),
            _ => throw new NotSupportedException($"Game {gameId} is not supported for locale resolution."),
        };
    }

    private string ResolveCoH3(uint key, params object[] args) {
        if (_localeStringsCoH3.TryGetValue(key, out var value)) {
            return string.Format(value, args);
        }
        return $"${key} No Range";
    }

}
