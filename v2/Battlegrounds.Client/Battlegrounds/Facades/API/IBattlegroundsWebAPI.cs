using System.Text.Json.Serialization;

using Battlegrounds.Models;

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
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("user")] ApiUser? User
);

public enum RefreshOutcome {
    Success,
    Rejected,
    Transient
}

public sealed record RefreshResult(RefreshOutcome Outcome, RefreshResponse? Response, string? ErrorCode = null);

public sealed record StartAuthResponse(
    [property: JsonPropertyName("loginSessionId")] string SessionId,
    [property: JsonPropertyName("authLink")] string AuthUrl,
    [property: JsonPropertyName("verifier")] string Verifier
);

public sealed record EndAuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("user")] ApiUser User,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt
);

public sealed record NewsPreviewResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("authorRole")] string AuthorRole,
    [property: JsonPropertyName("isFeatured")] bool IsFeatured,
    [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("resources")] IReadOnlyList<string> Resources
);

public sealed record PagedNewsResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<NewsPreviewResponse> Items,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("hasMore")] bool HasMore
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
/// management, public key retrieval, and the news feed.
/// </summary>
/// <remarks>This interface is designed to facilitate communication with the Battlegrounds Web API.  It includes
/// methods for obtaining a public key, managing authentication tokens,  handling third-party authentication
/// flows, and reading the news feed with its resources.</remarks>
public interface IBattlegroundsWebAPI {

    Task<string> GetPublicKeyAsync();

    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<RefreshResult> RefreshTokenAsync(RefreshRequest request);

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
    /// <param name="verifier">The verifier string provided by the authentication provider. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an  <see cref="EndAuthResponse"/>
    /// object with the authentication result, or <see langword="null"/>  if the session is invalid or the process could
    /// not be completed.</returns>
    Task<EndAuthResponse?> EndAuthAsync(AuthProvider provider, string sessionId, string verifier);

    void SetAuthenticationToken(string token);

    Task<IReadOnlyList<NewsPreviewResponse>> GetLatestNewsAsync();

    Task<PagedNewsResponse?> GetNewsPageAsync(int page, int pageSize);

    Task<byte[]?> DownloadResourceAsync(string resourceId);

    string GetResourceUrl(string resourceId);

    /// <summary>
    /// Asynchronously retrieves user information based on the provided user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="User"/>
    /// object with the user's information, or <see langword="null"/> if the user is not found or an error occurs.</returns>
    Task<User?> GetUserAsync(string userId);

}
