using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Infrastructure;

/// <summary>
/// Provides user authentication and management services, including login, token handling, and user retrieval.
/// </summary>
/// <remarks>This service handles user authentication through various providers, token management (including
/// refreshing expired tokens), and retrieval of user information. It integrates with the <see
/// cref="IBattlegroundsWebAPI"/> for API communication and <see cref="IBrowserService"/> for browser-based
/// authentication flows.  The service maintains a local cache of the authenticated user and their tokens, and supports
/// automatic login using locally stored encrypted token data. It also provides methods to check token expiration and
/// refresh tokens as needed.</remarks>
/// <param name="logger"></param>
/// <param name="webAPI"></param>
/// <param name="browserService"></param>
public sealed class UserService(ILogger<UserService> logger, IBattlegroundsWebAPI webAPI, IBrowserService browserService) : IUserService {
        
    private sealed record JWTHeader(
        [property: JsonPropertyName("alg")] string Algorithm,
        [property: JsonPropertyName("typ")] string Type = "JWT",
        [property: JsonPropertyName("kid")] string? KeyId = null
    );
    private sealed record StoredTokenData(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("issued_at")] DateTime IssuedAt,
        [property: JsonPropertyName("expiration")] DateTime Expiration,
        [property: JsonPropertyName("accessed_at")] DateTime AccessedAt,
        [property: JsonPropertyName("user")] User? User = null
    );

    private static readonly string _userTokenStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds", "local_user.dat");

    private readonly ILogger<UserService> _logger = logger;
    private readonly IBattlegroundsWebAPI _webAPI = webAPI;
    private readonly IBrowserService _browserService = browserService;

    private TaskCompletionSource<bool> _hasLoggedInUser = new TaskCompletionSource<bool>();

    private User? _localUser;
    private string _token = string.Empty;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private string _refreshToken = string.Empty;
    //private RSA? _publicKey = null;

    public bool IsExpired => DateTime.UtcNow >= _tokenExpiration; // Check if the token is expired

    public Task<bool> IsUserLoggedIn => _hasLoggedInUser.Task;

    public async Task<User?> GetLocalUserAsync() {
        if (_localUser is not null) {
            return _localUser; // Return the cached local user
        }
        if (string.IsNullOrEmpty(_refreshToken)) {
            return null; // If the refresh token is empty or null, user is not logged in
        }
        if (string.IsNullOrEmpty(await GetToken())) {
            return null; // If the token is empty or null, user is not logged in
        }
        throw new InvalidOperationException("User is not logged in or token is invalid.");
    }

    public string GetLocalUserRefreshToken() => _refreshToken; // Returns the current refresh token, which may be expired

    public string GetLocalUserToken() => _token; // Returns the current token, which may be expired

    public Task<User> GetUserAsync(string userId) => GetLocalUserAsync()!; // TODO: Implement this method to fetch user data from Battlegrounds API

    public async Task<User?> LoginAsync(string userEmail, string password) {

        if (_localUser is not null && !IsExpired) {
            return _localUser; // Already logged in
        }

        if (string.IsNullOrWhiteSpace(userEmail)) {
            throw new ArgumentException("Username cannot be null or empty.", nameof(userEmail));
        }

        if (string.IsNullOrWhiteSpace(password)) {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        _logger.LogInformation("Logging in user {UserName}...", userEmail);

        LoginResponse loginResponse = await _webAPI.LoginAsync(new LoginRequest(userEmail, password)) ?? throw new InvalidOperationException("Login response is null.");
        StoreTokenAndUser(loginResponse.Token, loginResponse.RefreshToken, new DateTime(loginResponse.ExpiresAt, DateTimeKind.Utc), new User {
            UserId = loginResponse.User.Id,
            UserDisplayName = loginResponse.User.Username,
        });
        _logger.LogInformation("User {UserName} with Id {Id} logged in successfully.", userEmail, _localUser.UserId);
        return _localUser;

    }

    public Task<bool> LogOutAsync() {
        _token = string.Empty;
        _tokenExpiration = DateTime.MinValue;
        _refreshToken = string.Empty;
        _localUser = null;
        _hasLoggedInUser = new TaskCompletionSource<bool>();
        return Task.FromResult(true);
    }

    private async Task<string> GetToken() {
        if (DateTime.UtcNow >= _tokenExpiration) {
            // Token expired, refresh it
            if (string.IsNullOrWhiteSpace(_refreshToken)) {
                _logger.LogWarning("Refresh token is not available. User needs to log in again.");
                return string.Empty; // If refresh fails, return empty token
            }
            if (await RefreshToken(_refreshToken, _localUser) is false) {
                _logger.LogWarning("Failed to refresh token for user {UserName}. User needs to log in again.", _localUser?.UserDisplayName);
                return string.Empty; // If refresh fails, return empty token
            }
            return _token;
        }
        return _token; // Return the existing token if it's still valid
    }

    [MemberNotNull(nameof(_localUser), nameof(_token), nameof(_refreshToken), nameof(_tokenExpiration))]
    private void StoreTokenAndUser(string token, string refreshToken, DateTime tokenExpiration, User user) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        _localUser = user;
        _token = token;
        _refreshToken = refreshToken;
        _tokenExpiration = tokenExpiration;
        _hasLoggedInUser.SetResult(true);
        StoreTokenInEncryptedFile(_token, _refreshToken, _tokenExpiration, DateTime.UtcNow, user);
        _webAPI.SetAuthenticationToken(_token); // Set the authentication token for the web API
    }

    public Task<string> GetLocalUserTokenAsync() => GetToken();

    public async ValueTask<bool> AutoLoginAsync() {

        if (_localUser is not null) {
            return true; // Already logged in
        }

        if (!File.Exists(_userTokenStore)) {
            return false; // No local user token file found
        }

        StoredTokenData? tokenData = GetTokenFromEncryptedFile();
        if (tokenData is null) {
            return false; // Token is empty or null
        }

        if (DateTime.UtcNow >= tokenData.Expiration) {
            return await RefreshToken(tokenData.RefreshToken, tokenData.User); // Token is expired, attempt to refresh it
        }

        _token = tokenData.Token;
        _refreshToken = tokenData.RefreshToken;
        _tokenExpiration = tokenData.Expiration;
        _localUser = tokenData.User;
        _hasLoggedInUser.SetResult(true);

        return tokenData.User is not null && _tokenExpiration > DateTime.UtcNow;

    }

    private async ValueTask<bool> RefreshToken(string refreshToken, User? user) {
        try {
            var response = await _webAPI.RefreshTokenAsync(new RefreshRequest(refreshToken));
            if (response is null) {
                return false; // Failed to refresh token
            }
            if (user is null) {
                throw new NotImplementedException("User not found, should ask API for user data based on token");
            }
            StoreTokenAndUser(response.Token, response.RefreshToken, DateTime.UtcNow.AddSeconds(3600), user);
            return true; // Successfully refreshed token
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while refreshing token.");
            return false; // Failed to refresh token
        }
    }

    public Task<User> LoginWithDiscordAsync() => LoginWithProviderAsync(AuthProvider.Discord);

    private async Task<User> LoginWithProviderAsync(AuthProvider provider) {

        StartAuthResponse? startAuthResponse = await _webAPI.StartAuthAsync(provider) ?? throw new InvalidOperationException("Failed to start authentication with provider.");
        if (startAuthResponse is null) {
            _logger.LogWarning("Authentication session could not be started for provider {Provider}.", provider);
            throw new InvalidOperationException("Authentication session could not be started.");
        }

        _logger.LogInformation("Starting authentication with {Provider}", provider);

        // Open url in default browser
        _browserService.OpenUrl(startAuthResponse.AuthUrl);

        _logger.LogInformation("Waiting for authentication to complete...");

        EndAuthResponse? endAuthResponse = await _webAPI.EndAuthAsync(provider, startAuthResponse.SessionId);
        if (endAuthResponse is null) {
            _logger.LogWarning("Authentication session ended without response for provider {Provider}.", provider);
            throw new InvalidOperationException("Authentication session ended without response.");
        }

        _logger.LogInformation("Authentication with {Provider} completed successfully.", provider);

        StoreTokenAndUser(endAuthResponse.Token, endAuthResponse.RefreshToken, endAuthResponse.ExpiresAt, new User {
            UserId = endAuthResponse.User.Id,
            UserDisplayName = endAuthResponse.User.DisplayName,
        });

        _logger.LogInformation("User {UserName} with Id {Id} logged in successfully via {Provider}.", _localUser.Email, _localUser.UserId, provider);

        return _localUser;

    }

    private static StoredTokenData? GetTokenFromEncryptedFile() {
        if (!File.Exists(_userTokenStore)) {
            throw new FileNotFoundException("Local user token file not found.", _userTokenStore);
        }
        byte[] cipherText = File.ReadAllBytes(_userTokenStore);
        byte[] plainText = ProtectedData.Unprotect(cipherText, null, DataProtectionScope.CurrentUser);
        return JsonSerializer.Deserialize<StoredTokenData>(Encoding.UTF8.GetString(plainText));
    }

    private static async void StoreTokenInEncryptedFile(string token, string refreshToken, DateTime expiration, DateTime issuedAt, User? user) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        var tokenData = new StoredTokenData(token, refreshToken, issuedAt, expiration, DateTime.UtcNow, user);
        byte[] plainText = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tokenData));
        byte[] cipherText = ProtectedData.Protect(plainText, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_userTokenStore, cipherText);
    }

}
