using Battlegrounds.Models.Companies;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed record PickableCompany(bool IsNone, bool GenerateRandom, Company? Company) {
    public string DisplayName {
        get {
            if (IsNone)
                return "None";
            if (GenerateRandom) return "Random AI Company";
            return Company?.Name ?? "Unknown Company";
        }
    }
}
