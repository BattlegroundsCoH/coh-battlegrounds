namespace Battlegrounds.Core.Users;

public record struct UserContext(ulong UserId, string UserDisplayName, ulong ClientHash);
