using System.Threading.Tasks;

using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Game.Match.Startup;

namespace Battlegrounds.Game.Match.Composite;

/// <summary>
/// Represents a Battlegrounds singleplayer session. Implements <see cref="IMatchStarter"/>, <see cref="IMatchAnalyzer"/>, and <see cref="IMatchFinalizer"/>.
/// </summary>
public class SingleplayerSession : IMatchStarter, IMatchAnalyzer, IMatchFinalizer {

    protected bool m_isStarted;
    protected bool m_isCancelled;
    protected bool m_hasSuccessAnalysis;
    protected IPlayStrategy m_playStrategyResult;
    protected IAnalyzedMatch m_analyzedMatch;

    public bool HasStarted => this.m_isStarted;

    public bool IsCancelled => this.m_isCancelled;

    public IPlayStrategy PlayObject => this.m_playStrategyResult;

    public IAnalyzedMatch MatchAnalysis => this.m_analyzedMatch;

    public bool AnalysisSuccess => this.m_hasSuccessAnalysis;

    public event AnalysisCancelledHandler AnalysisCancelled;

    public SingleplayerSession() {
        this.m_isCancelled = false;
        this.m_isStarted = false;
        this.m_playStrategyResult = null;
    }

    public async void Startup(IStartupStrategy startupStrategy) {

        // Assign cancelled handler
        startupStrategy.StartupCancelled += this.StartupStrategy_StartupCancelled;

        // Local method for play
        void ManagedLobbyPlayer() {

            // Begin
            if (!startupStrategy.OnBegin(this)) {
                startupStrategy.OnCancel(this, "One or more players stopped the host from launching.");
                return;
            }

            // Tell the strategy to prepare
            if (startupStrategy.OnPrepare(this)) {

                // Tell the strategy to collect companies
                if (!startupStrategy.OnCollectCompanies(this)) {
                    startupStrategy.OnCancel(this, "One or more companies were not collected.");
                    return;
                }

                // Tell the strategy to collect match info
                if (!startupStrategy.OnCollectMatchInfo(this)) {
                    startupStrategy.OnCancel(this, "Match information was not collected.");
                    return;
                }

                // Tell the strategy to compile (and upload)
                if (!startupStrategy.OnCompile(this)) {
                    startupStrategy.OnCancel(this, "Match data was not uploaded to server");
                    return;
                }

                // Launch self
                if (!startupStrategy.OnStart(this, out this.m_playStrategyResult)) {
                    startupStrategy.OnCancel(this, "Match was cancelled while waiting for local player to start.");
                    return;
                }

                // Now mark self as started
                this.m_isStarted = true;

            } else {

                // Trigger on cancelled
                startupStrategy.OnCancel(this, "Preparation was cancelled.");

            }

        }

        // Await for this task to complete
        await Task.Run(ManagedLobbyPlayer);

    }

    private void StartupStrategy_StartupCancelled(IStartupStrategy sender, object caller, string reason) => this.m_isCancelled = true;

    public void Analyze(IAnalyzeStrategy strategy, IMatchData matchResults) {

        // Set to false
        this.m_hasSuccessAnalysis = false;

        // Bind to ReplayMatchData -> Get the latest replay match data here
        if (matchResults.LoadMatchData(ReplayMatchData.LATEST_REPLAY_FILE)) {

            // Parse the match results
            if (matchResults.ParseMatchData()) {

                // Prepare data
                strategy.OnPrepare(this, matchResults);

                // Analyze data
                strategy.OnAnalyze(this);

                // Cleanup
                this.m_analyzedMatch = strategy.OnCleanup(this);

                // Set as true
                this.m_hasSuccessAnalysis = true;

            } else {

                this.AnalysisCancelled?.Invoke(matchResults, "Failed to parse match data"); // Might move this into the strategy

            }

        } else {

            this.AnalysisCancelled?.Invoke(matchResults, "Failed to read match data");

        }

    }

    public void Finalize(IFinalizeStrategy strategy, IAnalyzedMatch analyzedMatch) {

        // Finalize
        strategy.Finalize(analyzedMatch);

        // Synchronize with other players
        strategy.Synchronize(this);

    }

}

