using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

public sealed class HttpBattlegroundsWebAPI(
    ILogger<HttpBattlegroundsWebAPI> logger,
    IAsyncHttpClient asyncHttpClient,
    Configuration configuration) : IBattlegroundsWebAPI {

    private static readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ILogger<HttpBattlegroundsWebAPI> _logger = logger;
    private readonly IAsyncHttpClient _httpClient = asyncHttpClient;
    private readonly Configuration _configuration = configuration;

    private string _authToken = string.Empty;

    public string LoginEndpoint => $"{_configuration.API.LoginUrlOverride}{_configuration.API.LoginEndpoint}";
    public string RefreshEndpoint => $"{_configuration.API.LoginUrlOverride}{_configuration.API.RefreshEndpoint}";
    public string PublicKeyEndpoint => $"{_configuration.API.LoginUrlOverride}{_configuration.API.PublicKeyEndpoint}";


    public async Task<LoginResponse> LoginAsync(LoginRequest request) {
        _logger.LogDebug("Logging in using {Endpoint}", LoginEndpoint);
        HttpRequestMessage requestMessage = new(HttpMethod.Post, LoginEndpoint) {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };
        HttpResponseMessage response = await _httpClient.SendRequestAsync(requestMessage);
        if (!response.IsSuccessStatusCode) {
            //string errorContent = await response.Content.ReadAsStringAsync() ?? string.Empty; // TODO: Log this...
            throw new HttpRequestException($"Login failed with status code {response.StatusCode}.");
        }

        Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");
        return await FromJson<LoginResponse>(contentStream) ?? throw new InvalidOperationException("Failed to deserialize login response.");
    }

    public async Task<RefreshResponse> RefreshTokenAsync(RefreshRequest request) {
        _logger.LogDebug("Refreshing token using {Endpoint}", RefreshEndpoint);
        HttpRequestMessage requestMessage = new(HttpMethod.Post, RefreshEndpoint) {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };
        HttpResponseMessage response = await _httpClient.SendRequestAsync(requestMessage);
        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"Token refresh failed with status code {response.StatusCode}.");
        }
        Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");
        return await FromJson<RefreshResponse>(contentStream) ?? throw new InvalidOperationException("Failed to deserialize refresh response.");
    }

    private static ValueTask<T?> FromJson<T>(Stream source) {
        if (source is null) {
            throw new ArgumentNullException(nameof(source), "Source stream cannot be null.");
        }
        return JsonSerializer.DeserializeAsync<T>(source, _jsonOptions);
    }

    public void SetAuthenticationToken(string token) => _authToken = token;

    public async Task<string> GetPublicKeyAsync() {
        _logger.LogDebug("Retrieving public key from {Endpoint}", PublicKeyEndpoint);
        HttpRequestMessage requestMessage = new(HttpMethod.Get, PublicKeyEndpoint);
        HttpResponseMessage response = await _httpClient.SendRequestAsync(requestMessage);
        if (!response.IsSuccessStatusCode) {
            throw new HttpRequestException($"Failed to retrieve public key with status code {response.StatusCode}.");
        }
        _logger.LogDebug("Public key retrieved successfully.");
        return await response.Content.ReadAsStringAsync();
    }

}
