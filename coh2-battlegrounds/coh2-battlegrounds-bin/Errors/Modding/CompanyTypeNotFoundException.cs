using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Errors.Modding;

/// <summary>
/// Represents an exception that is thrown when a company type cannot be found for a faction in a mod package.
/// </summary>
public sealed class CompanyTypeNotFoundException : BattlegroundsModException {

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyTypeNotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="package">The mod package where the company type was not found.</param>
    public CompanyTypeNotFoundException(string message, IModPackage package) : base(message, package) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyTypeNotFoundException"/> class with a specified company type, faction and package.
    /// </summary>
    /// <param name="companyType">The company type that could not be found.</param>
    /// <param name="faction">The faction for which the company type could not be found.</param>
    /// <param name="package">The mod package where the company type was not found.</param>
    public CompanyTypeNotFoundException(string companyType, Faction faction, IModPackage package) 
        : base($"Failed to find company type '{companyType}' for faction '{faction.Name}' in package '{package.PackageName}'", package) { }

}
