using Battlegrounds.Models;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class GameLocaleService : IGameLocaleService {

    private readonly Dictionary<uint, string> _localeStringsCoH3 = [];
    private readonly Dictionary<uint, string> _localeStringsCoH2 = [];

    private readonly Dictionary<Type, string> _identifiers = new() {
        { typeof(CoH3), CoH3.GameId },
    };

    public LocaleString FromGame<T>(uint key) where T : Game => _identifiers[typeof(T)] switch {
        CoH3.GameId => new LocaleString(key, ResolveCoH3),
        _ => throw new NotSupportedException($"Game {typeof(T).Name} is not supported for locale resolution."),
    };

    public Task<bool> LoadLocalesAsync() {
        // Mock for now
        _localeStringsCoH3[11153223] = "Infantry Section";
        _localeStringsCoH3[11265170] = "Long description for infantry section, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11266227] = "Good vs Infantry";
        return Task.FromResult(true);
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
