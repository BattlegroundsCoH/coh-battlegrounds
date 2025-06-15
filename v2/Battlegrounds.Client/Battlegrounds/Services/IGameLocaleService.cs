using Battlegrounds.Models;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface IGameLocaleService {

    LocaleString FromGame<T>(uint key) where T : Game;

    string ResolveLocaleString<T>(uint key, params object[] args) where T : Game;

    Task<bool> LoadLocalesAsync();

}
