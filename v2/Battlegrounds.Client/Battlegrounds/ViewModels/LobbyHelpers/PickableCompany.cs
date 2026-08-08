using Battlegrounds.Models.Companies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.LobbyHelpers;

/// <summary>
/// Represents a selectable company option, which may be a specific company, a random AI company, or a 'None'
/// placeholder.
/// </summary>
/// <remarks>Use this type to represent user choices in scenarios where a company selection may be optional or
/// randomized. The properties provide convenient access to display and identification information based on the selected
/// option.</remarks>
/// <param name="IsNone">Indicates whether this instance represents the 'None' option, meaning no company is selected.</param>
/// <param name="GenerateRandom">Specifies whether this instance represents a randomly generated AI company.</param>
/// <param name="Company">The company associated with this option, or null if the option is 'None' or a random AI company.</param>
/// <param name="ShowCompanyPreviewCommand">The command to show the company preview.</param>
public sealed record PickableCompany(bool IsNone, bool GenerateRandom, Company? Company, IRelayCommand ShowCompanyPreviewCommand) {
    
    /// <summary>
    /// Gets the display name representing the current company state.
    /// </summary>
    /// <remarks>The display name reflects the company context. If no company is selected, it returns "None".
    /// If a random company is generated, it returns "Random AI Company". Otherwise, it returns the name of the company
    /// or "Unknown Company" if the name is unavailable.</remarks>
    public string DisplayName {
        get {
            if (IsNone)
                return "None";
            if (GenerateRandom) return "Random AI Company";
            return Company?.Name ?? "Unknown Company";
        }
    }
    
    /// <summary>
    /// Gets the faction associated with the company. Returns an empty string if no faction is available.
    /// </summary>
    public string Faction => Company?.Faction ?? string.Empty;
    
    /// <summary>
    /// Gets the unique identifier of the game associated with the company.
    /// </summary>
    /// <remarks>Returns an empty string if the company is not set or does not have a game
    /// identifier.</remarks>
    public string GameId => Company?.GameId ?? string.Empty;

}
