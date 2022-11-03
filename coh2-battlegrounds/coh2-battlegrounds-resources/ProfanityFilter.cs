using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Text.Json;

using Battlegrounds.Functional;

namespace Battlegrounds.Resources;

/// <summary>
/// Static utility class for filtering profanities
/// </summary>
public static class ProfanityFilter {

    /// <summary>
    /// Record representing a filter and its censored words
    /// </summary>
    /// <param name="Superset">The name of the filter this filter is a superset of.</param>
    /// <param name="FilterWords">The words to filter based on language 'all' is applied regardless of language</param>
    private record FilterContent(string Superset, Dictionary<string, string[]> FilterWords);

    private static bool __isFilterLoaded = false;
    private static Dictionary<string, FilterContent>? __filters;

    [MemberNotNullWhen(true, nameof(__filters))]
    public static bool IsFilterLoaded => __isFilterLoaded;

    public static void LoadFilter() {

        // Get filter from resources
        var rawBytes = ResourceHandler.GetResourceAndUnload(ResourceType.ProfanityFilter);
        if (rawBytes is null) {
            return;
        }

        // Try parse
        try {
            __filters = JsonSerializer.Deserialize<Dictionary<string, FilterContent>>(rawBytes) ?? new();
            // Mark as loaded
            __isFilterLoaded = true;
        } catch (Exception e) {
            Trace.WriteLine(e, nameof(ProfanityFilter));
        }

    }

    /// <summary>
    /// Runs a string through the specified filter mode (use 'strict' for public stuff, like lobbies)
    /// </summary>
    /// <param name="toFilter">The string to filter</param>
    /// <param name="corrected">The correct string if any changes have been made</param>
    /// <param name="filterMode">The filter to use (use 'strict' if public)</param>
    /// <returns>if a profanity is found (<paramref name="toFilter"/> != <paramref name="corrected"/>) then <see langword="false"/>; Otherwise <see langword="true"/>.</returns>
    public static bool Filter(string toFilter, out string corrected, string filterMode = "profanities") {
        corrected = toFilter;
        if (IsFilterLoaded) {

            if (__filters.TryGetValue(filterMode, out var content) && content is FilterContent filter) {

                corrected = filter.FilterWords["all"].Fold(toFilter, ApplyFilter);
                if (filter.FilterWords.TryGetValue(BattlegroundsInstance.Localize.Language.ToString().ToLowerInvariant(), out var words) && words is not null) {
                    corrected = words.Fold(toFilter, ApplyFilter);
                }

                // If superset, apply subset
                if (!string.IsNullOrEmpty(filter.Superset)) {
                    return (corrected == toFilter) && Filter(corrected, out corrected, filter.Superset);
                }

            }

            return corrected == toFilter;
        } else {
            return true;
        }
    }

    private static string ApplyFilter(string str, string filter) => str.Replace(filter, "*");

}
