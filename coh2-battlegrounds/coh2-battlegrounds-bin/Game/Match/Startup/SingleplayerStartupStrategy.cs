using Battlegrounds.Compiler;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Composite;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Logging;

namespace Battlegrounds.Game.Match.Startup;

/// <summary>
/// Startup Strategy for games with one human player. Can be extended with custom behaviour.
/// </summary>
public sealed class SingleplayerStartupStrategy : BaseStartupStrategy {

    private static readonly Logger logger = Logger.CreateLogger();

    private Session? m_collectedSession;

    /// <summary>
    /// Create a new <see cref="SingleplayerStartupStrategy"/> instance.
    /// </summary>
    /// <param name="game">The specific game to play</param>
    public SingleplayerStartupStrategy(GameCase game) : base(game) {}

    /// <inheritdoc/>
    public override bool OnPrepare(object caller) => this.GetLocalCompany(BattlegroundsContext.Steam.User.ID);

    /// <inheritdoc/>
    public override bool OnBegin(object caller) => true;

    /// <inheritdoc/>
    public override bool OnCollectMatchInfo(object caller) {

        // Verify the info collector is valid
        if (this.SessionInfoCollector is null) {
            logger.Error("Session info collector was null on match collection.");
            return false;
        }

        // Verify local company is there
        if (this.LocalCompany is not Company local) {
            logger.Error("Local company was not defined on startup.");
            return false;
        }

        // Collect session info and create session from it.
        var info = this.SessionInfoCollector();

        // Zip the single company, if we're a semi-singleplayer call
        if (caller is not SingleplayerSession) {
            Session.ZipCompanies(new[] { local }, ref info);
        }

        // Create the session
        this.m_collectedSession = Session.CreateSession(info);

        // Return whether or not we got a session
        return this.m_collectedSession is not null;

    }

    /// <inheritdoc/>
    public ICompanyCompiler GetCompanyCompiler() => new CompanyCompiler();

    /// <inheritdoc/>
    public ISessionCompiler GetSessionCompiler() => new SessionCompiler();

    /// <inheritdoc/>
    public override bool OnCompile(object caller) {

        // Create compiler
        var compiler = this.GetSessionCompiler();
        compiler.SetCompanyCompiler(this.GetCompanyCompiler());

        // Log state
        this.OnFeedback(null, "Compiling gamemode");

        // Verify the info collector is valid
        if (this.m_collectedSession is null) {
            logger.Error("Session info collector was null on match compile.");
            return false;
        }

        // Compile session
        bool result = sessionHandler.CompileSession(compiler, this.m_collectedSession);

        // Log result
        if (result) {
            this.OnFeedback(null, "Compiled gamemode");
        }

        return result;

    }

    /// <inheritdoc/>
    public override bool OnStart(object caller, out IPlayStrategy playStrategy) {

        // Verify the info collector is valid
        if (this.m_collectedSession is null) {
            logger.Error("Session info collector was null on match compile.");
            playStrategy = new NoPlayStrategy();
            return false;
        }

        // Use the overwatch strategy (and launch).
        playStrategy = this.PlayStrategyFactory?.CreateStrategy(this.m_collectedSession, sessionHandler) is IPlayStrategy strat ? strat : new NoPlayStrategy();
        playStrategy.Launch();

        // Inform player they'll now be launching
        this.OnFeedback(null, $"Launching game...");

        // Return true
        return playStrategy.IsLaunched;

    }

}
