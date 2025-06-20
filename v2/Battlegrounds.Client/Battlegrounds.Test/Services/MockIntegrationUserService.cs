using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Test.Services;

public sealed class MockIntegrationUserService(string user) : IUserService {

    private static readonly Dictionary<string, string> _userCredentials = new() { // Dummy users for testing
        { "admin", "admin123" },
        { "user", "password123" }
    };

    private static readonly Configuration Configuration = new() {
        API = new Configuration.APIConfiguration {
            LoginUrlOverride = "http://bg.test.service.cohbattlegrounds.com:8087"
        }
    };

    private readonly TestLogger<MockIntegrationUserService> _logger = new();

    private static readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly AsyncHttpClient _httpClient = new(new HttpClient(), Configuration, new TestLogger<AsyncHttpClient>());

    private string _token = string.Empty;

    public static string LoginEndpoint => $"{Configuration.API.LoginUrlOverride}{Configuration.API.LoginEndpoint}";

    public IAsyncHttpClient HttpClient => _httpClient;

    public ValueTask<bool> AutoLoginAsync() {
        throw new NotImplementedException();
    }

    public Task<User?> GetLocalUserAsync() => Task.FromResult<User?>(new User {
        UserId = user,
        UserDisplayName = user,
    });

    public string GetLocalUserRefreshToken() {
        throw new NotImplementedException();
    }

    public string GetLocalUserToken() => _token;

    public async Task<string> GetLocalUserTokenAsync() {
        if (!string.IsNullOrEmpty(_token)) {
            return _token;
        }
        await LoginAsync("user", "admin123");
        if (string.IsNullOrEmpty(_token)) {
            throw new InvalidOperationException("User token is not set after login.");
        }
        return _token;
    }

    public Task<User> GetUserAsync(string userId) {
        throw new NotImplementedException();
    }

    public async Task<User?> LoginAsync(string userName, string password) {
        _logger.LogDebug("Logging in using {Endpoint}", LoginEndpoint);
        HttpRequestMessage requestMessage = new(HttpMethod.Post, LoginEndpoint) {
            Content = JsonContent.Create(new LoginRequest(user, _userCredentials[user]), options: _jsonOptions)
        };
        HttpResponseMessage response = await _httpClient.SendRequestAsync(requestMessage);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Login failed with status code {StatusCode} and error description: {ErrorDesc}", response.StatusCode, await response.Content.ReadAsStringAsync());
            throw new HttpRequestException($"Login failed with status code {response.StatusCode}.");
        }

        Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");

        var loginResponse = await JsonSerializer.DeserializeAsync<LoginResponse>(contentStream, _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize login response.");
        _token = loginResponse.Token;
        return loginResponse.User;
    }

    public Task<bool> LogOutAsync() {
        throw new NotImplementedException();
    }
}
