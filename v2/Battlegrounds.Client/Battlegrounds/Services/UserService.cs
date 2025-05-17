using Battlegrounds.Models;

namespace Battlegrounds.Services;

public sealed class UserService : IUserService {
    public Task<User> GetLocalUserAsync() => Task.FromResult(new User() { Username = "local_user", UserId = "Local User" });
}
