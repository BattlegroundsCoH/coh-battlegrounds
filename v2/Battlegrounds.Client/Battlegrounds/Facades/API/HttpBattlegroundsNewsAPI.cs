using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

public sealed class HttpBattlegroundsNewsAPI(
    ILogger<HttpBattlegroundsNewsAPI> logger,
    IAsyncHttpClient asyncHttpClient,
    Configuration configuration) : IBattlegroundsNewsAPI {

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public static readonly string LatestNewsEndpoint = "/api/v1/news/";

    public static readonly string NewsPageEndpoint = "/api/v1/news/page";

    /// <summary>
    /// Deliberately <i>unversioned</i>: the API stores this exact path inside article markdown,
    /// so it must stay stable across versions.
    /// </summary>
    public static readonly string NewsResourceEndpoint = "/api/news/resources";

    private readonly ILogger<HttpBattlegroundsNewsAPI> _logger = logger;
    private readonly IAsyncHttpClient _httpClient = asyncHttpClient;
    private readonly Configuration _configuration = configuration;

    private string BaseUrl => _configuration.API.BaseUrl.TrimEnd('/');

    public string GetResourceUrl(string resourceId) => $"{BaseUrl}{NewsResourceEndpoint}/{Uri.EscapeDataString(resourceId)}";

    public async Task<IReadOnlyList<NewsPreviewResponse>> GetLatestNewsAsync() {

        string endpoint = $"{BaseUrl}{LatestNewsEndpoint}";
        _logger.LogDebug("Retrieving latest news from {Endpoint}", endpoint);

        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Failed to retrieve latest news. Status code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return [];
        }

        try {
            return await response.Content.ReadFromJsonAsync<List<NewsPreviewResponse>>(_jsonOptions) ?? [];
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to deserialize the latest news response.");
            return [];
        }

    }

    public async Task<PagedNewsResponse?> GetNewsPageAsync(int page, int pageSize) {

        string endpoint = $"{BaseUrl}{NewsPageEndpoint}?page={page}&pageSize={pageSize}";
        _logger.LogDebug("Retrieving news page from {Endpoint}", endpoint);

        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Failed to retrieve news page {Page}. Status code: {StatusCode}, Reason: {ReasonPhrase}", page, response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try {
            return await response.Content.ReadFromJsonAsync<PagedNewsResponse>(_jsonOptions);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to deserialize the news page {Page} response.", page);
            return null;
        }

    }

    public async Task<byte[]?> DownloadResourceAsync(string resourceId) {

        string endpoint = GetResourceUrl(resourceId);
        _logger.LogDebug("Downloading news resource from {Endpoint}", endpoint);

        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Failed to download news resource {ResourceId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", resourceId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try {
            return await response.Content.ReadAsByteArrayAsync();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to read the content of news resource {ResourceId}.", resourceId);
            return null;
        }

    }

}
