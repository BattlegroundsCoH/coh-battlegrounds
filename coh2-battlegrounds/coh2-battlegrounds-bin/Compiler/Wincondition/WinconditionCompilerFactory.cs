﻿using System;

using Battlegrounds.Compiler.Locale;
using Battlegrounds.Compiler.Locale.CoH2;
using Battlegrounds.Compiler.Locale.CoH3;
using Battlegrounds.Compiler.Wincondition.CoH2;
using Battlegrounds.Compiler.Wincondition.CoH3;
using Battlegrounds.Game;

namespace Battlegrounds.Compiler.Wincondition;

/// <summary>
/// Factory class responsible for creating instances of <see cref="IWinconditionCompiler"/> based on the specified <see cref="GameCase"/>.
/// </summary>
public sealed class WinconditionCompilerFactory {

    private readonly ILocaleCompiler localeCompilerCoH2;
    private readonly ILocaleCompiler localeCompilerCoH3;
    private readonly string workDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WinconditionCompilerFactory"/> class.
    /// </summary>
    public WinconditionCompilerFactory() {
        localeCompilerCoH2 = new CoH2LocaleCompiler();
        localeCompilerCoH3 = new CoH3LocaleCompiler();
        workDirectory = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BUILD_FOLDER, string.Empty);
    }

    /// <summary>
    /// Gets the appropriate <see cref="IWinconditionCompiler"/> implementation based on the specified <see cref="GameCase"/>.
    /// </summary>
    /// <param name="game">The <see cref="GameCase"/> enumeration value representing the game.</param>
    /// <returns>An instance of the corresponding <see cref="IWinconditionCompiler"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid game is specified with no wincondition compiler associated.</exception>
    public IWinconditionCompiler GetWinconditionCompiler(GameCase game) => game switch { 
        GameCase.CompanyOfHeroes2 => new CoH2WinconditionCompiler(workDirectory, localeCompilerCoH2
            /*, new BattlegroundsArchiver(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.COH2_FOLDER), game)*/),
        GameCase.CompanyOfHeroes3 => new CoH3WinconditionCompiler(workDirectory, localeCompilerCoH3, 
            new BattlegroundsArchiver(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.COH3_FOLDER), game)),
        _ => throw new ArgumentException("Specified game has no wincondition compiler associated", nameof(game)),
    };

}
