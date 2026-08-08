using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.News;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

public sealed class NewsService(
    ILogger<NewsService> logger,
    IBattlegroundsWebAPI webApi,
    Configuration configuration,
    TimeProvider timeProvider) : INewsService {

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ILogger<NewsService> _logger = logger;
    private readonly IBattlegroundsWebAPI _webApi = webApi;
    private readonly Configuration _configuration = configuration;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly SemaphoreSlim _latestLock = new(1, 1);

    private IReadOnlyList<NewsArticle>? _latest;
    private DateTimeOffset _latestFetchedAt;

    public async Task<IReadOnlyList<NewsArticle>> GetLatestAsync(int count, bool forceRefresh = false, CancellationToken ct = default) {

        await _latestLock.WaitAsync(ct);
        try {

            bool isStale = _latest is null || _timeProvider.GetUtcNow() - _latestFetchedAt >= CacheDuration;
            if (forceRefresh || isStale) {
                var response = await _webApi.GetLatestNewsAsync();
                if (response.Count > 0) {
                    _latest = [.. response.Select(MapToArticle).OrderByDescending(x => x.PublishedAt)];
                    _latestFetchedAt = _timeProvider.GetUtcNow();
                } else if (_latest is null) {
                    _logger.LogWarning("The news feed returned no entries.");
                    return [];
                }
            }

            return [.. _latest!.Take(count)];

        } finally {
            _latestLock.Release();
        }

    }

    public async Task<NewsPage> GetPageAsync(int page, int pageSize, CancellationToken ct = default) {

        var response = await _webApi.GetNewsPageAsync(page, pageSize);
        if (response is null) {
            return NewsPage.Empty(page, pageSize);
        }

        return new NewsPage(
            Items: [.. response.Items.Select(MapToArticle).OrderByDescending(x => x.PublishedAt)],
            Page: response.Page,
            PageSize: response.PageSize,
            Total: response.Total,
            HasMore: response.HasMore);

    }

    private NewsArticle MapToArticle(NewsPreviewResponse response) => new(
        Id: response.Id,
        Slug: response.Slug,
        Title: response.Title,
        Description: response.Description,
        Category: response.Category,
        Author: response.Author,
        PublishedAt: ToLocalTime(response.PublishedAt ?? response.CreatedAt),
        IsFeatured: response.IsFeatured,
        CoverImageUrl: GetCoverImageUrl(response),
        ArticleUrl: GetArticleUrl(response.Slug));

    private string? GetCoverImageUrl(NewsPreviewResponse response)
        => response.Resources is { Count: > 0 } resources ? _webApi.GetResourceUrl(resources[0]) : null;

    private string GetArticleUrl(string slug)
        => $"{_configuration.WebsiteUrl.TrimEnd('/')}/news/{Uri.EscapeDataString(slug)}";
    
    private static DateTime ToLocalTime(DateTime timestamp)
        => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc).ToLocalTime();

}
