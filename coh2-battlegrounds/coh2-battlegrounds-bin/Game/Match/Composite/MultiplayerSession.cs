using System.Threading.Tasks;

using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Game.Match.Composite;

/// <summary>
/// Represents a Battlegrounds multiplayer session. Implements <see cref="IMatchStarter"/>, <see cref="IMatchAnalyzer"/>, and <see cref="IMatchFinalizer"/>.
/// </summary>
public sealed class MultiplayerSession : IMatchStarter, IMatchAnalyzer, IMatchFinalizer {

    private bool m_isStarted;
    private bool m_isCancelled;
    private bool m_hasSuccessAnalysis;
    private readonly LobbyAPI m_lobby;
    private IPlayStrategy? m_playStrategyResult;
    private IAnalyzedMatch? m_analyzedMatch;

    public bool HasStarted => this.m_isStarted;

    public bool IsCancelled => this.m_isCancelled;

    public IPlayStrategy? PlayObject => this.m_playStrategyResult;

    public IAnalyzedMatch? MatchAnalysis => this.m_analyzedMatch;

    public bool AnalysisSuccess => this.m_hasSuccessAnalysis;

    public event AnalysisCancelledHandler? AnalysisCancelled;

    public MultiplayerSession(LobbyAPI lobby) {
        this.m_isCancelled = false;
        this.m_isStarted = false;
        this.m_lobby = lobby;
        this.m_playStrategyResult = null;
    }

    public async void Startup(IStartupStrategy startupStrategy) {

        // Assign cancelled handler
        startupStrategy.StartupCancelled += this.StartupStrategy_StartupCancelled;

        // Local method for play
        void ManagedLobbyPlayer() {

            // Begin
            if (!startupStrategy.OnBegin(this.m_lobby)) {
                startupStrategy.OnCancel(this, "One or more players stopped the host from launching.");
                return;
            }

            // Tell the strategy to prepare
            if (startupStrategy.OnPrepare(this.m_lobby)) {

                // Tell the strategy to collect companies
                if (!startupStrategy.OnCollectCompanies(this.m_lobby)) {
                    startupStrategy.OnCancel(this, "One or more companies were not collected.");
                    return;
                }

                // Tell the strategy to collect match info
                if (!startupStrategy.OnCollectMatchInfo(this.m_lobby)) {
                    startupStrategy.OnCancel(this, "Match information was not collected.");
                    return;
                }

                // Tell the strategy to compile (and upload)
                if (!startupStrategy.OnCompile(this.m_lobby)) {
                    startupStrategy.OnCancel(this, "Match data was not uploaded to server");
                    return;
                }

                // Tell the strategy to wait for players
                if (!startupStrategy.OnWaitForStart(this.m_lobby)) {
                    startupStrategy.OnCancel(this, "Match was cancelled while waiting for startup.");
                    return;
                }

                // Tell the strategy to wait for players players to signal "STARTED"
                if (!startupStrategy.OnWaitForAllToSignal(this.m_lobby)) {
                    startupStrategy.OnCancel(this, "Match was cancelled while waiting for all players to start.");
                    return;
                }

                // Launch self
                if (!startupStrategy.OnStart(this.m_lobby, out this.m_playStrategyResult)) {
                    startupStrategy.OnCancel(this, "Match was cancelled while waiting for local player to start.");
                    return;
                }

                // Now mark self as started
                this.m_isStarted = true;

            } else {

                // Trigger on cancelled
                startupStrategy.OnCancel(this.m_lobby, "Preparation was cancelled.");

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
                strategy.OnPrepare(this.m_lobby, matchResults);

                // Analyze data
                strategy.OnAnalyze(this.m_lobby);

                // Cleanup
                this.m_analyzedMatch = strategy.OnCleanup(this.m_lobby);

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
        strategy.Synchronize(this.m_lobby);

    }

}

