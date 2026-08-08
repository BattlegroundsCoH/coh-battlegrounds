using System.Collections.ObjectModel;
using System.ComponentModel;

using Battlegrounds.Helpers;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.News;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Statistics;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.News;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public record RecentMatchViewModel(string GameId, string CompanyFaction, string CompanyName, string Map, bool Victory, DateTime Timestamp, TimeSpan Duration);

public record FeaturedCompanyViewModel(string CompanyFaction, string CompanyName, string GameId, int PlayCount, int VeteranUnits);

public sealed class HomeViewModel(
    ILogger<HomeViewModel> logger,
    IStatisticsService statisticsService,
    ICompanyService companyService,
    IUpdateService updateService,
    INewsService newsService,
    IImageCacheService imageCacheService,
    IBrowserService browserService,
    IDialogService dialogService) : INotifyPropertyChanged {

    /// <summary>How many previews the dashboard card has room for.</summary>
    private const int NewsPreviewCount = 3;

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly TaskCompletionSource _dataLoadedCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ILogger<HomeViewModel> _logger = logger;
    private readonly IStatisticsService _statisticsService = statisticsService;
    private readonly ICompanyService _companyService = companyService;
    private readonly IUpdateService _updateService = updateService;
    private readonly INewsService _newsService = newsService;
    private readonly IImageCacheService _imageCacheService = imageCacheService;
    private readonly IBrowserService _browserService = browserService;
    private readonly IDialogService _dialogService = dialogService;

    private bool _isInitialized = false;

    private PeriodicRefresh? _newsRefresh;

    private string _welcomeMessage = "Welcome back, Commander!";

    public string WelcomeMessage => _welcomeMessage;

    public bool IsUpdateAvailable { get; private set; } = false;
    public string NewVersionNumber { get; private set; } = string.Empty;

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

    public ObservableCollection<NewsItemViewModel> NewsAndUpdates { get; } = [];

    public IAsyncRelayCommand InstallUpdateCommand => new AsyncRelayCommand(InstallUpdate, () => IsUpdateAvailable);

    public void OnDataLoaded() {
        _dataLoadedCompletionSource.SetResult();
    }

    public void UpdateUser(User user) {
        _welcomeMessage = $"Welcome back, {user?.UserDisplayName ?? "Commander"}!";
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeMessage)));
    }

    public void OnViewActivated() {
        OnViewModelInitialized();
        LoadNews();
    }

    /// <summary>
    /// Starts re-requesting the news feed every <see cref="Consts.NewsRefreshInterval"/>.
    /// </summary>
    /// <remarks>Driven by the view's visibility rather than its construction, because this view-model
    /// is a singleton and the view is not: the dashboard is also merely <i>hidden</i> — not
    /// unloaded — while a lobby is open, so an activation hook alone would leave it polling behind a
    /// screen nobody is looking at. Safe to call when already running.</remarks>
    public void StartNewsAutoRefresh() {
        _newsRefresh ??= new PeriodicRefresh(Consts.NewsRefreshInterval, RefreshNewsSilentlyAsync, _logger);
        _newsRefresh.Start();
    }

    /// <summary>Stops the automatic refresh and abandons a request that is still in flight.</summary>
    public void StopNewsAutoRefresh() => _newsRefresh?.Stop();

    private async void OnViewModelInitialized() {

        // Wait for the statistics service to load the data before updating the properties
        await Task.WhenAll(_statisticsService.IsLoaded, _dataLoadedCompletionSource.Task);

        await UpdateData();

        _isInitialized = true;

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

    public void NotifyUpdateAvailable(string version) {
        NewVersionNumber = version;
        IsUpdateAvailable = true;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdateAvailable)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NewVersionNumber)));
        InstallUpdateCommand.NotifyCanExecuteChanged();
    }

    private async Task InstallUpdate() {
        await _updateService.DownloadAndInstallUpdate();
    }

    public async void Refresh() {

        LoadNews();

        if (!_isInitialized) {
            return;
        }

        // Refresh logic here
        await UpdateData();

    }

    /// <summary>
    /// Fills the dashboard's news card.
    /// </summary>
    /// <remarks>Deliberately not part of <see cref="UpdateData"/>: that runs only once
    /// <see cref="IStatisticsService.IsLoaded"/> and the replay/company load have both completed, and
    /// gating the news feed on either would leave the card empty for seconds on a cold start.
    /// <para>A failed fetch leaves the collection empty, which the view renders as its empty
    /// state — the client never surfaces a transport error.</para></remarks>
    public async Task LoadNewsAsync(bool forceRefresh = false, CancellationToken ct = default) {
        var articles = await _newsService.GetLatestAsync(NewsPreviewCount, forceRefresh, ct);
        await ShowNewsAsync(articles, ct);
    }

    /// <summary>
    /// Re-requests the feed on the timer and shows it only if it actually differs.
    /// </summary>
    /// <remarks>Bypasses the service's cache — that cache exists so returning to the dashboard does not
    /// re-request the feed.
    /// <para>An empty result is left on the floor rather than shown: the service cannot distinguish a
    /// failed request from a genuinely empty feed.</para></remarks>
    private async Task RefreshNewsSilentlyAsync(CancellationToken ct) {

        var articles = await _newsService.GetLatestAsync(NewsPreviewCount, forceRefresh: true, ct);
        if (articles.Count == 0 || NewsItemViewModel.Matches(NewsAndUpdates, articles)) {
            return;
        }

        await ShowNewsAsync(articles, ct);

    }

    private async Task ShowNewsAsync(IReadOnlyList<NewsArticle> articles, CancellationToken ct) {

        NewsAndUpdates.Clear();
        var items = articles.Select(x => new NewsItemViewModel(x, _imageCacheService, _browserService, _dialogService, _logger)).ToList();
        items.ForEach(NewsAndUpdates.Add);

        // Covers are fetched after the rows are bound, so the titles appear immediately and each
        // thumbnail fades in as it lands rather than the whole card waiting on the slowest image.
        await Task.WhenAll(items.Select(x => x.LoadCoverImageAsync(ct)));

    }

    private async void LoadNews() {
        try {
            await LoadNewsAsync();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to load the news feed for the dashboard.");
        }
    }

    private async Task UpdateData() {

        // Clean up existing data
        RecentMatches.Clear();
        FeaturedCompanies.Clear();

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

}
