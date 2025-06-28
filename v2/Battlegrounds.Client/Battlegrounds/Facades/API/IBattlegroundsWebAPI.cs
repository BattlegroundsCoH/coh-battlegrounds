using System.Text.Json.Serialization;

namespace Battlegrounds.Facades.API;
public sealed record LoginRequest(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("password")] string Password
    );
public sealed record LoginResponse(
    [property: JsonPropertyName("access_token")] string Token,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("expires_at")] long ExpiresAt,
    [property: JsonPropertyName("user")] ApiUser User
);
public sealed record ApiUser(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("role")] string Role
);
public sealed record RefreshRequest(
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
public sealed record RefreshResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);

public sealed record StartAuthResponse(
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("auth_url")] string AuthUrl
);

public sealed record EndAuthResponse(
    [property: JsonPropertyName("session_id")] string SessionId,
    [property: JsonPropertyName("user")] ApiUser User,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("expires_at")] long ExpiresAt
);

public enum AuthProvider {
    Battlegrounds,
    Steam,
    Discord
}

public interface IBattlegroundsWebAPI {
    
    Task<string> GetPublicKeyAsync();

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<RefreshResponse> RefreshTokenAsync(RefreshRequest request);

    Task<StartAuthResponse?> StartAuthAsync(AuthProvider provider);

    Task<EndAuthResponse?> EndAuthAsync(AuthProvider provider, string sessionId);

    void SetAuthenticationToken(string token);

}
