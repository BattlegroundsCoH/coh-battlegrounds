namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// Represents a string ID. Implementation of <see cref="IObjectID"/>. This class cannot be inherited.
/// </summary>
public sealed class StringID : IObjectID {

    private string m_str;

    /// <summary>
    /// Initialise a new <see cref="StringID"/> instance with a string identifier.
    /// </summary>
    /// <param name="str">The identifier this string object is identifying.</param>
    public StringID(string str) {
        this.m_str = str;
    }

    /// <summary>
    /// Get identifier from string.
    /// </summary>
    /// <param name="str">The identifier to set.</param>
    public void FromString(string str) => this.m_str = str;

    /// <summary>
    /// Get the identifier as string.
    /// </summary>
    /// <returns>String identifier.</returns>
    public override string ToString() => this.m_str;

    /// <summary>
    /// Checks if <paramref name="obj"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">Object to check against.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> has the same string ID as called instance; Otherwise <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is StringID sid && sid.m_str == this.m_str;

    /// <summary>
    /// Get the hash code of the string ID.
    /// </summary>
    /// <returns>Returns the hash code of the object.</returns>
    public override int GetHashCode() => base.GetHashCode();

}
