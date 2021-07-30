using System.Diagnostics;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;

namespace Battlegrounds.Game.Match.Startup {

    /// <summary>
    /// Abstract class with base parameters defined for an <see cref="IStartupStrategy"/> implementation.
    /// </summary>
    public abstract class BaseStartupStrategy : IStartupStrategy {

        private Company m_localCompany;

        public LocalCompanyHandle LocalCompanyCollector { get; set; }

        public SessionInfoHandle SessionInfoCollector { get; set; }

        public IPlayStrategyFactory PlayStrategyFactory { get; set; }

        /// <summary>
        /// Get or set the local company used by the <see cref="IStartupStrategy"/>.
        /// </summary>
        protected Company LocalCompany {
            get => this.m_localCompany;
            set => this.m_localCompany = value;
        }

        public event CancelHandler StartupCancelled;
        public event InformationHandler StartupFeedback;

        public abstract bool OnBegin(object caller);

        public virtual void OnCancel(object caller, string cancelReason) {
            Trace.WriteLine($"Startup cancelled ({caller}): \"{cancelReason}\"", "IStartupStrategy");
            this.StartupCancelled?.Invoke(this, caller, cancelReason);
        }

        protected virtual void OnFeedback(object caller, string message) => this.StartupFeedback?.Invoke(this, caller, message);

        public virtual bool OnCollectCompanies(object caller) => true;

        public abstract bool OnCollectMatchInfo(object caller);

        public abstract bool OnCompile(object caller);

        public abstract bool OnPrepare(object caller);

        public abstract bool OnStart(object caller, out IPlayStrategy playStrategy);

        public virtual bool OnWaitForAllToSignal(object caller) => true;

        public virtual bool OnWaitForStart(object caller) => true;

        /// <summary>
        /// Get (and assign to <see cref="LocalCompany"/>) the local company, using the <see cref="LocalCompanyCollector"/> handle.
        /// </summary>
        /// <param name="name">The unique ID of the local user.</param>
        /// <returns>Will return <see langword="true"/> if the local company was assigned. Otherwise <see langword="false"/>.</returns>
        protected bool GetLocalCompany(ulong id) {

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

}
