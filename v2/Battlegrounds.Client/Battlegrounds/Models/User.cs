namespace Battlegrounds.Models;

public sealed class User {

    private string _displayName = string.Empty;
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

    public string UserDisplayName {
        get => _displayName;
        init {
            if (value == _displayName)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be null or empty.", nameof(value));
            _displayName = value;
        }
    }

}
