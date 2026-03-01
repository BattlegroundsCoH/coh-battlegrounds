using System.Collections.ObjectModel;
using System.ComponentModel;

using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Statistics;
using Battlegrounds.Services;

namespace Battlegrounds.ViewModels;

public record RecentMatchViewModel(string GameId, string CompanyFaction, string CompanyName, string Map, bool Victory, DateTime Timestamp, TimeSpan Duration);

public record FeaturedCompanyViewModel(string CompanyFaction, string CompanyName, string GameId, int PlayCount, int VeteranUnits);

public record NewsOrUpdatesViewModel();

public sealed class HomeViewModel : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly TaskCompletionSource _dataLoadedCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly IStatisticsService _statisticsService;
    private readonly ICompanyService _companyService;

    private string _welcomeMessage = "Welcome back, Commander!";

    public string WelcomeMessage => _welcomeMessage;

    public int TotalMatches { get; private set; } = 0;
    public int TotalVictories { get; private set; } = 0;
    public int WinRate => TotalMatches > 0 ? (int)((double)TotalVictories / TotalMatches * 100) : 0;
    public string MostPlayedFaction { get; private set; } = "N/A";
    public string MostPlayedFactionGameId { get; private set; } = CoH3.GameId; // Default to CoH3 for the faction icon, will be updated to the correct game once the data is loaded
    public string MostPlayedScenario { get; private set; } = "N/A";
    public string MostPlayedScenarioGameId { get; private set; } = CoH3.GameId; // Default to CoH3 for the scenario icon, will be updated to the correct game once the data is loaded

    public string TotalPlayTime { get; private set; } = "0h 0m";
    public string Rank { get; private set; } = "Recruit";
    public int CompaniesOwned { get; private set; } = 0;

    public ObservableCollection<RecentMatchViewModel> RecentMatches { get; } = [];

    public ObservableCollection<FeaturedCompanyViewModel> FeaturedCompanies { get; } = [];

    public ObservableCollection<NewsOrUpdatesViewModel> NewsAndUpdates { get; } = [];

    public HomeViewModel(IStatisticsService statisticsService, ICompanyService companyService) {
        _statisticsService = statisticsService;
        _companyService = companyService;
    }

    public void OnDataLoaded() {
        _dataLoadedCompletionSource.SetResult();
    }

    public void UpdateUser(User user) {
        _welcomeMessage = $"Welcome back, {user?.UserDisplayName ?? "Commander"}!";
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeMessage)));
    }

    public void OnViewActivated() {
        OnViewModelInitialized();
    }

    private async void OnViewModelInitialized() {

        // Wait for the statistics service to load the data before updating the properties
        await Task.WhenAll(_statisticsService.IsLoaded, _dataLoadedCompletionSource.Task);

        // Get the played matches from the statistics service and update the properties accordingly
        var playedMatches = _statisticsService.GetPlayedMatches();

        TotalMatches = playedMatches.Count;
        TotalVictories = playedMatches.Count(m => m.IsVictory);
        CompaniesOwned = _companyService.CompanyCount;

        // Group matches by player company
        var matchesByCompany = playedMatches.GroupBy(m => m.PlayerCompanyId).ToDictionary(g => g.Key, g => g.ToList());
        var playedCompanies = await matchesByCompany.Keys
            .ToAsyncEnumerable()
            .Select(async (string id, CancellationToken _) => await _companyService.GetCompanyAsync(id, localOnly: true))
            .Where(x => x is not null)
            .ToDictionaryAsync(x => x!.Id, x => x!);
        var companyPlayCounts = playedCompanies.Values.ToDictionary(c => c, c => matchesByCompany[c.Id].Count);

        // Group matches by player faction
        var matchesByFaction = playedMatches.GroupBy(m => m.PlayerFaction).ToDictionary(g => g.Key, g => g.ToList());

        MostPlayedFaction = matchesByFaction.OrderByDescending(g => g.Value.Count).FirstOrDefault().Key ?? "N/A";
        MostPlayedScenario = playedMatches.GroupBy(m => m.PlayedMap).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "N/A";
        TotalPlayTime = FormatPlayTime(TimeSpan.FromSeconds(playedMatches.Sum(m => m.Duration.TotalSeconds)));

        // Grab the last three matches and create RecentMatchViewModel instances for each of them, then add them to the RecentMatches collection
        playedMatches.OrderByDescending(m => m.DatePlayed)
            .Take(3)
            .Select(x => MapToRecentMatchViewModel(x, playedCompanies))
            .ToList()
            .ForEach(RecentMatches.Add);

        // Grab the three most played companies and create FeaturedCompanyViewModel instances for each of them, then add them to the FeaturedCompanies collection
        if (companyPlayCounts.Count > 0) {
            companyPlayCounts.OrderByDescending(kv => kv.Value)
                .Take(2)
                .Select(kv => MapToFeaturedCompanyViewModel(kv.Key, kv.Value))
                .ToList()
                .ForEach(FeaturedCompanies.Add);
        } else {
            (await _companyService.GetLocalCompaniesAsync())
                .OrderByDescending(x => x.CreatedAt)
                .Take(2)
                .Select(x => MapToFeaturedCompanyViewModel(x, 0))
                .ToList()
                .ForEach(FeaturedCompanies.Add);
        }

        // Notify the view that the properties have changed so that it can update the UI
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeMessage)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalMatches)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalVictories)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WinRate)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPlayTime)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rank)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostPlayedFaction)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostPlayedScenario)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompaniesOwned)));

    }

    private static RecentMatchViewModel MapToRecentMatchViewModel(MatchPlayed match, Dictionary<string, Company> companies) {
        return new RecentMatchViewModel(
            GameId: match.GameId,
            CompanyFaction: match.PlayerFaction,
            CompanyName: companies.TryGetValue(match.PlayerCompanyId, out var company) ? company.Name : "Unknown Company",
            Map: match.PlayedMap,
            Victory: match.IsVictory,
            Timestamp: match.DatePlayed,
            Duration: match.Duration
        );
    }

    private static FeaturedCompanyViewModel MapToFeaturedCompanyViewModel(Company company, int playCount) {
        int veteranUnits = company.Squads.Where(x => x.Rank > 0).Count();
        return new FeaturedCompanyViewModel(company.Faction, company.Name, company.GameId, playCount, veteranUnits);
    }

    private static string FormatPlayTime(TimeSpan totalPlayTime) {
        int hours = (int)totalPlayTime.TotalHours;
        if (hours > 2) {
            if (hours > 10) {
                return $"{hours} hours"; // Drop minutes if playtime exceeds 10 hours for a cleaner display
            }
            return $"{hours} hours {totalPlayTime.Minutes} minutes";
        }
        return $"{(int)totalPlayTime.TotalMinutes} minutes";
    }

}
