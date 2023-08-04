using System;
using System.IO;

using Battlegrounds.Compiler.Source.CoH2;
using Battlegrounds.Compiler.Source.CoH3;
using Battlegrounds.Game;

namespace Battlegrounds.Compiler.Source; 

/// <summary>
/// Factory for finding the correct <see cref="IWinconditionSourceProvider"/>.
/// </summary>
public sealed class WinconditionSourceProviderFactory {

    private IWinconditionSourceProvider GetLocalCoH2Source() {
        
        string path = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "bg_wc\\coh2\\");
        if (Directory.Exists(path)) {
            return new CoH2LocalSourceProvider(path);
        } else {
            return new NoSourceProvider(); 
        }

    }

    private IWinconditionSourceProvider GetLocalCoH3Source() {

        string path = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "bg_wc\\coh3\\");
        if (Directory.Exists(path)) {
            return new CoH3LocalSourceProvider(path);
        } else {
            return new NoSourceProvider();
        }

    }

    /// <summary>
    /// Get the source provider 
    /// </summary>
    /// <param name="game">The game to fetch source for</param>
    /// <returns>The <see cref="IWinconditionSourceProvider"/> that can provide sources for the provided game</returns>
    /// <exception cref="ArgumentException"></exception>
    public IWinconditionSourceProvider GetSource(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => GetLocalCoH2Source(),
        GameCase.CompanyOfHeroes3 => GetLocalCoH3Source(),
        _ => throw new ArgumentException("Provided game has no method of fetching source files", nameof(game)),
    };

}
