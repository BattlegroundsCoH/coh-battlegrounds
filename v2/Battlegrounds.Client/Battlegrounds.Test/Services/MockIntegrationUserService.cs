using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Services;

namespace Battlegrounds.Test.Services;

public sealed class MockIntegrationUserService(string user) : IUserService {

    private static readonly Dictionary<string, string> _userCredentials = new() { // Dummy users for testing
        { "admin", "admin123" },
        { "user", "password123" }
    };

    private static readonly Configuration Configuration = new() {
        API = new Configuration.APIConfiguration {
            LoginUrlOverride = "https:// bg.test.service.cohbg.com:8087",
            LoginEndpoint = "/api/v1/login",
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

    public Task<bool> IsUserLoggedIn => Task.FromResult(true);

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
        return new User {
            UserId = userName,
            UserDisplayName = userName,
        };
    }

    public Task<bool> LogOutAsync() {
        throw new NotImplementedException();
    }

    public Task<User> LoginWithDiscordAsync() {
        throw new NotImplementedException();
    }

    public Task<User> LoginWithSteamAsync() {
        throw new NotImplementedException();
    }

}
