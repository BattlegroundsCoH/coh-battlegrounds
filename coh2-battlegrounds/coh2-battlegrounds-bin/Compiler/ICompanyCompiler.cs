using System.Collections.Generic;

using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Compiler;

/// <summary>
/// Interface for compiling a <see cref="Company"/> into Lua code.
/// </summary>
public interface ICompanyCompiler {

    /// <summary>
    /// Compile the <paramref name="company"/> into a lua-convertible represention. Only in-game values are compiled.
    /// </summary>
    /// <param name="company">The company to compile into lua string.</param>
    /// <param name="indexOnTeam">The index of the player on their team. (Should only be used for AI).</param>
    /// <param name="isAIPlayer">Is this an AI player company</param>
    /// <param name="customNames">List of custom names.</param>
    /// <returns>A formatted object ready to be converted into Lua source code.</returns>
    Dictionary<string, object> CompileToLua(Company company, bool isAIPlayer, byte indexOnTeam, IList<string> customNames);

}

