using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Security;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services;

public sealed class UserService(ILogger<UserService> logger, IBattlegroundsWebAPI webAPI) : IUserService {
        
    private sealed record JWTHeader(
        [property: JsonPropertyName("alg")] string Algorithm,
        [property: JsonPropertyName("typ")] string Type = "JWT"
    );
    private sealed record StoredTokenData(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("issued_at")] DateTime IssuedAt,
        [property: JsonPropertyName("expiration")] DateTime Expiration
    );

    private static readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private static readonly string _userTokenStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds", "local_user.dat");

    private readonly ILogger<UserService> _logger = logger;
    private readonly IBattlegroundsWebAPI _webAPI = webAPI;

    private User? _localUser;
    private string _token = string.Empty;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private string _refreshToken = string.Empty;
    private RSA? _publicKey = null;

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

    public async Task<User?> LoginAsync(string userName, string password) {

        if (_localUser is not null) {
            return _localUser; // Already logged in
        }

        if (string.IsNullOrWhiteSpace(userName)) {
            throw new ArgumentException("Username cannot be null or empty.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(password)) {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        LoginResponse loginResponse = await _webAPI.LoginAsync(new LoginRequest(userName, password)) ?? throw new InvalidOperationException("Login response is null.");
        StoreToken(loginResponse.Token, loginResponse.RefreshToken);
        _localUser = await GetUserFromToken(loginResponse.Token);
        return _localUser;

    }

    private async Task<User> GetUserFromToken(string token) {
        _logger.LogDebug("Extracting user from JWT token.");

        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        string[] parts = token.Split('.');
        if (parts.Length != 3) {
            throw new ArgumentException("Invalid JWT token format.", nameof(token));
        }
        string headerEncoded = parts[0];
        byte[] headerBytes = Base64URLDecode(headerEncoded);
        JWTHeader? header = JsonSerializer.Deserialize<JWTHeader>(headerBytes, _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize JWT header.");
        if (header.Algorithm != "RS256") {
            throw new NotSupportedException($"Unsupported JWT algorithm: {header.Algorithm}");
        }

        string payloadEncoded = parts[1];
        byte[] payloadBytes = Base64URLDecode(payloadEncoded);
        UserClaim? userClaim = JsonSerializer.Deserialize<UserClaim>(payloadBytes, _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize JWT payload.");
        if (string.IsNullOrWhiteSpace(userClaim.Subject) || string.IsNullOrWhiteSpace(userClaim.UserName)) {
            throw new InvalidOperationException("Invalid user claim in JWT payload.");
        }

        if (!await ValidateSignature(headerEncoded, payloadEncoded, parts[2])) {
            throw new InvalidOperationException("JWT token signature validation failed.");
        }

        _logger.LogDebug("JWT token signature validated successfully.");

        var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (userClaim.ExpiresAt <= utcNow) {
            _logger.LogDebug("JWT token has expired. Expires at: {ExpiresAt}, Current time: {CurrentTime}", userClaim.ExpiresAt, utcNow);
            throw new InvalidOperationException("JWT token has expired.");
        }

        if (userClaim.IssuedAt >= utcNow) {
            _logger.LogDebug("JWT token is not yet valid. Issued at: {IssuedAt}, Current time: {CurrentTime}", userClaim.IssuedAt, utcNow);
            throw new InvalidOperationException("JWT token is not yet valid.");
        }

        _logger.LogDebug("JWT token is valid and not expired.");

        return new User {
            UserId = userClaim.Subject,
            UserDisplayName = userClaim.UserName
        };
    }

    private async Task<bool> ValidateSignature(string headerEncoded, string payloadEncoded, string signatureEncoded) {

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
    }

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
            StoreToken(refreshResponse.Token, refreshResponse.RefreshToken);
            return _token;
        }
        return _token; // Return the existing token if it's still valid
    }

    private void StoreToken(string token, string refreshToken) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        _token = token;
        _refreshToken = refreshToken;
        _tokenExpiration = DateTime.UtcNow.AddMinutes(30); // Assuming token is valid for 30 minutes, adjust as necessary (TODO: Extract from JWT)
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
        _localUser = await GetUserFromToken(_token);

        return true; // Successfully auto-logged in with the stored token

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
