namespace Battlegrounds.Models;

public sealed class User {

    private string _email = string.Empty;
    private string _displayName = string.Empty;
    private string _userId = string.Empty;

    public string Email {
        get => _email;
        init {
            if (value == _email)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be null or empty.", nameof(value));
            _email = value;
        }
    }

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
        get => string.IsNullOrEmpty(_displayName) ? _email : _displayName;
        init {
            if (value == _displayName)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be null or empty.", nameof(value));
            _displayName = value;
        }
    }

}
