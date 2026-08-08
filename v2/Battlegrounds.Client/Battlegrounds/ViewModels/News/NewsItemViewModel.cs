using System.ComponentModel;
using System.Windows.Media;

using Battlegrounds.Helpers;
using Battlegrounds.Models.News;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.Modals;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels.News;

public sealed class NewsItemViewModel : INotifyPropertyChanged {

    private readonly IImageCacheService _imageCacheService;
    private readonly IBrowserService _browserService;
    private readonly IDialogService _dialogService;
    private readonly ILogger _logger;
    private readonly NewsArticle _article;

    private ImageSource? _coverImage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => _article.Title;

    public string Description => _article.Description;

    public string Category => _article.Category;

    public string Author => _article.Author;

    public DateTime PublishedAt => _article.PublishedAt;

    public string ArticleUrl => _article.ArticleUrl;

    public bool IsFeatured => _article.IsFeatured;

    public ImageSource? CoverImage {
        get => _coverImage;
        private set {
            _coverImage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoverImage)));
        }
    }

    public IAsyncRelayCommand OpenCommand { get; }

    /// <summary>
    /// Whether a freshly fetched feed is identical to the one already on screen.
    /// </summary>
    public static bool Matches(IReadOnlyList<NewsItemViewModel> shown, IReadOnlyList<NewsArticle> fetched)
        => shown.Count == fetched.Count && shown.Select(x => x._article).SequenceEqual(fetched);

    public NewsItemViewModel(NewsArticle article, IImageCacheService imageCacheService, IBrowserService browserService, IDialogService dialogService, ILogger logger) {
        _article = article;
        _imageCacheService = imageCacheService;
        _browserService = browserService;
        _dialogService = dialogService;
        _logger = logger;
        OpenCommand = new AsyncRelayCommand(OpenArticle);
    }
    
    public async Task LoadCoverImageAsync(CancellationToken ct = default) {
        if (_article.CoverImageUrl is not string url) {
            return;
        }
        CoverImage = await _imageCacheService.GetImageAsync(url, ct);
    }

    /// <summary>
    /// Confirms the hand-off, then opens the article in the user's browser.
    /// </summary>
    /// <remarks>The client shows previews only, so every click here leaves the app for the website.
    /// That is worth asking about rather than doing silently — a tile looks like in-app content, and
    /// nothing else in the launcher launches an external program from a plain click.
    /// <para>The destination host is named in the prompt rather than described vaguely, so the answer
    /// is informed by where the user is actually going.</para></remarks>
    private async Task OpenArticle() {

        var answer = await _dialogService.ShowConfirmationAsync(
            DialogType.YesNo,
            "Open in your browser?",
            $"\"{Title}\" will open at {DestinationHost} in your default web browser. " +
            "Battlegrounds stays running in the background.");

        if (answer is not DialogResult.Yes) {
            return;
        }

        try {
            _browserService.OpenUrl(_article.ArticleUrl);
        } catch (Exception ex) {
            // BrowserService throws when the shell cannot handle the URL. Letting that escape a
            // command would take the app down over a failed click.
            _logger.LogError(ex, "Failed to open the news article {ArticleUrl}.", _article.ArticleUrl);
        }

    }

    /// <summary>
    /// The host the article lives on, for naming in the confirmation prompt.
    /// </summary>
    /// <remarks>Falls back to the whole URL rather than throwing: the website address is a
    /// developer-editable configuration value, so it is not guaranteed to parse as an absolute URI
    /// and a malformed one must not turn a click into a crash.</remarks>
    private string DestinationHost
        => Uri.TryCreate(_article.ArticleUrl, UriKind.Absolute, out Uri? uri) ? uri.Host : _article.ArticleUrl;

}
