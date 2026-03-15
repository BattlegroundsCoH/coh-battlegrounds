using System.Text.Json.Serialization;

namespace Battlegrounds.Facades.API;

public sealed record LoginRequest(
        [property: JsonPropertyName("Username")] string Username,
        [property: JsonPropertyName("Password")] string Password
    );

public sealed record LoginResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt,
    [property: JsonPropertyName("user")] ApiUser User
);

public sealed record ApiUser(
    [property: JsonPropertyName("bgId")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("displayName")] string DisplayName
);

public sealed record RefreshRequest(
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);

public sealed record RefreshResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);

public sealed record StartAuthResponse(
    [property: JsonPropertyName("loginSessionId")] string SessionId,
    [property: JsonPropertyName("authLink")] string AuthUrl
);

public sealed record EndAuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("user")] ApiUser User,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt
);

/// <summary>
/// Represents the authentication providers supported by the application.
/// </summary>
/// <remarks>This enumeration is used to specify the source of user authentication, such as Battlegrounds, Steam,
/// or Discord.</remarks>
public enum AuthProvider {

    /// <summary>
    /// Uses the Battlegrounds platform for authentication, allowing users to log in with their Battlegrounds accounts. (Username/Password)
    /// </summary>
    Battlegrounds,

    /// <summary>
    /// Uses the Steam platform for authentication, allowing users to log in with their Steam accounts. (3rd Party OAuth)
    /// </summary>
    Steam,

    /// <summary>
    /// Uses Discord for authentication, enabling users to log in with their Discord accounts. (3rd Party OAuth)
    /// </summary>
    Discord

}

/// <summary>
/// Defines the contract for interacting with the Battlegrounds Web API,  providing methods for authentication, token
/// management, and public key retrieval.
/// </summary>
/// <remarks>This interface is designed to facilitate communication with the Battlegrounds Web API.  It includes
/// methods for obtaining a public key, managing authentication tokens,  and handling third-party authentication
/// flows.</remarks>
public interface IBattlegroundsWebAPI {
    
    Task<string> GetPublicKeyAsync();

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<RefreshResponse> RefreshTokenAsync(RefreshRequest request);

    /// <summary>
    /// Initiates an authentication process with the specified authentication provider.
    /// </summary>
    /// <param name="provider">The authentication provider to use for the authentication process.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a  <see cref="StartAuthResponse"/>
    /// object with details about the authentication process,  or <see langword="null"/> if the operation fails or is
    /// canceled.</returns>
    Task<StartAuthResponse?> StartAuthAsync(AuthProvider provider);

    /// <summary>
    /// Completes the authentication process for the specified provider and session.
    /// </summary>
    /// <param name="provider">The authentication provider used to complete the process.</param>
    /// <param name="sessionId">The unique identifier for the authentication session. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an  <see cref="EndAuthResponse"/>
    /// object with the authentication result, or <see langword="null"/>  if the session is invalid or the process could
    /// not be completed.</returns>
    Task<EndAuthResponse?> EndAuthAsync(AuthProvider provider, string sessionId);

    void SetAuthenticationToken(string token);

}
