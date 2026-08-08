using System.Text.Json.Serialization;

namespace Battlegrounds.Facades.API;

/// <summary>
/// A single news entry as returned by the feed and paging endpoints, without its markdown body.
/// </summary>
/// <remarks>
/// <paramref name="Resources"/> holds resource <i>identifiers</i>, not URLs — the API returns no
/// image URL at all. Index 0 is the cover by convention, and the list may be empty. Build the URL
/// with <see cref="IBattlegroundsNewsAPI.GetResourceUrl(string)"/>.
/// <para>
/// <paramref name="PublishedAt"/> and <paramref name="CreatedAt"/> come off <c>timestamp</c>
/// columns and serialize with neither a <c>Z</c> nor an offset, so they deserialize as
/// <see cref="DateTimeKind.Unspecified"/> despite being UTC. Callers must say so explicitly.
/// </para></remarks>
public sealed record NewsPreviewResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("authorRole")] string AuthorRole,
    [property: JsonPropertyName("isFeatured")] bool IsFeatured,
    [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("resources")] IReadOnlyList<string> Resources
);

/// <summary>
/// One page of news entries.
/// </summary>
public sealed record PagedNewsResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<NewsPreviewResponse> Items,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("hasMore")] bool HasMore
);

public interface IBattlegroundsNewsAPI {

    Task<IReadOnlyList<NewsPreviewResponse>> GetLatestNewsAsync();

    Task<PagedNewsResponse?> GetNewsPageAsync(int page, int pageSize);

    Task<byte[]?> DownloadResourceAsync(string resourceId);
    
    string GetResourceUrl(string resourceId);

}
