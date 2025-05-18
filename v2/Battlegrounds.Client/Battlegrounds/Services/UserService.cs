using System.Net.Http;

using Battlegrounds.Models;

namespace Battlegrounds.Services;

public sealed class UserService(HttpClient httpClient) : IUserService {

    public Task<User> GetLocalUserAsync() => Task.FromResult(new User() { UserDisplayName = "Local User", UserId = "local_user_id" });

    public Task<User> GetUserAsync(string userId) => GetLocalUserAsync(); // TODO: Implement this method to fetch user data from Battlegrounds API

}
