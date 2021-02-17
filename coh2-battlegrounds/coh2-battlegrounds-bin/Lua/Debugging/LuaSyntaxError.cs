namespace Battlegrounds.Lua.Debugging {
    
    /// <summary>
    /// Represents errors that occured while verifying Lua code syntax.
    /// </summary>
    public class LuaSyntaxError : LuaException {
    
        /// <summary>
        /// Get the line where the syntax error was detected.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Get the starting column where the syntax error was detected.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Get the suggested fix for this syntax error.
        /// </summary>
        public string Suggestion { get; }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a default error message.
        /// </summary>
        public LuaSyntaxError() : base("Lua syntax error") { this.Line = int.MaxValue; this.Column = int.MaxValue; this.Suggestion = string.Empty; }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        public LuaSyntaxError(string luaSyntaxErrMessage) : base(luaSyntaxErrMessage) {
            this.Line = int.MaxValue; this.Column = int.MaxValue; this.Suggestion = string.Empty;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message and origin of the syntax error.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        /// <param name="line">The line that caused the error</param>
        /// <param name="column">The column that contains the starting character of the error</param>
        public LuaSyntaxError(string luaSyntaxErrMessage, int line, int column) : base(luaSyntaxErrMessage) {
            this.Line = line;
            this.Column = column;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message and a suggestion for fixing the error.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        /// <param name="suggestion">The suggested correction to display</param>
        public LuaSyntaxError(string luaSyntaxErrMessage, string suggestion) : base(luaSyntaxErrMessage) {
            this.Line = int.MaxValue;
            this.Column = int.MaxValue;
            this.Suggestion = suggestion;
        }

        /// <summary>
        /// Initialize a new <see cref="LuaSyntaxError"/> class with a specialised error message, a suggestion for fixing the error and origin of the syntax error.
        /// </summary>
        /// <param name="luaSyntaxErrMessage">The specialised error message to display</param>
        /// <param name="suggestion">The suggested correction to display</param>
        /// <param name="line">The line that caused the error</param>
        /// <param name="column">The column that contains the starting character of the error</param>
        public LuaSyntaxError(string luaSyntaxErrMessage, string suggestion, int line, int column) : base(luaSyntaxErrMessage) {
            this.Line = line;
            this.Column = column;
            this.Suggestion = suggestion;
        }

    }

}
