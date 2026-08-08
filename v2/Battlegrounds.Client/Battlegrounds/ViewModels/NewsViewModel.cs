using System.Collections.ObjectModel;
using System.ComponentModel;

using Battlegrounds.Helpers;
using Battlegrounds.Models.News;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.News;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed class NewsViewModel : INotifyPropertyChanged {

    public const int PageSize = 9;

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ILogger<NewsViewModel> _logger;
    private readonly INewsService _newsService;
    private readonly IImageCacheService _imageCacheService;
    private readonly IBrowserService _browserService;
    private readonly IDialogService _dialogService;
    private readonly TimeProvider _timeProvider;

    private PeriodicRefresh? _autoRefresh;

    private bool _isLoading;
    private bool _hasMore;
    private bool _hasLoaded;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private DateTimeOffset? _lastUpdated;

    public ObservableCollection<NewsItemViewModel> Articles { get; } = [];

    public bool IsLoading {
        get => _isLoading;
        private set {
            _isLoading = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            NextPageCommand.NotifyCanExecuteChanged();
            PreviousPageCommand.NotifyCanExecuteChanged();
        }
    }

    public int CurrentPage {
        get => _currentPage;
        private set {
            _currentPage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageLabel)));
            PreviousPageCommand.NotifyCanExecuteChanged();
        }
    }

    public int TotalPages {
        get => _totalPages;
        private set {
            _totalPages = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPages)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasPager)));
        }
    }

    public bool HasPager => TotalPages > 1;

    public bool HasMore {
        get => _hasMore;
        private set {
            _hasMore = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasMore)));
            NextPageCommand.NotifyCanExecuteChanged();
        }
    }

    public string PageLabel => $"PAGE {CurrentPage} OF {TotalPages}";

    public DateTimeOffset? LastUpdated {
        get => _lastUpdated;
        private set {
            _lastUpdated = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdated)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasLastUpdated)));
        }
    }

    public string LastUpdatedLabel
        => LastUpdated is { } timestamp ? $"UPDATED {timestamp.ToLocalTime():HH:mm}" : string.Empty;

    public bool HasLastUpdated => LastUpdated is not null;

    public IAsyncRelayCommand NextPageCommand { get; }

    public IAsyncRelayCommand PreviousPageCommand { get; }

    public NewsViewModel(
        ILogger<NewsViewModel> logger,
        INewsService newsService,
        IImageCacheService imageCacheService,
        IBrowserService browserService,
        IDialogService dialogService,
        TimeProvider timeProvider) {
        _logger = logger;
        _newsService = newsService;
        _imageCacheService = imageCacheService;
        _browserService = browserService;
        _dialogService = dialogService;
        _timeProvider = timeProvider;
        NextPageCommand = new AsyncRelayCommand(() => LoadPageAsync(CurrentPage + 1), () => !IsLoading && HasMore);
        PreviousPageCommand = new AsyncRelayCommand(() => LoadPageAsync(CurrentPage - 1), () => !IsLoading && CurrentPage > 1);
    }

    public async void OnViewActivated() {
        if (_hasLoaded) {
            return;
        }
        _hasLoaded = true;
        try {
            await LoadPageAsync(1);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to load the first page of news.");
        }
    }

    public void StartAutoRefresh() {
        _autoRefresh ??= new PeriodicRefresh(Consts.NewsRefreshInterval, RefreshSilentlyAsync, _logger);
        _autoRefresh.Start();
    }

    public void StopAutoRefresh() => _autoRefresh?.Stop();

    /// <summary>
    /// Re-reads the first page on the timer and applies it only if it actually differs.
    /// </summary>
    public async Task RefreshSilentlyAsync(CancellationToken ct = default) {

        if (CurrentPage != 1) {
            return;
        }

        var result = await _newsService.GetPageAsync(1, PageSize, ct);
        if (result.Items.Count == 0 && Articles.Count > 0) {
            return;
        }

        TotalPages = result.TotalPages;
        HasMore = result.HasMore;
        LastUpdated = _timeProvider.GetUtcNow();

        if (NewsItemViewModel.Matches(Articles, result.Items)) {
            return;
        }

        await ShowArticlesAsync(result.Items, ct);

    }

    public async Task LoadPageAsync(int page, CancellationToken ct = default) {

        IsLoading = true;
        try {

            var result = await _newsService.GetPageAsync(Math.Max(page, 1), PageSize, ct);

            CurrentPage = result.Page;
            TotalPages = result.TotalPages;
            HasMore = result.HasMore;

            if (result.Items.Count > 0) {
                LastUpdated = _timeProvider.GetUtcNow();
            }

            await ShowArticlesAsync(result.Items, ct);

        } finally {
            IsLoading = false;
        }

    }

    private async Task ShowArticlesAsync(IReadOnlyList<NewsArticle> articles, CancellationToken ct) {

        Articles.Clear();
        var items = articles.Select(x => new NewsItemViewModel(x, _imageCacheService, _browserService, _dialogService, _logger)).ToList();
        items.ForEach(Articles.Add);

        await Task.WhenAll(items.Select(x => x.LoadCoverImageAsync(ct)));

    }

}
