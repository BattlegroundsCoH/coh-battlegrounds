using Battlegrounds.Core.Users;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services.Standard;

public class UserService(
    ILogger<UserService> logger) : IUserService {

    private UserContext context = new(0L, "Yolo", 0);

    public UserContext UserContext => context;

}
