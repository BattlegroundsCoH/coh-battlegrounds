using System;
using System.Threading.Tasks;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Startup;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Handler for when a match has fully completed.
    /// </summary>
    public delegate void MatchCompletedHandler();

    /// <summary>
    /// Handler for handling errors that have occured during the match.
    /// </summary>
    public delegate void MatchErrorHandler(object cause, string reason);

    /// <summary>
    /// Basic controller for controlloing the flow of a match from startup to finalization.
    /// </summary>
    public sealed class MatchController {

        private IMatchStarter m_starter;
        private IMatchAnalyzer m_analyzer;
        private IMatchFinalizer m_finalizer;
        private IStartupStrategy m_startupStrategy;
        private IAnalyzeStrategy m_analyzeStrategy;
        private IFinalizeStrategy m_finalizeStrategy;

        /// <summary>
        /// Occurs when the whole match is completed.
        /// </summary>
        public event MatchCompletedHandler Complete;

        /// <summary>
        /// Occurs when the controller receives an error message.
        /// </summary>
        public event MatchErrorHandler Error;

        /// <summary>
        /// Create a new <see cref="MatchController"/> instance with default strategies.
        /// </summary>
        public MatchController() {
            this.m_starter = null;
            this.m_analyzer = null;
            this.m_finalizer = null;
            this.m_startupStrategy = new SingleplayerStartupStrategy(); // Default Behaviour
            this.m_analyzeStrategy = new SingleplayerMatchAnalyzer(); // Default Behaviour
            this.m_finalizeStrategy = new SingleplayerFinalizer(); // Default behaviour
        }

        /// <summary>
        /// Set composite objects responsible for handling the startup process.
        /// </summary>
        /// <param name="matchStarter">The object responsible for starting a match-</param>
        /// <param name="startupStrategy">The object representing the strategy to use when starting.</param>
        /// <exception cref="ArgumentNullException"/>
        public void SetStartupObjects(IMatchStarter matchStarter, IStartupStrategy startupStrategy) {
            this.m_starter = matchStarter;
            if (startupStrategy is not null) {
                this.m_startupStrategy = startupStrategy;
            } else {
                throw new ArgumentNullException(nameof(startupStrategy), "Startup strategy cannot be null.");
            }
        }

        /// <summary>
        /// Set composite objects responsible for handling the analysis process.
        /// </summary>
        /// <param name="matchAnalyzer">The object responsible for analyzing a match.</param>
        /// <param name="analyzeStrategy">The object representing the analysis strategy.</param>
        /// <exception cref="ArgumentNullException"/>
        public void SetAnalysisObjects(IMatchAnalyzer matchAnalyzer, IAnalyzeStrategy analyzeStrategy) {
            this.m_analyzer = matchAnalyzer;
            if (analyzeStrategy is not null) {
                this.m_analyzeStrategy = analyzeStrategy;
            } else {
                throw new ArgumentNullException(nameof(analyzeStrategy), "Analysis strategy cannot be null.");
            }
        }

        /// <summary>
        /// Set composite objects responsible for handling the finalizing process.
        /// </summary>
        /// <param name="matchFinalizer">The object responsible for finalizing a match.</param>
        /// <param name="finalizeStrategy">The object representing the finalizer strategy.</param>
        /// <exception cref="ArgumentNullException"/>
        public void SetFinalizerObjects(IMatchFinalizer matchFinalizer, IFinalizeStrategy finalizeStrategy) {
            this.m_finalizer = matchFinalizer;
            if (finalizeStrategy is not null) {
                this.m_finalizeStrategy = finalizeStrategy;
            } else {
                throw new ArgumentNullException(nameof(finalizeStrategy), "Finalize strategy cannot be null.");
            }
        }

        /// <summary>
        /// Start controlling the whole process using the designated strategies and case handlers.
        /// </summary>
        /// <remarks>
        /// <see langword="async"/>
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        public async void Control() {

            // Make sure we have a starter
            if (this.m_starter is null) {
                throw new ArgumentNullException(nameof(this.m_starter), "Startup object cannot be null.");
            }
            
            // Make sure we have a starter
            if (this.m_analyzer is null) {
                throw new ArgumentNullException(nameof(this.m_analyzer), "Analysis object cannot be null.");
            } else {
                this.m_analyzer.AnalysisCancelled += (cause, reason) => {
                    this.Error?.Invoke(cause, reason);
                };
            }
            
            // Make sure we have a starter
            if (this.m_finalizer is null) {
                throw new ArgumentNullException(nameof(this.m_finalizer), "Finalizer object cannot be null.");
            } else {
                /*
                this.m_finalizer.FinalizeCancelled += (cause, reason) => {
                    this.Error?.Invoke(); // TODO: Proper error handling
                };
                 */
            }

            // Start game and wait for analyzing to finish.
            await Task.Run(async () => {

                // Startup game
                this.m_starter.Startup(this.m_startupStrategy);
                while (!this.m_starter.HasStarted && !this.m_starter.IsCancelled) {
                    await Task.Delay(100);
                }

                // Quit now if cancelled
                if (this.m_starter.IsCancelled) {
                    return;
                }

                // Get the play object and wait for match to finish
                var player = this.m_starter.PlayObject;
                player.Launch(); // Launch in case it hasn't launched
                player.WaitForExit();

                // Make sure it was a perfect run
                if (player.IsPerfect()) {

                    // Get the results
                    var results = player.GetResults();

                    // Then analyze
                    this.m_analyzer.Analyze(this.m_analyzeStrategy, results);

                    // If the played match was matching, we can finalize
                    if (results.IsSessionMatch) {

                        // Get the analysis
                        var analysis = this.m_analyzer.MatchAnalysis ?? new NullAnalysis();

                        // Make sure we can finalize
                        if (analysis.IsFinalizableMatch) {

                            // And then commit
                            this.m_finalizer.Finalize(this.m_finalizeStrategy, analysis);

                        }

                        // Invoke the complete event
                        this.Complete?.Invoke();

                    } else {

                        // "Throw" Error
                        this.Error?.Invoke(results, "Match ID did not match");

                    }

                } else {

                    // Let caller know that the player was faulty
                    this.Error?.Invoke(player, "The player detected one or more problems");

                }

            });

        }

    }

}
