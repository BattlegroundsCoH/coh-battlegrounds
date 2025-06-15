using System;

using Battlegrounds.Compiler.Source;
using Battlegrounds.Compiler.Wincondition;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Factory class responsible for creating instances of <see cref="ISessionHandler"/> based on the specified <see cref="GameCase"/>.
/// </summary>
public sealed class SessionHandlerFactory {

    private readonly WinconditionCompilerFactory winconditionCompilerFactory;
    private readonly WinconditionSourceProviderFactory winconditionSourceFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionHandlerFactory"/> class.
    /// </summary>
    public SessionHandlerFactory() {
        winconditionCompilerFactory = new WinconditionCompilerFactory();
        winconditionSourceFactory = new WinconditionSourceProviderFactory();
    }

    /// <summary>
    /// Gets the appropriate <see cref="ISessionHandler"/> implementation based on the specified <see cref="GameCase"/>.
    /// </summary>
    /// <param name="game">The <see cref="GameCase"/> enumeration value representing the game.</param>
    /// <returns>An instance of the corresponding <see cref="ISessionHandler"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid game is specified with no handler defined.</exception>
    public ISessionHandler GetHandler(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => new CoH2SessionHandler(winconditionSourceFactory, winconditionCompilerFactory),
        GameCase.CompanyOfHeroes3 => new CoH3SessionHandler(winconditionSourceFactory, winconditionCompilerFactory),
        _ => throw new ArgumentException("Invalid game specified; no handler defined", nameof(game)),
    };

}
