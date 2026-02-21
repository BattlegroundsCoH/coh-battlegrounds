namespace Battlegrounds.Models;

/// <summary>
/// Represents a user account with an email address, unique user identifier, and display name.
/// </summary>
/// <remarks>The display name defaults to the email address if not explicitly set. All properties are initialized
/// with validation to ensure that values are not null or empty.</remarks>
public sealed class User {

    /// <summary>
    /// Gets the email address associated with the user. This property can only be set during object initialization.
    /// </summary>
    /// <remarks>The email address must be a non-empty, non-whitespace string. Attempting to set this property
    /// to null, an empty string, or whitespace will result in an ArgumentException.</remarks>
    public string Email {
        get => field;
        init {
            if (value == field)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be null or empty.", nameof(value));
            field = value;
        }
    } = string.Empty;

    /// <summary>
    /// Gets the unique identifier for the user. This property must be set to a non-empty, non-whitespace string during
    /// object initialization.
    /// </summary>
    /// <remarks>An attempt to set this property to a null, empty, or whitespace value will result in an
    /// ArgumentException. The property is initialized to an empty string by default.</remarks>
    public string UserId {
        get => field;
        init {
            if (value == field)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("UserId cannot be null or empty.", nameof(value));
            field = value;
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the user. If not set, the user's email address is used as the display name.
    /// </summary>
    /// <remarks>The display name must not be null, empty, or consist only of white-space characters.
    /// Attempting to set this property to an invalid value will result in an ArgumentException.</remarks>
    public string UserDisplayName {
        get => string.IsNullOrEmpty(field) ? Email : field;
        init {
            if (value == field)
                return;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be null or empty.", nameof(value));
            field = value;
        }
    } = string.Empty;

}
