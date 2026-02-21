using Battlegrounds.Models;

namespace Battlegrounds.Services;

/// <summary>
/// Defines methods for managing user authentication and retrieval in the application.
/// </summary>
/// <remarks>This interface provides functionality for user login, logout, token management, and user retrieval.
/// It supports both synchronous and asynchronous operations for handling user-related tasks.</remarks>
public interface IUserService {

    /// <summary>
    /// Asynchronously retrieves the local user associated with the current context.
    /// </summary>
    /// <remarks>This method is typically used to obtain information about the user currently associated with
    /// the application or system. The returned user object may include details such as username, ID, or other profile
    /// information, depending on the implementation.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains the local user as a <see
    /// cref="User"/> object, or <see langword="null"/> if no local user is available.</returns>
    Task<User?> GetLocalUserAsync();

    /// <summary>
    /// Asynchronously retrieves a user by their unique identifier.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to fetch user details. Ensure that the
    /// <paramref name="userId"/>  is valid and corresponds to an existing user in the system.</remarks>
    /// <param name="userId">The unique identifier of the user to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="User"/> object 
    /// corresponding to the specified <paramref name="userId"/>, or <see langword="null"/> if no user is found.</returns>
    Task<User> GetUserAsync(string userId);

    /// <summary>
    /// Authenticates a user asynchronously using their email and password.
    /// </summary>
    /// <remarks>Ensure that the provided email and password are valid and correspond to an existing user
    /// account. This method does not handle account lockout or multi-factor authentication scenarios.</remarks>
    /// <param name="userEmail">The email address of the user attempting to log in. Cannot be null or empty.</param>
    /// <param name="password">The password associated with the user's account. Cannot be null or empty.</param>
    /// <returns>A <see cref="User"/> object representing the authenticated user if the login is successful;  otherwise, <see
    /// langword="null"/> if the credentials are invalid.</returns>
    Task<User?> LoginAsync(string userEmail, string password);

    Task<bool> LogOutAsync();

    string GetLocalUserToken();

    Task<string> GetLocalUserTokenAsync(); // Will refresh token if expired

    string GetLocalUserRefreshToken();
    
    ValueTask<bool> AutoLoginAsync();
    Task<User> LoginWithDiscordAsync();
}
