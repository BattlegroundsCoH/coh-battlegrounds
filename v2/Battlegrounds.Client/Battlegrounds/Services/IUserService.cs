using Battlegrounds.Models;

namespace Battlegrounds.Services;

public interface IUserService {

    Task<User?> GetLocalUserAsync();

    Task<User> GetUserAsync(string userId);

    Task<User?> LoginAsync(string userName, string password);

    Task<bool> LogOutAsync();

    string GetLocalUserToken();

    Task<string> GetLocalUserTokenAsync(); // Will refresh token if expired

    string GetLocalUserRefreshToken();

}
