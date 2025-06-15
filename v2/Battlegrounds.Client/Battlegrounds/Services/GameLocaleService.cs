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
        _localeStringsCoH3[11266227] = "Good vs Infantry"; // brief-text
        _localeStringsCoH3[11241690] = "Medic";
        _localeStringsCoH3[11189891] = "Long description for medic, which should also be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11158242] = "Heals infantry"; // brief-text
        _localeStringsCoH3[11200128] = "Australian Infantry Section";
        _localeStringsCoH3[11265170] = "Long description for Australian Infantry Section, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11248817] = "Good vs Infantry"; // brief-text
        _localeStringsCoH3[11241688] = "Canadian Shock Section";
        _localeStringsCoH3[11241902] = "Long description for Canadian Shock Troops, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11248818] = "Good vs Infantry"; // brief-text
        _localeStringsCoH3[11222386] = "Commandos";
        _localeStringsCoH3[11265170] = "Long description for Commandos, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11265805] = "Good vs Infantry"; // brief-text
        _localeStringsCoH3[11180063] = "Foot Guards Troops";
        _localeStringsCoH3[11265170] = "Long description for Guards Troops, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11266224] = "Good vs Infantry, Anti-Tank capabilities"; // brief-text
        _localeStringsCoH3[11184154] = "Ghurka Rifles"; // Ghurka or Gurkha Rifles?
        _localeStringsCoH3[11265170] = "Long description for Ghurka Rifles, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11266235] = "Good vs Infantry"; // brief-text
        _localeStringsCoH3[11189629] = "Thompson Submachine Gun Package";
        _localeStringsCoH3[11242613] = "Long description for Thompson Submachine Gun Package, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11242884] = "Good vs Infantry"; // brief-text
        _localeStringsCoH3[11153222] = "Royal Engineers";
        _localeStringsCoH3[11265170] = "Long description for Royal Engineers, which should be very long and detailed to test the formatting and length handling in the UI.";
        _localeStringsCoH3[11266413] = "Can repair vehicles"; // brief-text
        _localeStringsCoH3[11206886] = "Flamethrower Package";
        _localeStringsCoH3[11154006] = "Utility Package";
        _localeStringsCoH3[11242522] = "Piat Package";
        _localeStringsCoH3[11241693] = "2-Pounder Anti-Tank Gun";
        _localeStringsCoH3[11241694] = "6-Pounder Anti-Tank Gun";
        _localeStringsCoH3[11241691] = "17-Pounder Anti-Tank Gun";
        _localeStringsCoH3[11180060] = "Vickers Machine Gun Team";
        _localeStringsCoH3[11241696] = "81mm Mortar Team";
        _localeStringsCoH3[11239092] = "ML 4.2 Inch Mortar Team";
        _localeStringsCoH3[11241712] = "75mm Pack Howitzer Team";
        _localeStringsCoH3[11202605] = "CWT-15 Truck";
        _localeStringsCoH3[11242536] = "Quad Mount .50 Caliber";
        _localeStringsCoH3[11242533] = "Medical Truck";
        _localeStringsCoH3[11180067] = "Dingo Scout Car"; // And for some reason there is no Univeral Carrier in coh3...
        _localeStringsCoH3[11189140] = "Archer Tank Destroyer";
        _localeStringsCoH3[11241698] = "Bishop Self-Propelled Gun";
        _localeStringsCoH3[11199787] = "Centaur Tank";
        _localeStringsCoH3[11184728] = "Black Prince Heavy Tank";
        _localeStringsCoH3[11216435] = "Churchill Crocodile Flame Tank";
        _localeStringsCoH3[11185284] = "Churchill Tank";
        _localeStringsCoH3[11189924] = "Crusader Mk. III Tank";
        _localeStringsCoH3[11180065] = "Crusader Mk. II Tank"; // Crusader Mk II Tank
        _localeStringsCoH3[11199758] = "75mm Churchill Tank"; // 75mm Churchill Mk VII Tank
        _localeStringsCoH3[11184735] = "Crusader Mk. II AA Tank";
        _localeStringsCoH3[11180068] = "Mk. 3 Grant Tank";
        _localeStringsCoH3[11180069] = "Humber Armoured Car";
        _localeStringsCoH3[11189939] = "M4A1 Sherman Tank";
        _localeStringsCoH3[11242531] = "50. Caliber M2 Browning Machine Gun Package";
        _localeStringsCoH3[11180071] = "M3 Stuart Light Tank";
        _localeStringsCoH3[11242534] = "Utility Kit";
        _localeStringsCoH3[11186706] = "Tank Commander";
        _localeStringsCoH3[11189947] = "Valentine Tank";
        _localeStringsCoH3[11180070] = "Matilda Mk. II Infantry Tank";
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
