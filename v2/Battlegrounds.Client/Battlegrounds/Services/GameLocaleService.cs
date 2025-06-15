using System.Diagnostics;
using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;

using Microsoft.Extensions.Logging;

using Serilog;

namespace Battlegrounds.Services;

public sealed class GameLocaleService : IGameLocaleService {

    private readonly Dictionary<uint, string> _localeStringsCoH3 = [];
    private readonly Dictionary<uint, string> _localeStringsCoH2 = [];

    private readonly Dictionary<Type, string> _identifiers = new() {
        { typeof(CoH3), CoH3.GameId },
    };

    public string Language { get; set; } = Consts.UCS_LANG_ENGLISH; // Default language

    public LocaleString FromGame<T>(uint key) where T : Game => _identifiers[typeof(T)] switch {
        CoH3.GameId => new LocaleString(key, ResolveCoH3),
        _ => throw new NotSupportedException($"Game {typeof(T).Name} is not supported for locale resolution."),
    };

    public async Task<bool> LoadLocalesAsync() { // TODO: Maybe move into a separate method for language selection

        LocaleParser localeParser = new(LoggerFactory.Create(x => x.AddSerilog()).CreateLogger<LocaleParser>());

        // Load CoH3 locale strings
        try {
            using var coh3localeStream = File.OpenRead("Assets/Factions/coh3/locale.yaml");
            var stopwatch = Stopwatch.StartNew();
            var coh3Locales = await localeParser.ParseLocalesAsync(coh3localeStream);
            if (!coh3Locales.TryGetValue(Language, out var coh3entries)) {
                coh3entries = coh3Locales[Consts.UCS_LANG_ENGLISH]; // Fallback to English if the requested language is not available
            }
            foreach (var (key, value) in coh3entries) {
                _localeStringsCoH3[key] = value;
            }
            stopwatch.Stop();
            Log.Information("Loaded {Count} CoH3 locale strings in {ElapsedMilliseconds} ms.", _localeStringsCoH3.Count, stopwatch.ElapsedMilliseconds);
        } catch (Exception ex) {
            Log.Error(ex, "Failed to load CoH3 locale strings.");
            return false;
        }

        // TODO: Load CoH2 locale strings if needed

        return true;

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
