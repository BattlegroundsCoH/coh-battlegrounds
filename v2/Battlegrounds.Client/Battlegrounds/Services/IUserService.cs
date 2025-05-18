using Battlegrounds.Models;

namespace Battlegrounds.Services;

public interface IUserService {

    Task<User> GetLocalUserAsync();

    Task<User> GetUserAsync(string userId);

}
