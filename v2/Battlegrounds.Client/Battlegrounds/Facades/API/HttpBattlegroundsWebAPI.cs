using System.IO;
using System.Net;
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

    public string AuthStartEndpoint(AuthProvider authProvider) => $"{_configuration.API.BaseUrl}{_configuration.API.AuthStartEndpoint.Replace("<IdP>", authProvider switch {
        AuthProvider.Battlegrounds => "battlegrounds",
        AuthProvider.Steam => "steam",
        AuthProvider.Discord => "discord",
        _ => throw new ArgumentOutOfRangeException(nameof(authProvider), $"Unsupported authentication provider: {authProvider}")
    })}";

    public string AuthStatusEndpoint(AuthProvider authProvider) => $"{_configuration.API.BaseUrl}{_configuration.API.AuthStatusEndpoint.Replace("<IdP>", authProvider switch {
        AuthProvider.Battlegrounds => "battlegrounds",
        AuthProvider.Steam => "steam",
        AuthProvider.Discord => "discord",
        _ => throw new ArgumentOutOfRangeException(nameof(authProvider), $"Unsupported authentication provider: {authProvider}")
    })}";

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

    public async Task<StartAuthResponse?> StartAuthAsync(AuthProvider provider) {

        string endpoint = AuthStartEndpoint(provider);
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

    const int MaxRetries = 240; // Maximum number of retries for checking auth status
    const int RetryDelayMilliseconds = 250; // Delay between retries in milliseconds
    // 240 retries with 250ms delay = 60 seconds total wait time (Not accounting for network latency)

    public async Task<EndAuthResponse?> EndAuthAsync(AuthProvider provider, string sessionId) {

        string endpoint = $"{AuthStatusEndpoint(provider)}?id={sessionId}";
        for (int i = 0; i < MaxRetries; i++) {
            try {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
                if (response.StatusCode is HttpStatusCode.OK) {
                    _logger.LogDebug("Authentication status for {Provider} with session {SessionId} is OK.", provider, sessionId);
                    Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");
                    return await FromJson<EndAuthResponse>(contentStream) ?? throw new InvalidOperationException("Failed to deserialize end auth response.");
                } else if (response.StatusCode is HttpStatusCode.NotFound) {
                    _logger.LogDebug("Authentication status for {Provider} with session {SessionId} not found. Retrying...", provider, sessionId);
                    await Task.Delay(RetryDelayMilliseconds); // Wait before retrying
                } else {
                    _logger.LogError("Authentication status check failed with status code {StatusCode}. Error: {ErrorMessage}", response.StatusCode, await response.Content.ReadAsStringAsync());
                    await Task.Delay(RetryDelayMilliseconds); // Wait before retrying
                }
            } catch (HttpRequestException ex) {
                _logger.LogError(ex, "Error checking authentication status for {Provider} with session {SessionId}. Retrying...", provider, sessionId);
                await Task.Delay(RetryDelayMilliseconds); // Wait before retrying
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error checking authentication status for {Provider} with session {SessionId}. Retrying...", provider, sessionId);
                await Task.Delay(RetryDelayMilliseconds); // Wait before retrying
            }
        }

        _logger.LogError("Authentication status check for {Provider} with session {SessionId} timed out after 1000 attempts.", provider, sessionId);
        return null;

    }

}
