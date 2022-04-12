using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua.Debugging {

    /// <summary>
    /// Represents errors that occured while verifying Lua code syntax.
    /// </summary>
    public class LuaSyntaxError : LuaException {

        /// <summary>
        /// Get the position of the syntax error
        /// </summary>
        public LuaSourcePos SourcePos { get; }

        /// <summary>
        /// Get the suggested fix for this syntax error.
        /// </summary>
        public string Suggestion { get; }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a default error message.
        /// </summary>
        public LuaSyntaxError() : base("Lua syntax error") { this.SourcePos = LuaSourcePos.Undefined; this.Suggestion = string.Empty; }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        public LuaSyntaxError(string luaSyntaxErrMessage) : base(luaSyntaxErrMessage) {
            this.SourcePos = LuaSourcePos.Undefined; this.Suggestion = string.Empty;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message and origin of the syntax error.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display.</param>
        /// <param name="source">The source position of the syntax error.</param>
        public LuaSyntaxError(string luaSyntaxErrMessage, LuaSourcePos source) : base(luaSyntaxErrMessage) {
            this.SourcePos = source;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message and a suggestion for fixing the error.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        /// <param name="suggestion">The suggested correction to display</param>
        public LuaSyntaxError(string luaSyntaxErrMessage, string suggestion) : base(luaSyntaxErrMessage) {
            this.SourcePos = LuaSourcePos.Undefined;
            this.Suggestion = suggestion;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message, a suggestion for fixing the error and origin of the syntax error.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        /// <param name="suggestion">The suggested correction to display</param>
        /// <param name="source">The source position of the syntax error.</param>
        public LuaSyntaxError(string luaSyntaxErrMessage, string suggestion, LuaSourcePos source) : base(luaSyntaxErrMessage) {
            this.SourcePos = source;
            this.Suggestion = suggestion;
        }

    }

}
