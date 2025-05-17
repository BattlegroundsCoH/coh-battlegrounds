using Battlegrounds.Models;

namespace Battlegrounds.Services;

public interface IUserService {

    Task<User> GetLocalUserAsync();

}
