using System.Text.Json.Serialization;

using Battlegrounds.Models;

namespace Battlegrounds.Facades.API;
public sealed record LoginRequest(
        [property: JsonPropertyName("username")] string UserName,
        [property: JsonPropertyName("password")] string Password
    );
public sealed record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("user")] User User
);
public sealed record RefreshRequest(
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
public sealed record RefreshResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);

public interface IBattlegroundsWebAPI {
    
    Task<string> GetPublicKeyAsync();

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<RefreshResponse> RefreshTokenAsync(RefreshRequest request);

    void SetAuthenticationToken(string token);

}
