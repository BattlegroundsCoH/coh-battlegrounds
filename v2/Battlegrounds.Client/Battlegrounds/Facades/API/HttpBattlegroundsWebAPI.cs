using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

public sealed class HttpBattlegroundsWebAPI(
    ILogger<HttpBattlegroundsWebAPI> logger,
    IAsyncHttpClient asyncHttpClient,
    Configuration configuration,
    TimeSpan? authPollInterval = null,
    TimeSpan? authPollBudget = null) : IBattlegroundsWebAPI {

    private static readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static readonly string LatestNewsEndpoint = "/api/v1/news/";

    public static readonly string NewsPageEndpoint = "/api/v1/news/page";

    /// <summary>
    /// Deliberately <i>unversioned</i>: the API stores this exact path inside article markdown,
    /// so it must stay stable across versions.
    /// </summary>
    public static readonly string NewsResourceEndpoint = "/api/news/resources";

    private static readonly TimeSpan DefaultAuthPollBudget = TimeSpan.FromMinutes(4.5);

    private static readonly TimeSpan DefaultAuthPollInterval = TimeSpan.FromSeconds(1);

    private readonly ILogger<HttpBattlegroundsWebAPI> _logger = logger;
    private readonly IAsyncHttpClient _httpClient = asyncHttpClient;
    private readonly Configuration _configuration = configuration;
    private readonly TimeSpan _authPollInterval = authPollInterval ?? DefaultAuthPollInterval;
    private readonly TimeSpan _authPollBudget = authPollBudget ?? DefaultAuthPollBudget;

    private string _authToken = string.Empty;

    public string LoginEndpoint => $"{_configuration.API.LoginUrlOverride}{_configuration.API.LoginEndpoint}";

    public string RefreshEndpoint => $"{BaseUrl}/auth/v1/refresh";

    public string LogoutEndpoint => $"{BaseUrl}/auth/v1/logout";

    public string PublicKeyEndpoint => $"{_configuration.API.LoginUrlOverride}/publickey";

    private string BaseUrl => _configuration.API.BaseUrl.TrimEnd('/');

    public string AuthStartEndpoint(AuthProvider authProvider) => $"{BaseUrl}/auth/v1/{ProviderSegment(authProvider)}/start";

    public string AuthStatusEndpoint(AuthProvider authProvider) => $"{BaseUrl}/auth/v1/{ProviderSegment(authProvider)}/status";

    public string AuthCancelEndpoint => $"{BaseUrl}/auth/v1/session/cancel";

    private static string ProviderSegment(AuthProvider authProvider) => authProvider switch {
        AuthProvider.Battlegrounds => "battlegrounds",
        AuthProvider.Steam => "steam",
        AuthProvider.Discord => "discord",
        _ => throw new ArgumentOutOfRangeException(nameof(authProvider), $"Unsupported authentication provider: {authProvider}")
    };

    public async Task<LoginResponse> LoginAsync(LoginRequest request) {
        _logger.LogDebug("Logging in using {Endpoint}", LoginEndpoint);
        HttpRequestMessage requestMessage = new(HttpMethod.Post, LoginEndpoint) {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };
        HttpResponseMessage response = await _httpClient.SendRequestAsync(requestMessage);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Login failed with status code {StatusCode}. Error: {ErrorMessage}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new HttpRequestException($"Login failed with status code {response.StatusCode}.");
        }

        Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");
        return await FromJson<LoginResponse>(contentStream) ?? throw new InvalidOperationException("Failed to deserialize login response.");
    }

    public async Task<RefreshResult> RefreshTokenAsync(RefreshRequest request) {

        _logger.LogDebug("Refreshing token using {Endpoint}", RefreshEndpoint);
        HttpRequestMessage requestMessage = new(HttpMethod.Post, RefreshEndpoint) {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };
        HttpResponseMessage response = await _httpClient.SendRequestAsync(requestMessage);

        if (response.IsSuccessStatusCode) {
            try {
                Stream contentStream = await response.Content.ReadAsStreamAsync();
                RefreshResponse? refreshResponse = await FromJson<RefreshResponse>(contentStream);
                if (refreshResponse is null || string.IsNullOrWhiteSpace(refreshResponse.Token) || string.IsNullOrWhiteSpace(refreshResponse.RefreshToken)) {
                    _logger.LogError("Token refresh returned a success status but an unusable body.");
                    return new RefreshResult(RefreshOutcome.Transient, null);
                }
                return new RefreshResult(RefreshOutcome.Success, refreshResponse);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to deserialize the token refresh response.");
                return new RefreshResult(RefreshOutcome.Transient, null);
            }
        }

        string? errorCode = await ReadProblemTitleAsync(response);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.NotFound) {
            _logger.LogWarning("Token refresh was rejected with status {StatusCode} ({ErrorCode}).", response.StatusCode, errorCode ?? "no error code");
            return new RefreshResult(RefreshOutcome.Rejected, null, errorCode);
        }

        _logger.LogWarning("Token refresh failed with status {StatusCode} ({Reason}). The refresh token is retained.", response.StatusCode, response.ReasonPhrase);
        return new RefreshResult(RefreshOutcome.Transient, null, errorCode);

    }

    public async Task<bool> LogoutAsync() {

        _logger.LogDebug("Logging out using {Endpoint}", LogoutEndpoint);

        HttpRequestMessage request = new(HttpMethod.Post, LogoutEndpoint);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Logout failed with status code {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return false;
        }

        _logger.LogInformation("Session revoked successfully.");
        return true;

    }

    private static async Task<string?> ReadProblemTitleAsync(HttpResponseMessage response) {
        try {
            string body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) {
                return null;
            }
            using JsonDocument document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("title", out JsonElement title) ? title.GetString() : null;
        } catch (Exception) {
            return null;
        }
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

    public async Task<StartAuthResponse?> StartAuthAsync(AuthProvider provider, string? returnUrl = null) {

        string endpoint = string.IsNullOrWhiteSpace(returnUrl)
            ? AuthStartEndpoint(provider)
            : $"{AuthStartEndpoint(provider)}?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _logger.LogDebug("Starting authentication with {Provider} at {Endpoint}", provider, endpoint);

        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        try {
            HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
            if (!response.IsSuccessStatusCode) {
                _logger.LogError("Failed to start authentication with {Provider}. Status code: {StatusCode}. Error: {ErrorMessage}", provider, response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            return await response.Content.ReadFromJsonAsync<StartAuthResponse>(_jsonOptions)
                   ?? throw new InvalidOperationException("Failed to deserialize start auth response.");
        } catch (HttpRequestException ex) {
            _logger.LogError(ex, "Error starting authentication with {Provider}.", provider);
            return null;
        } catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error starting authentication with {Provider}.", provider);
            return null;
        }
    }

    public async Task<AuthStatusResult> EndAuthAsync(AuthProvider provider, string sessionId, string verifier, CancellationToken cancellationToken = default) {

        string endpoint = $"{AuthStatusEndpoint(provider)}?id={Uri.EscapeDataString(sessionId)}&verifier={Uri.EscapeDataString(verifier)}";
        DateTime deadline = DateTime.UtcNow + _authPollBudget;
        int attempt = 0;

        while (DateTime.UtcNow < deadline) {

            if (cancellationToken.IsCancellationRequested) {
                _logger.LogDebug("Waiting on the {Provider} login session was cancelled.", provider);
                return new AuthStatusResult(AuthStatusOutcome.Cancelled);
            }

            attempt++;

            try {

                HttpRequestMessage request = new(HttpMethod.Get, endpoint);
                HttpResponseMessage response = await _httpClient.SendRequestAsync(request);

                if (response.StatusCode is HttpStatusCode.OK) {
                    return await ReadAuthStatusAsync(provider, sessionId, response);
                }

                if (response.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.NotFound) {
                    // 404 is not "gone": the API answers it for a session it cannot see yet as well as one that has
                    // expired, deliberately indistinguishably. Both are waited out.
                    if (attempt % 5 == 1) {
                        _logger.LogDebug("The {Provider} login session {SessionId} has not resolved yet ({StatusCode}).", provider, sessionId, response.StatusCode);
                    }
                } else {
                    _logger.LogError("Checking the {Provider} login session failed with status {StatusCode}. Error: {ErrorMessage}", provider, response.StatusCode, await response.Content.ReadAsStringAsync());
                }

            } catch (OperationCanceledException) {
                _logger.LogDebug("Waiting on the {Provider} login session was cancelled.", provider);
                return new AuthStatusResult(AuthStatusOutcome.Cancelled);
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error checking the {Provider} login session {SessionId}.", provider, sessionId);
            }

            try {
                await Task.Delay(_authPollInterval, cancellationToken);
            } catch (OperationCanceledException) {
                _logger.LogDebug("Waiting on the {Provider} login session was cancelled.", provider);
                return new AuthStatusResult(AuthStatusOutcome.Cancelled);
            }

        }

        _logger.LogError("The {Provider} login session {SessionId} did not resolve within {Budget}.", provider, sessionId, _authPollBudget);
        return new AuthStatusResult(AuthStatusOutcome.TimedOut);

    }

    public async Task CancelAuthAsync(string sessionId, string verifier) {

        string endpoint = $"{AuthCancelEndpoint}?id={Uri.EscapeDataString(sessionId)}&verifier={Uri.EscapeDataString(verifier)}";

        try {
            HttpRequestMessage request = new(HttpMethod.Post, endpoint);
            HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
            if (response.IsSuccessStatusCode) {
                _logger.LogDebug("The login session {SessionId} was abandoned server-side.", sessionId);
            } else {
                _logger.LogDebug("Abandoning the login session {SessionId} answered {StatusCode}.", sessionId, response.StatusCode);
            }
        } catch (Exception ex) {
            _logger.LogDebug(ex, "The login session {SessionId} could not be abandoned server-side.", sessionId);
        }

    }
    
    private async Task<AuthStatusResult> ReadAuthStatusAsync(AuthProvider provider, string sessionId, HttpResponseMessage response) {

        string body = await response.Content.ReadAsStringAsync();

        try {

            using JsonDocument document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("status", out JsonElement status)) {

                string? code = document.RootElement.TryGetProperty("code", out JsonElement codeElement) ? codeElement.GetString() : null;
                string? description = document.RootElement.TryGetProperty("description", out JsonElement descriptionElement) ? descriptionElement.GetString() : null;

                if (string.Equals(status.GetString(), "MergeRequired", StringComparison.OrdinalIgnoreCase)) {
                    _logger.LogWarning("The {Provider} login session {SessionId} needs an account merge, which is a website flow.", provider, sessionId);
                    return new AuthStatusResult(AuthStatusOutcome.MergeRequired, null, code, description);
                }

                _logger.LogWarning("The {Provider} login session {SessionId} was refused ({Code}).", provider, sessionId, code ?? "no code");
                return new AuthStatusResult(AuthStatusOutcome.Failed, null, code, description);

            }

        } catch (JsonException ex) {
            _logger.LogError(ex, "The {Provider} login session {SessionId} returned a body that is not JSON.", provider, sessionId);
            return new AuthStatusResult(AuthStatusOutcome.Failed, null, null, "The sign-in response could not be read.");
        }

        try {
            EndAuthResponse? endAuthResponse = JsonSerializer.Deserialize<EndAuthResponse>(body, _jsonOptions);
            if (endAuthResponse is null || string.IsNullOrWhiteSpace(endAuthResponse.Token)) {
                _logger.LogError("The {Provider} login session {SessionId} completed but carried no token.", provider, sessionId);
                return new AuthStatusResult(AuthStatusOutcome.Failed, null, null, "The sign-in response could not be read.");
            }
            _logger.LogDebug("The {Provider} login session {SessionId} completed.", provider, sessionId);
            return new AuthStatusResult(AuthStatusOutcome.Success, endAuthResponse);
        } catch (JsonException ex) {
            _logger.LogError(ex, "Failed to deserialize the {Provider} login session {SessionId}.", provider, sessionId);
            return new AuthStatusResult(AuthStatusOutcome.Failed, null, null, "The sign-in response could not be read.");
        }

    }

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

    public async Task<User?> GetUserAsync(string userId) {

        string endpoint = $"{BaseUrl}/api/v1/users/{Uri.EscapeDataString(userId)}";
        _logger.LogDebug("Retrieving user from {Endpoint}", endpoint);

        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Failed to retrieve user {UserId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", userId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try {
            var responseMapped = await response.Content.ReadFromJsonAsync<GetUserResponse>(_jsonOptions);
            if (responseMapped is null) {
                _logger.LogError("Failed to deserialize the user {UserId} response.", userId);
                return null;
            }
            return new User() { UserId = responseMapped.Id, UserDisplayName = responseMapped.UserDisplayName };
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to deserialize the user {UserId} response.", userId);
            return null;
        }

    }

    private sealed record GetUserResponse(
        [property: JsonPropertyName("bgId")] string Id,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("displayName")] string UserDisplayName
    );

}
