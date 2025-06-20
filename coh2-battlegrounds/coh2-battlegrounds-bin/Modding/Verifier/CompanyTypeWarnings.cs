﻿using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Logging;
using Battlegrounds.Modding.Content;
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

    private static readonly Logger logger = Logger.CreateLogger();

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
            logger.Warning($"Company type '{companyType.Id}' has no tow transports. If there is heavy artillery, this may cause a crash in the company builder.");
            state = CompanyTypeState.Invalid;
        }

        // Verify there are deployment methods
        if (companyType.DeployTypes.Length is 0) {
            logger.Warning($"Company type '{companyType.Id}' has no deployment methods defined.");
            state = CompanyTypeState.Invalid;
        }

        // Verify there are valid upper cap on units
        if (companyType.MaxInfantry + companyType.MaxLeaders + companyType.MaxTeamWeapons + companyType.MaxVehicles <= 0) {
            logger.Warning($"Company type '{companyType.Id}' cannot add units (max caps all non-positive).");
            state = CompanyTypeState.Invalid;
        }

        // UI Warning regarding highlighted units
        if (companyType.UIData.HighlightUnits?.Length is not 3) {
            logger.Warning($"Company type '{companyType.Id}' has more or less than 3 highlighted units.");
        } else if (companyType.UIData.HighlightUnits?.Intersect(companyType.Exclude).Length is not 0) {
            logger.Warning($"Company type '{companyType.Id}' highlighted units in exclusion list.");
        }

        // UI Warning regarding highlighted units
        if (companyType.UIData.HighlightAbilities?.Length is not 3) {
            logger.Warning($"Company type '{companyType.Id}' has more or less than 3 highlighted abilities.");
        }

        // Verify there are command levels
        if (companyType.Roles.Count is 0) {
            logger.Warning($"Company type '{companyType.Id}' has no command levels defined.");
        }

        // Check phases
        var dupl = new HashSet<string>();
        foreach (var (k, v) in companyType.Roles) {
            var intersect = companyType.Exclude.Intersect(v.Unlocks);
            if (intersect.Length > 0) {
                intersect.ForEach(x => logger.Warning($"Company type '{companyType.Id}' has unit '{x}' in exclusion list and in command level '{k}'."));
            }
            for (int i = 0; i < v.Unlocks.Length; i++) {
                if (string.IsNullOrEmpty(v.Unlocks[i])) {
                    logger.Warning($"(Fatal) Company type '{companyType.Id}' phase '{k}' contains the empty string at index {i+1}.");
                    state = CompanyTypeState.Invalid;
                    continue;
                }
                if (!dupl.Add(v.Unlocks[i])) {
                    logger.Warning($"Company type '{companyType.Id}' has unit '{v.Unlocks[i]}' in phase '{k}' put is also in a different phase.");
                }
            }
        }

        // Verify abilities exist
        if (companyType.FactionData is FactionData fdata) {
            for (int i = 0; i < companyType.Abilities.Length; i++) {
                if (!fdata.Abilities.Any(x => x.Blueprint == companyType.Abilities[i].Id)) {
                    logger.Warning($"Company type '{companyType.Id}' has ability '{companyType.Abilities[i].Id}' that is not defined for the faction.");
                }
            }
        }

        // Return state
        return state;

    }

}
