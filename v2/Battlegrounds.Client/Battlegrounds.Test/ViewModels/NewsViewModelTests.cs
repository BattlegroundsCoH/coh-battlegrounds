using Battlegrounds.Helpers;
using Battlegrounds.Models.News;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.ViewModels.News;

using Microsoft.Extensions.Time.Testing;

using NSubstitute;

namespace Battlegrounds.Test.ViewModels;

[TestOf(typeof(NewsViewModel))]
public class NewsViewModelTests {

    private NewsViewModel _viewModel;
    private INewsService _newsService;
    private IImageCacheService _imageCacheService;
    private IBrowserService _browserService;
    private IDialogService _dialogService;
    private FakeTimeProvider _timeProvider;
    private TestLogger<NewsViewModel> _logger;

    [SetUp]
    public void SetUp() {
        _newsService = Substitute.For<INewsService>();
        _imageCacheService = Substitute.For<IImageCacheService>();
        _browserService = Substitute.For<IBrowserService>();
        _dialogService = Substitute.For<IDialogService>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero));
        _logger = new TestLogger<NewsViewModel>();
        _viewModel = new NewsViewModel(_logger, _newsService, _imageCacheService, _browserService, _dialogService, _timeProvider);
    }

    [TearDown]
    public void TearDown() {
        _newsService.ClearReceivedCalls();
        _imageCacheService.ClearReceivedCalls();
        _browserService.ClearReceivedCalls();
        _dialogService.ClearReceivedCalls();
        _logger.Dispose();
    }

    private static NewsArticle Article(string slug = "patch-1-2", string? coverImageUrl = null) => new(
        Id: "3f2a",
        Slug: slug,
        Title: "Patch 1.2",
        Description: "Balance pass",
        Category: "Patch",
        Author: "Ragnar",
        PublishedAt: new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Local),
        IsFeatured: false,
        CoverImageUrl: coverImageUrl,
        ArticleUrl: $"https://cohbattlegrounds.com/news/{slug}");

    private void GivenThePage(int page, int total, bool hasMore, params NewsArticle[] articles)
        => _newsService.GetPageAsync(page, NewsViewModel.PageSize, Arg.Any<CancellationToken>())
            .Returns(new NewsPage(articles, page, NewsViewModel.PageSize, total, hasMore));

    [Test]
    public async Task LoadPageAsync_PopulatesTheArticlesAndPagingState() {

        // Arrange
        GivenThePage(1, total: 27, hasMore: true, Article("a"), Article("b"));

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(_viewModel.Articles, Has.Count.EqualTo(2), "Both articles should be shown");
            Assert.That(_viewModel.CurrentPage, Is.EqualTo(1), "The current page should be reported");
            Assert.That(_viewModel.TotalPages, Is.EqualTo(3), "27 entries at 9 per page is three pages");
            Assert.That(_viewModel.HasMore, Is.True, "There should be more to fetch");
            Assert.That(_viewModel.PageLabel, Is.EqualTo("PAGE 1 OF 3"), "The pager label should read from the paging state");
            Assert.That(_viewModel.IsLoading, Is.False, "Loading should have finished");
        }

    }

    [Test]
    public async Task LoadPageAsync_ReplacesThePreviousPage() {

        // Arrange
        GivenThePage(1, total: 18, hasMore: true, Article("a"), Article("b"));
        GivenThePage(2, total: 18, hasMore: false, Article("c"));
        await _viewModel.LoadPageAsync(1);

        // Act
        await _viewModel.LoadPageAsync(2);

        // Assert
        Assert.That(_viewModel.Articles, Has.Count.EqualTo(1), "The previous page's articles should be gone");

    }

    [Test]
    public async Task LoadPageAsync_NeverRequestsAPageBelowOne() {

        // Arrange
        GivenThePage(1, total: 9, hasMore: false, Article());

        // Act
        await _viewModel.LoadPageAsync(0);

        // Assert
        await _newsService.Received(1).GetPageAsync(1, NewsViewModel.PageSize, Arg.Any<CancellationToken>());

    }

    [Test]
    public async Task LoadPageAsync_FetchesTheCoverOfEveryArticleThatHasOne() {

        // Arrange
        GivenThePage(1, total: 9, hasMore: false, Article("a", "https://api.example.com/cover-a"), Article("b"));

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(_viewModel.Articles, Has.Count.EqualTo(2), "Both articles should be shown");
        }
        await _imageCacheService.Received(1).GetImageAsync("https://api.example.com/cover-a", Arg.Any<CancellationToken>());
        await _imageCacheService.Received(1).GetImageAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

    }

    [Test]
    public async Task NextPageCommand_IsOnlyAvailableWhenThereIsMore() {

        // Arrange
        GivenThePage(1, total: 9, hasMore: false, Article());

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        Assert.That(_viewModel.NextPageCommand.CanExecute(null), Is.False, "There is nothing after the last page");

    }

    [Test]
    public async Task NextPageCommand_LoadsTheFollowingPage() {

        // Arrange
        GivenThePage(1, total: 18, hasMore: true, Article("a"));
        GivenThePage(2, total: 18, hasMore: false, Article("b"));
        await _viewModel.LoadPageAsync(1);

        // Act
        await _viewModel.NextPageCommand.ExecuteAsync(null);

        // Assert
        Assert.That(_viewModel.CurrentPage, Is.EqualTo(2), "The pager should have advanced");

    }

    [Test]
    public async Task PreviousPageCommand_IsUnavailableOnTheFirstPage() {

        // Arrange
        GivenThePage(1, total: 18, hasMore: true, Article());

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        Assert.That(_viewModel.PreviousPageCommand.CanExecute(null), Is.False, "There is nothing before the first page");

    }

    [Test]
    public async Task PreviousPageCommand_LoadsThePrecedingPage() {

        // Arrange
        GivenThePage(1, total: 18, hasMore: true, Article("a"));
        GivenThePage(2, total: 18, hasMore: false, Article("b"));
        await _viewModel.LoadPageAsync(2);

        // Act
        await _viewModel.PreviousPageCommand.ExecuteAsync(null);

        // Assert
        Assert.That(_viewModel.CurrentPage, Is.EqualTo(1), "The pager should have gone back");

    }

    [Test]
    public async Task HasPager_WhenEverythingFitsOnOnePage_IsFalse() {

        // Arrange
        GivenThePage(1, total: 5, hasMore: false, Article());

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        Assert.That(_viewModel.HasPager, Is.False, "A feed that fits on one page needs no pager");

    }

    [Test]
    public async Task HasPager_WhenThereIsMoreThanOnePage_IsTrue() {

        // Arrange
        GivenThePage(1, total: 12, hasMore: true, Article());

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        Assert.That(_viewModel.HasPager, Is.True, "Twelve entries at nine per page needs a pager");

    }

    [Test]
    public async Task HasPager_WhenTheRequestFails_IsFalse() {

        // Arrange — a dead backend must not leave a pager offering to page through nothing
        _newsService.GetPageAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(NewsPage.Empty(1, NewsViewModel.PageSize));

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        Assert.That(_viewModel.HasPager, Is.False, "There is nothing to page through");

    }

    [Test]
    public async Task RefreshSilentlyAsync_WhenTheFeedIsUnchanged_LeavesTheTilesAlone() {

        // Arrange — rebuilding the collection resets the scroll position and drops the hover, so an
        // unchanged feed must not touch it at all
        GivenThePage(1, total: 9, hasMore: false, Article("a"), Article("b"));
        await _viewModel.LoadPageAsync(1);
        var shownBefore = _viewModel.Articles.ToArray();

        // Act
        await _viewModel.RefreshSilentlyAsync();

        // Assert
        Assert.That(_viewModel.Articles, Is.EqualTo(shownBefore).AsCollection,
            "The very same item view-models should still be bound");

    }

    [Test]
    public async Task RefreshSilentlyAsync_WhenAnArticleIsPublished_ShowsIt() {

        // Arrange
        GivenThePage(1, total: 9, hasMore: false, Article("a"));
        await _viewModel.LoadPageAsync(1);
        GivenThePage(1, total: 18, hasMore: true, Article("new"), Article("a"));

        // Act
        await _viewModel.RefreshSilentlyAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(_viewModel.Articles, Has.Count.EqualTo(2), "The new article should have appeared");
            Assert.That(_viewModel.HasMore, Is.True, "The paging state should have been updated too");
            Assert.That(_viewModel.TotalPages, Is.EqualTo(2), "Eighteen entries at nine per page is two pages");
        }

    }

    [Test]
    public async Task RefreshSilentlyAsync_NeverRaisesIsLoading() {

        // Arrange — IsLoading gates the pager's CanExecute, so a poll that raised it would grey
        // PREVIOUS and NEXT out under the user's pointer every couple of minutes
        GivenThePage(1, total: 18, hasMore: true, Article("a"));
        await _viewModel.LoadPageAsync(1);
        bool wasEverLoading = false;
        _viewModel.PropertyChanged += (_, e) => wasEverLoading |= e.PropertyName == nameof(NewsViewModel.IsLoading);

        // Act
        await _viewModel.RefreshSilentlyAsync();

        // Assert
        Assert.That(wasEverLoading, Is.False, "A background refresh is not a loading state");

    }

    [Test]
    public async Task RefreshSilentlyAsync_WhenThePagerHasMovedOn_DoesNotRequestAnything() {

        // Arrange — publishing an entry shifts every later page by one, so silently re-reading page
        // two would swap articles out from under someone mid-read
        GivenThePage(1, total: 18, hasMore: true, Article("a"));
        GivenThePage(2, total: 18, hasMore: false, Article("b"));
        await _viewModel.LoadPageAsync(2);
        _newsService.ClearReceivedCalls();

        // Act
        await _viewModel.RefreshSilentlyAsync();

        // Assert
        await _newsService.DidNotReceive().GetPageAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());

    }

    [Test]
    public async Task RefreshSilentlyAsync_WhenTheRequestFails_KeepsWhatIsShown() {

        // Arrange — GetPageAsync reports a failed request as an empty page, and blanking a populated
        // grid because the backend blinked is worse than showing data a couple of minutes old
        GivenThePage(1, total: 9, hasMore: false, Article("a"), Article("b"));
        await _viewModel.LoadPageAsync(1);
        _newsService.GetPageAsync(1, NewsViewModel.PageSize, Arg.Any<CancellationToken>())
            .Returns(NewsPage.Empty(1, NewsViewModel.PageSize));

        // Act
        await _viewModel.RefreshSilentlyAsync();

        // Assert
        Assert.That(_viewModel.Articles, Has.Count.EqualTo(2), "The articles should have survived the failed poll");

    }

    [Test]
    public async Task LastUpdatedLabel_ReportsWhenTheFeedWasLastRead() {

        // Arrange
        GivenThePage(1, total: 9, hasMore: false, Article("a"));

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert — the caption replaces what used to be a REFRESH button
        using (Assert.EnterMultipleScope()) {
            Assert.That(_viewModel.HasLastUpdated, Is.True, "The page has been read");
            Assert.That(_viewModel.LastUpdatedLabel,
                Is.EqualTo($"UPDATED {_timeProvider.GetUtcNow().ToLocalTime():HH:mm}"),
                "The caption should read the clock, in local time");
        }

    }

    [Test]
    public void LastUpdatedLabel_BeforeAnythingIsLoaded_IsEmpty() {

        // Assert — nothing to claim about freshness yet, so the header shows no caption at all
        using (Assert.EnterMultipleScope()) {
            Assert.That(_viewModel.HasLastUpdated, Is.False, "Nothing has been read");
            Assert.That(_viewModel.LastUpdatedLabel, Is.Empty, "There is no time to report");
        }

    }

    [Test]
    public async Task RefreshSilentlyAsync_WhenNothingChanged_StillMovesTheLastUpdatedTime() {

        // Arrange — the caption claims the page is current as of that time, not that something changed
        GivenThePage(1, total: 9, hasMore: false, Article("a"));
        await _viewModel.LoadPageAsync(1);
        _timeProvider.Advance(TimeSpan.FromMinutes(2));

        // Act
        await _viewModel.RefreshSilentlyAsync();

        // Assert
        Assert.That(_viewModel.LastUpdated, Is.EqualTo(_timeProvider.GetUtcNow()),
            "The feed was re-read, even though it turned out to be identical");

    }

    [Test]
    public async Task LoadPageAsync_WhenTheRequestFails_ShowsTheEmptyState() {

        // Arrange
        _newsService.GetPageAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(NewsPage.Empty(1, NewsViewModel.PageSize));

        // Act
        await _viewModel.LoadPageAsync(1);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(_viewModel.Articles, Is.Empty, "Nothing should be shown");
            Assert.That(_viewModel.HasMore, Is.False, "There should be nothing more to fetch");
            Assert.That(_viewModel.IsLoading, Is.False, "Loading should have finished even though the request failed");
        }

    }

}

