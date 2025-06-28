using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services;

public sealed class UserService(ILogger<UserService> logger, IBattlegroundsWebAPI webAPI, Configuration configuration) : IUserService {
        
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
        [property: JsonPropertyName("user")] User? User = null
    );

    private static readonly string _userTokenStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds", "local_user.dat");

    private readonly ILogger<UserService> _logger = logger;
    private readonly IBattlegroundsWebAPI _webAPI = webAPI;
    private readonly Configuration _configuration = configuration;

    private User? _localUser;
    private string _token = string.Empty;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private string _refreshToken = string.Empty;
    private RSA? _publicKey = null;

    public bool IsExpired => DateTime.UtcNow >= _tokenExpiration; // Check if the token is expired

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
        StoreToken(loginResponse.Token, loginResponse.RefreshToken, new DateTime(loginResponse.ExpiresAt, DateTimeKind.Utc));
        _localUser = new User {
            Email = loginResponse.User.Email,
            UserId = loginResponse.User.Id,
        };
        _logger.LogInformation("User {UserName} with Id {Id} logged in successfully.", userEmail, _localUser.UserId);
        return _localUser;

    }

    /*private async Task<bool> ValidateRS256Signature(string headerEncoded, string payloadEncoded, string signatureEncoded) {

        // Ensure public key is available for signature validation
        if (_publicKey is null) {
            _logger.LogDebug("Retrieving public key for signature validation.");
            string publicKeyPem = await _webAPI.GetPublicKeyAsync() ?? throw new InvalidOperationException("Failed to retrieve public key.");
            _publicKey = RSAPublicKey.FromPem(publicKeyPem);
        }

        byte[] signature = Base64URLDecode(signatureEncoded);
        byte[] signedData = Encoding.UTF8.GetBytes($"{headerEncoded}.{payloadEncoded}");

        // Verify RS256 Signature (RSA + SHA256)
        var isValidSignature = _publicKey.VerifyData(
            signedData,
            signature,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        return isValidSignature;

    }

    private static byte[] Base64URLDecode(string input) {
        // Replace URL-safe characters and pad with '='
        string base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4) {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }*/

    public Task<bool> LogOutAsync() {
        throw new NotImplementedException();
    }

    private async Task<string> GetToken() {
        if (DateTime.UtcNow >= _tokenExpiration) {
            // Token expired, refresh it
            if (string.IsNullOrWhiteSpace(_refreshToken)) {
                throw new InvalidOperationException("Refresh token is not available. Please log in again.");
            }
            RefreshResponse? refreshResponse = await _webAPI.RefreshTokenAsync(new RefreshRequest(_refreshToken));
            StoreToken(refreshResponse.Token, refreshResponse.RefreshToken, DateTime.UtcNow.AddSeconds(3600));
            return _token;
        }
        return _token; // Return the existing token if it's still valid
    }

    private void StoreToken(string token, string refreshToken, DateTime tokenExpiration) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        _token = token;
        _refreshToken = refreshToken;
        _tokenExpiration = tokenExpiration;
        StoreTokenInEncryptedFile(_token, _refreshToken, _tokenExpiration, DateTime.UtcNow);
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
            return false; // Token is expired (TODO: Implement refresh logic if needed)
        }

        _token = tokenData.Token;
        _refreshToken = tokenData.RefreshToken;
        _tokenExpiration = tokenData.Expiration;
        _localUser = tokenData.User;

        return tokenData.User is not null && _tokenExpiration > DateTime.UtcNow;

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
        Process.Start(new ProcessStartInfo {
            FileName = startAuthResponse.AuthUrl,
            UseShellExecute = true
        });

        _logger.LogInformation("Waiting for authentication to complete...");

        EndAuthResponse? endAuthResponse = await _webAPI.EndAuthAsync(provider, startAuthResponse.SessionId);
        if (endAuthResponse is null) {
            _logger.LogWarning("Authentication session ended without response for provider {Provider}.", provider);
            throw new InvalidOperationException("Authentication session ended without response.");
        }

        _logger.LogInformation("Authentication with {Provider} completed successfully.", provider);
        _localUser = new User {
            Email = endAuthResponse.User.Email,
            UserId = endAuthResponse.User.Id,
        };

        StoreToken(endAuthResponse.Token, endAuthResponse.RefreshToken, new DateTime(endAuthResponse.ExpiresAt, DateTimeKind.Utc));

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

    private static async void StoreTokenInEncryptedFile(string token, string refreshToken, DateTime expiration, DateTime issuedAt) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        var tokenData = new StoredTokenData(token, refreshToken, issuedAt, expiration);
        byte[] plainText = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tokenData));
        byte[] cipherText = ProtectedData.Protect(plainText, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(_userTokenStore, cipherText);
    }

}
