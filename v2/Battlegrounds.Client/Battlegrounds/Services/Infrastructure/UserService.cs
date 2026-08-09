using System.Buffers.Text;
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
/// refresh tokens as needed.
/// <para>
/// The API <i>rotates</i> refresh tokens: exchanging one marks it used and issues a replacement, and presenting an
/// already-used token is treated as a breach that revokes every session the user has. Two rules follow, and both are
/// enforced here: a refresh must never run concurrently with itself (see <see cref="_refreshLock"/>), and a rotated
/// token must reach disk before it is used (see <see cref="PersistTokensAsync"/>).
/// </para></remarks>
/// <param name="logger"></param>
/// <param name="webAPI"></param>
/// <param name="browserService"></param>
/// <param name="tokenStorePath">Where the encrypted token file lives. Defaults to the per-user application data
/// location; supplied by tests so they do not write to the real user's store.</param>
public sealed class UserService(ILogger<UserService> logger, IBattlegroundsWebAPI webAPI, IBrowserService browserService, string? tokenStorePath = null) : IUserService, IDisposable {

    private sealed record StoredTokenData(
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("issued_at")] DateTime IssuedAt,
        [property: JsonPropertyName("expiration")] DateTime Expiration,
        [property: JsonPropertyName("accessed_at")] DateTime AccessedAt,
        [property: JsonPropertyName("user")] User? User = null
    );

    /// <summary>
    /// How long before the access token actually expires it is treated as expired.
    /// </summary>
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(2);

    /// <summary>
    /// How long before expiry the background refresh runs.
    /// </summary>
    private static readonly TimeSpan ProactiveLead = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How long to wait before retrying a background refresh that failed for a transient reason.
    /// </summary>
    private static readonly TimeSpan ProactiveRetryInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Floor on the background refresh delay, so a short-lived token cannot spin the timer.
    /// </summary>
    private static readonly TimeSpan MinimumProactiveDelay = TimeSpan.FromSeconds(30);

    private static readonly string DefaultTokenStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoHBattlegrounds", "local_user.dat");

    private readonly ILogger<UserService> _logger = logger;
    private readonly IBattlegroundsWebAPI _webAPI = webAPI;
    private readonly IBrowserService _browserService = browserService;
    private readonly string _userTokenStore = tokenStorePath ?? DefaultTokenStorePath;

    /// <summary>
    /// Serialises refresh attempts so a rotated token is never spent twice.
    /// </summary>
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// Fires <see cref="ProactiveLead"/> before the access token expires. Created lazily -- a field initializer cannot
    /// reference the instance it needs for the callback.
    /// </summary>
    private Timer? _refreshTimer;

    /// <summary>
    /// The most recent sign-out, so shutdown can wait for its server-side revocation. Never faults.
    /// </summary>
    private Task _pendingLogOut = Task.CompletedTask;

    private TaskCompletionSource<bool> _hasLoggedInUser = new TaskCompletionSource<bool>();

    private User? _localUser;
    private string _token = string.Empty;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private string _refreshToken = string.Empty;

    /// <summary>
    /// Whether the access token has expired, or is close enough to expiry that it should be refreshed before use.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= _tokenExpiration - RefreshSkew;

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
        return _localUser; // A refresh may have recovered the user from the API response; null if it did not
    }

    public string GetLocalUserRefreshToken() => _refreshToken; // Returns the current refresh token, which may be expired

    public async Task<User> GetUserAsync(string userId) {
        if (string.IsNullOrWhiteSpace(userId)) {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }
        if (_localUser is not null && _localUser.UserId == userId) {
            return await GetLocalUserAsync() ?? throw new InvalidOperationException("Local user is not available."); // Return the cached local user if it matches the requested userId
        }
        return (await _webAPI.GetUserAsync(userId)) ?? throw new InvalidOperationException($"User with ID {userId} not found."); // Fetch the user from the web API if not cached
    }

    public async Task<User?> LoginAsync(string userName, string password) {

        if (_localUser is not null && !IsExpired) {
            return _localUser; // Already logged in
        }

        if (string.IsNullOrWhiteSpace(userName)) {
            throw new ArgumentException("Username cannot be null or empty.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(password)) {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        _logger.LogInformation("Logging in user {UserName}...", userName);

        LoginResponse loginResponse = await _webAPI.LoginAsync(new LoginRequest(userName, password)) ?? throw new InvalidOperationException("Login response is null.");
        ApplyTokens(loginResponse.Token, loginResponse.RefreshToken, GetTokenExpiration(loginResponse.Token, loginResponse.ExpiresAt), new User {
            UserId = loginResponse.User.Id,
            UserDisplayName = loginResponse.User.Username,
        });
        await PersistTokensAsync();

        _logger.LogInformation("User {UserName} with Id {Id} logged in successfully.", userName, _localUser.UserId);
        return _localUser;

    }

    public Task<bool> LogOutAsync() {
        // Run on the thread pool rather than inheriting the dispatcher: shutdown blocks on this, and continuations
        // queued back onto a blocked UI thread would never get to run.
        Task<bool> logOut = Task.Run(LogOutCoreAsync);
        _pendingLogOut = logOut;
        return logOut;
    }

    public async Task WaitForPendingLogOutAsync(TimeSpan timeout) {

        Task pending = _pendingLogOut;
        if (pending.IsCompleted) {
            return;
        }

        _logger.LogInformation("Waiting up to {Timeout} for the session revocation to finish before exiting.", timeout);
        try {
            await pending.WaitAsync(timeout).ConfigureAwait(false);
        } catch (TimeoutException) {
            _logger.LogWarning("The session revocation did not finish within {Timeout}; exiting anyway.", timeout);
        } catch (Exception ex) {
            _logger.LogError(ex, "The pending session revocation failed.");
        }

    }

    private async Task<bool> LogOutCoreAsync() {

        bool revoked = false;
        try {
            string token = await GetToken();
            if (string.IsNullOrEmpty(token)) {
                _logger.LogWarning("No usable access token at logout; the session cannot be revoked server-side.");
            } else {
                revoked = await _webAPI.LogoutAsync();
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to revoke the session server-side during logout.");
        }

        ClearLocalCredentials();
        return revoked;

    }

    /// <summary>
    /// Gets the access token, refreshing it first if it expires within <paramref name="margin"/>.
    /// </summary>
    private async Task<string> GetToken(TimeSpan? margin = null) {

        TimeSpan required = margin ?? RefreshSkew;

        if (DateTime.UtcNow < _tokenExpiration - required) {
            return _token; // Return the existing token if it's still valid
        }

        await _refreshLock.WaitAsync();
        try {

            // Re-check under the lock: a caller that queued behind an in-flight refresh would otherwise present the
            // token that refresh just spent, which the API classifies as a breach and answers by revoking every session.
            if (DateTime.UtcNow < _tokenExpiration - required) {
                return _token;
            }

            if (string.IsNullOrWhiteSpace(_refreshToken)) {
                _logger.LogWarning("Refresh token is not available. User needs to log in again.");
                return string.Empty; // If refresh fails, return empty token
            }

            if (await RefreshCoreAsync(_refreshToken, _localUser) is false) {
                _logger.LogWarning("Failed to refresh token for user {UserName}. User needs to log in again.", _localUser?.UserDisplayName);
                return string.Empty; // If refresh fails, return empty token
            }

            return _token;

        } finally {
            _refreshLock.Release();
        }

    }

    /// <summary>
    /// Exchanges <paramref name="refreshToken"/> for a new token pair. Callers must hold <see cref="_refreshLock"/>.
    /// </summary>
    /// <param name="refreshToken">The refresh token to present.</param>
    /// <param name="user">The user to fall back to when the API response carries no user.</param>
    /// <returns><see langword="true"/> if the exchange succeeded and the new pair is stored; otherwise <see langword="false"/>.</returns>
    private async Task<bool> RefreshCoreAsync(string refreshToken, User? user) {

        RefreshResult result;
        try {
            result = await _webAPI.RefreshTokenAsync(new RefreshRequest(refreshToken));
        } catch (Exception ex) {
            _logger.LogError(ex, "Error occurred while refreshing token.");
            return false; // Treated as transient: the credentials are left alone so the next attempt can retry
        }

        switch (result.Outcome) {
            case RefreshOutcome.Success when result.Response is not null:
                RefreshResponse response = result.Response;
                User refreshedUser = response.User is { } apiUser
                    ? new User { UserId = apiUser.Id, UserDisplayName = string.IsNullOrWhiteSpace(apiUser.DisplayName) ? apiUser.Username : apiUser.DisplayName }
                    : user ?? throw new InvalidOperationException("Refresh response carried no user and none was cached.");
                ApplyTokens(response.Token, response.RefreshToken, GetTokenExpiration(response.Token, null), refreshedUser);
                await PersistTokensAsync();
                _logger.LogInformation("Access token refreshed; it now expires at {Expiration:u}.", _tokenExpiration);
                return true;

            case RefreshOutcome.Rejected:
                if (result.ErrorCode is "RefreshToken.Breach") {
                    _logger.LogError("The API reported a refresh token breach; all sessions for this account have been revoked server-side.");
                } else {
                    _logger.LogWarning("The refresh token was rejected ({ErrorCode}). Local credentials will be cleared.", result.ErrorCode ?? "no error code");
                }
                ClearLocalCredentials();
                return false;

            default:
                _logger.LogWarning("Token refresh did not succeed ({ErrorCode}); keeping the stored credentials for a later attempt.", result.ErrorCode ?? "no error code");
                return false;
        }

    }

    /// <summary>
    /// Applies a token pair to the in-memory session. Does not persist -- call <see cref="PersistTokensAsync"/> after.
    /// </summary>
    [MemberNotNull(nameof(_localUser), nameof(_token), nameof(_refreshToken), nameof(_tokenExpiration))]
    private void ApplyTokens(string token, string refreshToken, DateTime tokenExpiration, User user) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        _localUser = user;
        _token = token;
        _refreshToken = refreshToken;
        _tokenExpiration = tokenExpiration;
        _hasLoggedInUser.TrySetResult(true); // TrySetResult: this runs again on every refresh, and SetResult would throw
        _webAPI.SetAuthenticationToken(_token); // Set the authentication token for the web API
        ScheduleProactiveRefresh(Subtract(_tokenExpiration - DateTime.UtcNow, ProactiveLead));
    }

    /// <summary>
    /// Arms the background refresh to run after <paramref name="delay"/>.
    /// </summary>
    private void ScheduleProactiveRefresh(TimeSpan delay) {
        if (delay < MinimumProactiveDelay) {
            delay = MinimumProactiveDelay;
        }
        _refreshTimer ??= new Timer(_ => _ = ProactiveRefreshAsync(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _refreshTimer.Change(delay, Timeout.InfiniteTimeSpan);
        _logger.LogDebug("Background token refresh scheduled in {Delay}.", delay);
    }

    private void StopProactiveRefresh() => _refreshTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

    /// <summary>
    /// Renews the access token ahead of the point at which callers would start waiting on it.
    /// </summary>
    /// <remarks>A success re-arms the timer through <see cref="ApplyTokens"/>; a rejection stops it via
    /// <see cref="ClearLocalCredentials"/>. Only a transient failure needs re-arming here, and it is worth doing --
    /// otherwise the next attempt is the reactive one, with a caller waiting on it.</remarks>
    private async Task ProactiveRefreshAsync() {
        try {
            if (string.IsNullOrEmpty(await GetToken(ProactiveLead)) && !string.IsNullOrWhiteSpace(_refreshToken)) {
                _logger.LogDebug("Background token refresh did not succeed; retrying in {Interval}.", ProactiveRetryInterval);
                ScheduleProactiveRefresh(ProactiveRetryInterval);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Background token refresh failed.");
            ScheduleProactiveRefresh(ProactiveRetryInterval);
        }
    }

    private static TimeSpan Subtract(TimeSpan remaining, TimeSpan lead) => remaining > lead ? remaining - lead : TimeSpan.Zero;

    /// <summary>
    /// Writes the current token pair to the encrypted store.
    /// </summary>
    private async Task PersistTokensAsync() {
        try {
            await StoreTokenInEncryptedFileAsync(_token, _refreshToken, _tokenExpiration, DateTime.UtcNow, _localUser);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to persist the token store; removing it so a spent refresh token is not replayed at next start.");
            DeleteTokenStore();
        }
    }

    /// <summary>
    /// Drops the local session: in-memory tokens, the web API's bearer, and the on-disk store.
    /// </summary>
    private void ClearLocalCredentials() {
        StopProactiveRefresh();
        _token = string.Empty;
        _tokenExpiration = DateTime.MinValue;
        _refreshToken = string.Empty;
        _localUser = null;
        _hasLoggedInUser = new TaskCompletionSource<bool>();
        _webAPI.SetAuthenticationToken(string.Empty);
        DeleteTokenStore();
    }

    private void DeleteTokenStore() {
        try {
            if (File.Exists(_userTokenStore)) {
                File.Delete(_userTokenStore);
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to delete the local token store at {Path}.", _userTokenStore);
        }
    }

    public Task<string> GetLocalUserTokenAsync() => GetToken();

    public async ValueTask<bool> AutoLoginAsync() {

        if (_localUser is not null) {
            return true; // Already logged in
        }

        if (!File.Exists(_userTokenStore)) {
            return false; // No local user token file found
        }

        StoredTokenData? tokenData;
        try {
            tokenData = GetTokenFromEncryptedFile();
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Could not read the local token store; discarding it and requiring a fresh sign-in.");
            DeleteTokenStore();
            return false;
        }

        if (tokenData is null) {
            return false; // Token is empty or null
        }

        if (DateTime.UtcNow >= tokenData.Expiration - RefreshSkew) {
            // Token is expired, attempt to refresh it before the first authenticated call needs it
            _refreshToken = tokenData.RefreshToken;
            await _refreshLock.WaitAsync();
            try {
                return await RefreshCoreAsync(tokenData.RefreshToken, tokenData.User);
            } finally {
                _refreshLock.Release();
            }
        }

        if (tokenData.User is null) {
            _logger.LogWarning("The local token store holds no user; requiring a fresh sign-in.");
            DeleteTokenStore();
            return false;
        }

        ApplyTokens(tokenData.Token, tokenData.RefreshToken, tokenData.Expiration, tokenData.User);
        return true;

    }

    public Task<User> LoginWithDiscordAsync() => LoginWithProviderAsync(AuthProvider.Discord);

    public Task<User> LoginWithSteamAsync() => LoginWithProviderAsync(AuthProvider.Steam);

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

        EndAuthResponse? endAuthResponse = await _webAPI.EndAuthAsync(provider, startAuthResponse.SessionId, startAuthResponse.Verifier);
        if (endAuthResponse is null) {
            _logger.LogWarning("Authentication session ended without response for provider {Provider}.", provider);
            throw new InvalidOperationException("Authentication session ended without response.");
        }

        _logger.LogInformation("Authentication with {Provider} completed successfully.", provider);

        ApplyTokens(endAuthResponse.Token, endAuthResponse.RefreshToken, GetTokenExpiration(endAuthResponse.Token, endAuthResponse.ExpiresAt), new User {
            UserId = endAuthResponse.User.Id,
            UserDisplayName = endAuthResponse.User.DisplayName,
        });
        await PersistTokensAsync();

        _logger.LogInformation("User {UserName} with Id {Id} logged in successfully via {Provider}.", _localUser.UserDisplayName, _localUser.UserId, provider);

        return _localUser;

    }

    /// <summary>
    /// Determines when <paramref name="accessToken"/> expires.
    /// </summary>
    private DateTime GetTokenExpiration(string accessToken, DateTime? fallback) {
        if (TryGetJwtExpiry(accessToken, out DateTime expiresUtc)) {
            return expiresUtc;
        }
        if (fallback is { } reported) {
            _logger.LogDebug("Could not read the expiry from the access token; using the reported expiry instead.");
            return reported.Kind is DateTimeKind.Utc ? reported : reported.ToUniversalTime();
        }
        _logger.LogWarning("Could not determine the access token expiry; assuming one hour.");
        return DateTime.UtcNow.AddHours(1);
    }

    private static bool TryGetJwtExpiry(string jwt, out DateTime expiresUtc) {

        expiresUtc = default;
        if (string.IsNullOrWhiteSpace(jwt)) {
            return false;
        }

        string[] segments = jwt.Split('.');
        if (segments.Length is not 3) {
            return false;
        }

        try {
            byte[] payload = Base64Url.DecodeFromChars(segments[1]);
            using JsonDocument document = JsonDocument.Parse(payload);
            if (!document.RootElement.TryGetProperty("exp", out JsonElement exp) || !exp.TryGetInt64(out long expSeconds)) {
                return false;
            }
            expiresUtc = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            return true;
        } catch (Exception) {
            return false;
        }

    }

    private StoredTokenData? GetTokenFromEncryptedFile() {
        if (!File.Exists(_userTokenStore)) {
            throw new FileNotFoundException("Local user token file not found.", _userTokenStore);
        }
        byte[] cipherText = File.ReadAllBytes(_userTokenStore);
        byte[] plainText = ProtectedData.Unprotect(cipherText, null, DataProtectionScope.CurrentUser);
        return JsonSerializer.Deserialize<StoredTokenData>(Encoding.UTF8.GetString(plainText));
    }

    private async Task StoreTokenInEncryptedFileAsync(string token, string refreshToken, DateTime expiration, DateTime issuedAt, User? user) {
        if (string.IsNullOrWhiteSpace(token)) {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        var tokenData = new StoredTokenData(token, refreshToken, issuedAt, expiration, DateTime.UtcNow, user);
        byte[] plainText = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tokenData));
        byte[] cipherText = ProtectedData.Protect(plainText, null, DataProtectionScope.CurrentUser);
        Directory.CreateDirectory(Path.GetDirectoryName(_userTokenStore)!);
        await File.WriteAllBytesAsync(_userTokenStore, cipherText);
    }

    public void Dispose() {
        _refreshTimer?.Dispose();
        _refreshLock.Dispose();
    }

}