[TestOf(typeof(NewsItemViewModel))]
public class NewsItemViewModelTests {

    private IImageCacheService _imageCacheService;
    private IBrowserService _browserService;
    private IDialogService _dialogService;
    private TestLogger<NewsItemViewModel> _logger;

    [SetUp]
    public void SetUp() {
        _imageCacheService = Substitute.For<IImageCacheService>();
        _browserService = Substitute.For<IBrowserService>();
        _dialogService = Substitute.For<IDialogService>();
        // Approved by default; the tests that care about the prompt say otherwise explicitly.
        _dialogService.ShowConfirmationAsync(Arg.Any<DialogType>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(DialogResult.Yes);
        _logger = new TestLogger<NewsItemViewModel>();
    }

    [TearDown]
    public void TearDown() {
        _imageCacheService.ClearReceivedCalls();
        _browserService.ClearReceivedCalls();
        _dialogService.ClearReceivedCalls();
        _logger.Dispose();
    }

    private NewsItemViewModel CreateViewModel(string? coverImageUrl = null, bool isFeatured = false, string articleUrl = "https://cohbattlegrounds.com/news/patch-1-2") => new(
        new NewsArticle(
            Id: "3f2a",
            Slug: "patch-1-2",
            Title: "Patch 1.2",
            Description: "Balance pass",
            Category: "Patch",
            Author: "Ragnar",
            PublishedAt: new DateTime(2026, 7, 19, 12, 0, 0, DateTimeKind.Local),
            IsFeatured: isFeatured,
            CoverImageUrl: coverImageUrl,
            ArticleUrl: articleUrl),
        _imageCacheService,
        _browserService,
        _dialogService,
        _logger);

    [Test]
    public void IsFeatured_TracksTheArticle() {

        // Assert — this drives the badge over the cover, not the tile's size
        using (Assert.EnterMultipleScope()) {
            Assert.That(CreateViewModel(isFeatured: true).IsFeatured, Is.True, "A featured entry should say so");
            Assert.That(CreateViewModel().IsFeatured, Is.False, "An ordinary entry should not");
        }

    }

    [Test]
    public async Task OpenCommand_WhenConfirmed_OpensTheArticleOnTheWebsite() {

        // Arrange
        var viewModel = CreateViewModel();

        // Act
        await viewModel.OpenCommand.ExecuteAsync(null);

        // Assert
        _browserService.Received(1).OpenUrl("https://cohbattlegrounds.com/news/patch-1-2");

    }

    [Test]
    public async Task OpenCommand_AsksBeforeLeavingTheApp() {

        // Arrange
        var viewModel = CreateViewModel();

        // Act
        await viewModel.OpenCommand.ExecuteAsync(null);

        // Assert — a yes/no prompt naming the article and the destination host, so the user knows
        // both that they are leaving and where they are going
        await _dialogService.Received(1).ShowConfirmationAsync(
            DialogType.YesNo,
            Arg.Any<string>(),
            Arg.Is<string>(x => x.Contains("Patch 1.2") && x.Contains("cohbattlegrounds.com")));

    }

    [TestCase(DialogResult.No, TestName = "OpenCommand_WhenDeclined_DoesNotOpenTheBrowser")]
    [TestCase(DialogResult.Cancel, TestName = "OpenCommand_WhenCancelled_DoesNotOpenTheBrowser")]
    public async Task OpenCommand_WhenNotConfirmed_DoesNotOpenTheBrowser(DialogResult answer) {

        // Arrange
        _dialogService.ShowConfirmationAsync(Arg.Any<DialogType>(), Arg.Any<string?>(), Arg.Any<string?>())
            .Returns(answer);
        var viewModel = CreateViewModel();

        // Act
        await viewModel.OpenCommand.ExecuteAsync(null);

        // Assert
        _browserService.DidNotReceive().OpenUrl(Arg.Any<string>());

    }

    [Test]
    public async Task OpenCommand_WhenTheArticleUrlIsMalformed_StillNamesSomethingInThePrompt() {

        // Arrange — the website address is developer-editable configuration, so it is not
        // guaranteed to parse as an absolute URI and must not turn a click into a crash
        var viewModel = CreateViewModel(articleUrl: "not a url");

        // Act
        await viewModel.OpenCommand.ExecuteAsync(null);

        // Assert
        await _dialogService.Received(1).ShowConfirmationAsync(
            DialogType.YesNo,
            Arg.Any<string>(),
            Arg.Is<string>(x => x.Contains("not a url")));
        _browserService.Received(1).OpenUrl("not a url");

    }

    [Test]
    public void OpenCommand_WhenTheBrowserCannotBeLaunched_DoesNotThrow() {

        // Arrange — BrowserService throws when the shell cannot handle the URL, and an escaped
        // exception out of a command would take the app down over a failed click
        var viewModel = CreateViewModel();
        _browserService.When(x => x.OpenUrl(Arg.Any<string>()))
            .Do(_ => throw new InvalidOperationException("No browser"));

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await viewModel.OpenCommand.ExecuteAsync(null),
            "A failed browser launch should be logged, not thrown");

    }

    [Test]
    public async Task LoadCoverImageAsync_WhenThereIsNoCover_DoesNotAskTheCache() {

        // Arrange
        var viewModel = CreateViewModel(coverImageUrl: null);

        // Act
        await viewModel.LoadCoverImageAsync();

        // Assert
        Assert.That(viewModel.CoverImage, Is.Null, "There is no cover to show");
        await _imageCacheService.DidNotReceive().GetImageAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

    }

    [Test]
    public async Task LoadCoverImageAsync_RequestsTheCoverFromTheCache() {

        // Arrange
        var viewModel = CreateViewModel("https://api.example.com/cover-a");

        // Act
        await viewModel.LoadCoverImageAsync();

        // Assert
        await _imageCacheService.Received(1).GetImageAsync("https://api.example.com/cover-a", Arg.Any<CancellationToken>());

    }

}
