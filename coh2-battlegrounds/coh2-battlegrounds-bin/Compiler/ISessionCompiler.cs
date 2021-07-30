using Battlegrounds.Game.Match;

namespace Battlegrounds.Compiler {

    /// <summary>
    /// Interface for a <see cref="Session"/> to Lua compiler.
    /// </summary>
    public interface ISessionCompiler {

        /// <summary>
        /// Set the <see cref="ICompanyCompiler"/> to use when compiling a company.
        /// </summary>
        /// <param name="companyCompiler">The concrete <see cref="ICompanyCompiler"/> to use when compiling a company.</param>
        void SetCompanyCompiler(ICompanyCompiler companyCompiler);

        /// <summary>
        /// Compile a <see cref="Session"/> into Lua Source Code.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> instance to compile.</param>
        /// <returns>A formatted <see cref="string"/> containing Lua Source Code.</returns>
        string CompileSession(Session session);

    }

}
