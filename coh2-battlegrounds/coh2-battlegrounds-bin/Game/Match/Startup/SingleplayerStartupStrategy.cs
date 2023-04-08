using System.Diagnostics;

using Battlegrounds.Compiler;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Composite;
using Battlegrounds.Game.Match.Play;

namespace Battlegrounds.Game.Match.Startup;

/// <summary>
/// Startup Strategy for games with one human player. Can be extended with custom behaviour.
/// </summary>
public class SingleplayerStartupStrategy : BaseStartupStrategy {

    private Session? m_collectedSession;

    public override bool OnPrepare(object caller) => this.GetLocalCompany(BattlegroundsContext.Steam.User.ID);

    public override bool OnBegin(object caller) => true;

    public override bool OnCollectMatchInfo(object caller) {

        // Verify the info collector is valid
        if (this.SessionInfoCollector is null) {
            Trace.WriteLine("Session info collector was null on match collection.", nameof(SingleplayerStartupStrategy));
            return false;
        }

        // Verify local company is there
        if (this.LocalCompany is not Company local) {
            Trace.WriteLine("Local company was not defined on startup.", nameof(SingleplayerStartupStrategy));
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

    public virtual ICompanyCompiler GetCompanyCompiler() => new CompanyCompiler();

    public virtual ISessionCompiler GetSessionCompiler() => new SessionCompiler();

    public override bool OnCompile(object caller) {

        // Create compiler
        var compiler = this.GetSessionCompiler();
        compiler.SetCompanyCompiler(this.GetCompanyCompiler());

        // Log state
        this.OnFeedback(null, "Compiling gamemode");

        // Verify the info collector is valid
        if (this.m_collectedSession is null) {
            Trace.WriteLine("Session info collector was null on match compile.", nameof(SingleplayerStartupStrategy));
            return false;
        }

        // Compile session
        bool result = SessionUtility.CompileSession(compiler, this.m_collectedSession);

        // Log result
        if (result) {
            this.OnFeedback(null, "Compiled gamemode");
        }

        return result;

    }

    public override bool OnStart(object caller, out IPlayStrategy playStrategy) {

        // Verify the info collector is valid
        if (this.m_collectedSession is null) {
            Trace.WriteLine("Session info collector was null on match compile.", nameof(SingleplayerStartupStrategy));
            playStrategy = new NoPlayStrategy();
            return false;
        }

        // Use the overwatch strategy (and launch).
        playStrategy = this.PlayStrategyFactory?.CreateStrategy(this.m_collectedSession) is IPlayStrategy strat ? strat : new NoPlayStrategy();
        playStrategy.Launch();

        // Inform player they'll now be launching
        this.OnFeedback(null, $"Launching game...");

        // Return true
        return playStrategy.IsLaunched;

    }

}
