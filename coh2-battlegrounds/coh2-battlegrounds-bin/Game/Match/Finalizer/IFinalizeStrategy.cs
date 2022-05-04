using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Analyze;

namespace Battlegrounds.Game.Match.Finalizer;

/// <summary>
/// Handler for receiving a changed company.
/// </summary>
/// <param name="company">The updated <see cref="Company"/> after finalization.</param>
public delegate void FinalizedCompanyHandler(Company company);

/// <summary>
/// Strategy interface for finalizing a Company of Heroes 2: Battlegrounds match.
/// </summary>
public interface IFinalizeStrategy {

    /// <summary>
    /// Get or set the handler that will handle saving a finalized <see cref="Company"/>.
    /// </summary>
    FinalizedCompanyHandler CompanyHandler { get; set; }

    /// <summary>
    /// Finalize all the data from the <see cref="IAnalyzedMatch"/>.
    /// </summary>
    /// <param name="analyzedMatch">The <see cref="IAnalyzedMatch"/> data to finalize.</param>
    void Finalize(IAnalyzedMatch analyzedMatch);

    /// <summary>
    /// Synchronize all finalized data with other components.
    /// </summary>
    /// <param name="synchronizeObject">The object(s) to use when synchronizing.</param>
    void Synchronize(object synchronizeObject);

}
