using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Logging;

namespace Battlegrounds.Game.Match.Startup;

/// <summary>
/// Abstract class with base parameters defined for an <see cref="IStartupStrategy"/> implementation.
/// </summary>
public abstract class BaseStartupStrategy : IStartupStrategy {

    private static readonly Logger logger = Logger.CreateLogger();

    private static readonly SessionHandlerFactory sessionHandlerFactory = new SessionHandlerFactory();

    private Company? m_localCompany;

    /// <summary>
    /// The handler 
    /// </summary>
    protected ISessionHandler sessionHandler;

    /// <inheritdoc/>
    public LocalCompanyHandle? LocalCompanyCollector { get; set; }

    /// <inheritdoc/>
    public SessionInfoHandle? SessionInfoCollector { get; set; }

    /// <inheritdoc/>
    public IPlayStrategyFactory? PlayStrategyFactory { get; set; }

    /// <summary>
    /// Get or set the local company used by the <see cref="IStartupStrategy"/>.
    /// </summary>
    protected Company? LocalCompany {
        get => this.m_localCompany;
        set => this.m_localCompany = value;
    }

    /// <inheritdoc/>
    public event CancelHandler? StartupCancelled;

    /// <inheritdoc/>
    public event InformationHandler? StartupFeedback;

    /// <summary>
    /// Initialise a new <see cref="BaseStartupStrategy"/> instance for specified game.
    /// </summary>
    /// <param name="game">The game this session is targetting</param>
    public BaseStartupStrategy(GameCase game) {
        sessionHandler = sessionHandlerFactory.GetHandler(game);
    }

    /// <inheritdoc/>
    public abstract bool OnBegin(object caller);

    /// <inheritdoc/>
    public virtual void OnCancel(object? caller, string cancelReason) {
        logger.Info($"Startup cancelled ({caller}): \"{cancelReason}\"");
        this.StartupCancelled?.Invoke(this, caller, cancelReason);
    }

    /// <inheritdoc/>
    protected virtual void OnFeedback(object? caller, string message) => this.StartupFeedback?.Invoke(this, caller, message);

    /// <inheritdoc/>
    public virtual bool OnCollectCompanies(object caller) => true;

    /// <inheritdoc/>
    public abstract bool OnCollectMatchInfo(object caller);

    /// <inheritdoc/>
    public abstract bool OnCompile(object caller);

    /// <inheritdoc/>
    public abstract bool OnPrepare(object caller);

    /// <inheritdoc/>
    public abstract bool OnStart(object caller, [NotNullWhen(true)] out IPlayStrategy? playStrategy);

    /// <inheritdoc/>
    public virtual bool OnWaitForAllToSignal(object caller) => true;

    /// <inheritdoc/>
    public virtual bool OnWaitForStart(object caller) => true;

    /// <summary>
    /// Get (and assign to <see cref="LocalCompany"/>) the local company, using the <see cref="LocalCompanyCollector"/> handle.
    /// </summary>
    /// <param name="name">The unique ID of the local user.</param>
    /// <returns>Will return <see langword="true"/> if the local company was assigned. Otherwise <see langword="false"/>.</returns>
    protected bool GetLocalCompany(ulong id) {

        // Return false if no company collector defined
        if (this.LocalCompanyCollector is null)
            return false;

        // Get own company
        this.m_localCompany = this.LocalCompanyCollector();
        if (this.m_localCompany is null) {
            return false;
        } else {
            this.m_localCompany.Owner = id.ToString();
            return true;
        }

    }

}
