using System.Diagnostics;

using Battlegrounds.Errors.Common;

namespace Battlegrounds.Game.Match;

public class StartupException : BattlegroundsException {
    public StartupException() : base("Failed to start match on startup") { }
    public StartupException(string msg) : base(msg) {
        Trace.WriteLine(msg, nameof(StartupException));
    }
}
