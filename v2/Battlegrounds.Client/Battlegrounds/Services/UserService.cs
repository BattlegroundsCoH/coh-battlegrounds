using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Models;

namespace Battlegrounds.Services;

public sealed class UserService(HttpClient client, Configuration configuration) : IUserService {

    private sealed record LoginRequest(
        [property: JsonPropertyName("username")] string UserName,
        [property: JsonPropertyName("password")] string Password
    );
    private sealed record LoginResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("user")] User User
    );
    private sealed record RefreshRequest(
        [property: JsonPropertyName("refresh_token")] string RefreshToken
    );
    private sealed record RefreshResponse(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("refresh_token")] string RefreshToken
    );
    private sealed record JWTHeader(
        [property: JsonPropertyName("alg")] string Algorithm,
        [property: JsonPropertyName("typ")] string Type = "JWT"
    );

    private static readonly JsonSerializerOptions _jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient = client ?? throw new ArgumentNullException(nameof(client));

    private User? _localUser =
        #if DEBUG
        new User() { UserId = "localUserId", UserDisplayName = "Local User" };
#else
        null; // In production, local user is null until logged in
#endif
    private string _token = string.Empty;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private string _refreshToken = string.Empty;

    public string LoginEndpoint => $"{configuration.API.LoginUrlOverride}{configuration.API.LoginEndpoint}";

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

        HttpResponseMessage response = await _httpClient.PostAsync(LoginEndpoint, ToJson(new LoginRequest(userName, password)));
        if (!response.IsSuccessStatusCode) {
            //string errorContent = await response.Content.ReadAsStringAsync() ?? string.Empty; // TODO: Log this...
            throw new HttpRequestException($"Login failed with status code {response.StatusCode}.");
        }

        Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");
        LoginResponse? loginResponse = await FromJson<LoginResponse>(contentStream) ?? throw new InvalidOperationException("Failed to deserialize login response.");
        StoreToken(loginResponse.Token, loginResponse.RefreshToken);
        _localUser = GetUserFromToken(loginResponse.Token);
        return _localUser;

    }

    private User? GetUserFromToken(string token) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        string[] parts = token.Split('.');
        if (parts.Length != 3) {
            throw new ArgumentException("Invalid JWT token format.", nameof(token));
        }
        byte[] headerBytes = Convert.FromBase64String(parts[0]);
        JWTHeader? header = JsonSerializer.Deserialize<JWTHeader>(headerBytes, _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize JWT header.");
        if (header.Algorithm != "RS256") {
            throw new NotSupportedException($"Unsupported JWT algorithm: {header.Algorithm}");
        }
        byte[] payloadBytes = Convert.FromBase64String(parts[1]);
        UserClaim? userClaim = JsonSerializer.Deserialize<UserClaim>(payloadBytes, _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize JWT payload.");
        if (string.IsNullOrWhiteSpace(userClaim.Subject) || string.IsNullOrWhiteSpace(userClaim.UserName)) {
            throw new InvalidOperationException("Invalid user claim in JWT payload.");
        }
        // TODO: Validate signature using public key
        return new User {
            UserId = userClaim.Subject,
            UserDisplayName = userClaim.UserName
        };
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
            HttpResponseMessage response = await _httpClient.PostAsync($"{LoginEndpoint}/refresh", ToJson(new RefreshRequest(_refreshToken)));
            if (!response.IsSuccessStatusCode) {
                throw new HttpRequestException($"Token refresh failed with status code {response.StatusCode}.");
            }
            Stream contentStream = await response.Content.ReadAsStreamAsync() ?? throw new InvalidOperationException("Response content is null.");
            RefreshResponse? refreshResponse = await FromJson<RefreshResponse>(contentStream) ?? throw new InvalidOperationException("Failed to deserialize refresh response.");
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
        // TODO: Store tokens securely, e.g., in a secure storage or encrypted file
    }

    private static StringContent ToJson<T>(T value) {
        string json = JsonSerializer.Serialize(value, _jsonOptions);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    private static ValueTask<T?> FromJson<T>(Stream source) {
        if (source is null) {
            throw new ArgumentNullException(nameof(source), "Source stream cannot be null.");
        }
        return JsonSerializer.DeserializeAsync<T>(source, _jsonOptions);
    }

    public Task<string> GetLocalUserTokenAsync() => GetToken();

}
