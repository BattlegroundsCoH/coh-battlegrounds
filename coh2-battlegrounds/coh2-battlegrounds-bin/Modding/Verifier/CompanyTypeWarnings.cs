using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.Functional;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Modding.Verifier;

/// <summary>
/// Enum representing the company type state (valid/invalid).
/// </summary>
public enum CompanyTypeState {
    
    /// <summary>
    /// State returned when <see cref="CompanyTypeWarnings.CheckCompanyType(FactionCompanyType)"/> deems a company type to be valid.
    /// </summary>
    Valid,

    /// <summary>
    /// State returned when <see cref="CompanyTypeWarnings.CheckCompanyType(FactionCompanyType)"/> deems a company type to be invalid and should
    /// not be made visible in the company creator.
    /// </summary>
    Invalid

}

/// <summary>
/// Static utility class for checking a company and verifying the company type is valid.
/// </summary>
public static class CompanyTypeWarnings {

    /// <summary>
    /// Verifies the input <see cref="FactionCompanyType"/> is valid and reports any problems there may be.
    /// </summary>
    /// <param name="companyType">The company type to verify.</param>
    /// <returns>The state of the company.</returns>
    public static CompanyTypeState CheckCompanyType(FactionCompanyType companyType) {

        // Set default state
        var state = CompanyTypeState.Valid;

        // Verify there are tow transports
        if (!companyType.DeployBlueprints.Any(x => x.Tow)) {
            Trace.WriteLine($"(Fatal) Company type '{companyType.Id}' has no tow transports. If there is heavy artillery, this may cause a crash in the company builder.", nameof(CompanyTypeWarnings));
            state = CompanyTypeState.Invalid;
        }

        // Verify there are deployment methods
        if (companyType.DeployTypes.Length is 0) {
            Trace.WriteLine($"(Fatal) Company type '{companyType.Id}' has no deployment methods defined.", nameof(CompanyTypeWarnings));
            state = CompanyTypeState.Invalid;
        }

        // Verify there are valid upper cap on units
        if (companyType.MaxInfantry + companyType.MaxLeaders + companyType.MaxTeamWeapons + companyType.MaxVehicles <= 0) {
            Trace.WriteLine($"(Fatal) Company type '{companyType.Id}' cannot add units (max caps all non-positive).", nameof(CompanyTypeWarnings));
            state = CompanyTypeState.Invalid;
        }

        // Verify there are phases
        if (companyType.Phases.Count is 0) {
            Trace.WriteLine($"Company type '{companyType.Id}' has no phases defined.", nameof(CompanyTypeWarnings));
        }

        // Check phases
        var dupl = new HashSet<string>();
        int time = -1;
        foreach (var (k, v) in companyType.Phases) {
            var intersect = companyType.Exclude.Intersect(v.Unlocks);
            if (intersect.Length > 0) {
                intersect.ForEach(x => Trace.WriteLine($"Company type '{companyType.Id}' has unit '{x}' in exclusion list and in phase '{k}'.", nameof(CompanyTypeWarnings)));
            }
            for (int i = 0; i < v.Unlocks.Length; i++) {
                if (!dupl.Add(v.Unlocks[i])) {
                    Trace.WriteLine($"Company type '{companyType.Id}' has unit '{v.Unlocks[i]}' in phase '{k}' put is also in a different phase.", nameof(CompanyTypeWarnings));
                }
            }
            if (v.ActivationTime > time) {
                time = v.ActivationTime;
            } else {
                Trace.WriteLine($"(Fatal) Company type '{companyType.Id}' phase '{k}' is activated at time {v.ActivationTime} but previous phase is activated at time {time}.", nameof(CompanyTypeWarnings));
                state = CompanyTypeState.Invalid;
            }
        }

        // Verify abilities exist (TODO: When added)
        //for (int i = 0; i < companyType.)

        // Return state
        return state;

    }

}
