namespace Battlegrounds.Models.News;

public sealed record NewsArticle(
    string Id,
    string Slug,
    string Title,
    string Description,
    string Category,
    string Author,
    DateTime PublishedAt,
    bool IsFeatured,
    string? CoverImageUrl,
    string ArticleUrl
);

public sealed record NewsPage(
    IReadOnlyList<NewsArticle> Items,
    int Page,
    int PageSize,
    int Total,
    bool HasMore
) {

    public static NewsPage Empty(int page, int pageSize) => new([], page, pageSize, 0, false);
    
    public int TotalPages => Total <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);

}
