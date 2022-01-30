using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;

namespace Battlegrounds.Game.Match.Startup {

    /// <summary>
    /// Handle for getting the local company.
    /// </summary>
    /// <returns>A <see cref="Company"/> instance.</returns>
    public delegate Company? LocalCompanyHandle();

    /// <summary>
    /// Handle for collecting the <see cref="SessionInfo"/> instance to generate <see cref="Session"/> data from.
    /// </summary>
    /// <returns>The <see cref="SessionInfo"/>.</returns>
    public delegate SessionInfo SessionInfoHandle();

    /// <summary>
    /// Handler for handling cancel events triggered by <see cref="IStartupStrategy"/> objects.
    /// </summary>
    /// <param name="sender">The sending <see cref="IStartupStrategy"/> instance.</param>
    /// <param name="caller">The <see cref="object"/> that triggered the cancel event.</param>
    /// <param name="reason">The given reason for cancelling.</param>
    public delegate void CancelHandler(IStartupStrategy sender, object caller, string reason);

    /// <summary>
    /// Handler for handling information events triggered by <see cref="IStartupStrategy"/> objects.
    /// </summary>
    /// <param name="sender">The sending <see cref="IStartupStrategy"/> object.</param>
    /// <param name="caller">The <see cref="object"/> that triggered the information event.</param>
    /// <param name="message">The given missage.</param>
    public delegate void InformationHandler(IStartupStrategy sender, object caller, string message);

    /// <summary>
    /// Strategy interface for handling game startup mechanisms.
    /// </summary>
    public interface IStartupStrategy {

        /// <summary>
        /// Get or set the method handle for collecting the local player's <see cref="Company"/> to be played with.
        /// </summary>
        LocalCompanyHandle LocalCompanyCollector { get; set; }

        /// <summary>
        /// Get or set the method handle for collecting a <see cref="SessionInfo"/> instance.
        /// </summary>
        SessionInfoHandle SessionInfoCollector { get; set; }

        /// <summary>
        /// Get or set the <see cref="IPlayStrategyFactory"/> to use when constructing the <see cref="IPlayStrategy"/>.
        /// </summary>
        IPlayStrategyFactory PlayStrategyFactory { get; set; }

        /// <summary>
        /// Occurs when the strategy has cancelled the startup.
        /// </summary>
        event CancelHandler StartupCancelled;

        /// <summary>
        /// Occurs when the strategy has non error-related information to convey.
        /// </summary>
        event InformationHandler StartupFeedback;

        /// <summary>
        /// Invoked when the calling instance is beginning to start.
        /// </summary>
        /// <remarks>
        /// Should be the first method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnBegin(object caller);

        /// <summary>
        /// Invoked when the calling instance is to prepare basic data for startup.
        /// </summary>
        /// <remarks>
        /// Should be the second method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnPrepare(object caller);

        /// <summary>
        /// Invoked when the calling instance is to collect participating companies.
        /// </summary>
        /// <remarks>
        /// Should be the third method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnCollectCompanies(object caller);

        /// <summary>
        /// Invoked when the match information is to be collected.
        /// </summary>
        /// <remarks>
        /// Should be the fourth method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnCollectMatchInfo(object caller);

        /// <summary>
        /// Invoked when the game is ready to be started.
        /// </summary>
        /// <remarks>
        /// Should be the sixth method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnWaitForStart(object caller);

        /// <summary>
        /// Invoked when external parties have sent a start signal.
        /// </summary>
        /// <remarks>
        /// Should be the seventh method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnWaitForAllToSignal(object caller);

        /// <summary>
        /// Invoked when the client is to launch Company of Heroes 2.
        /// </summary>
        /// <remarks>
        /// Should be the eigth method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <param name="playStrategy">The resulting <see cref="IPlayStrategy"/> created from the startup strategy. (Tight coupling).</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnStart(object caller, out IPlayStrategy playStrategy);

        /// <summary>
        /// Invoked when the instance is to compile the match data.
        /// </summary>
        /// <remarks>
        /// Should be the fifth method called in the startup sequence.
        /// </remarks>
        /// <param name="caller">The instance that called the method.</param>
        /// <returns>Should return <see langword="true"/> if startup should continue. Otherwise <see langword="false"/> if startup should terminate.</returns>
        bool OnCompile(object caller);

        /// <summary>
        /// Invoked when the startup process is cancelled.
        /// </summary>
        /// <param name="caller">The instance that called the method.</param>
        /// <param name="cancelReason">The reason given as to why the startup was cancelled.</param>
        void OnCancel(object caller, string cancelReason);

    }

}
