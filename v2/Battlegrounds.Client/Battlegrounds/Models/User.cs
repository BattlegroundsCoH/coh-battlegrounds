namespace Battlegrounds.Models;

public sealed class User {

    private string _username = string.Empty;
    private string _userId = string.Empty;

    public string UserId {
        get => _userId;
        init {
            if (value == _userId)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("UserId cannot be null or empty.", nameof(value));
            _userId = value;
        }
    }

    public string Username {
        get => _username;
        init {
            if (value == _username)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be null or empty.", nameof(value));
            _username = value;
        }
    }

}
