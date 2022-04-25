namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// Represents an object key backed by an integer value. Implements <see cref="IObjectID"/>. This class cannot be inherited.
/// </summary>
public sealed class ObjectKey : IObjectID {

    int m_key;

    /// <summary>
    /// Initialise a new <see cref="ObjectKey"/> with a value of <see cref="int.MinValue"/>.
    /// </summary>
    public ObjectKey() => this.m_key = int.MinValue;

    /// <summary>
    /// Initialise a new <see cref="ObjectKey"/> instance from an integer.
    /// </summary>
    /// <param name="key">The object key</param>
    public ObjectKey(int key) => this.m_key = key;

    /// <summary>
    /// Initialise a new <see cref="ObjectKey"/> instance from a string.
    /// </summary>
    /// <param name="k">The object int key in string from.</param>
    /// <exception cref="System.ArgumentNullException"/>
    /// <exception cref="System.FormatException"/>
    /// <exception cref="System.OverflowException"/>
    public ObjectKey(string k) => this.m_key = int.Parse(k);

    /// <summary>
    /// Convert the given string into an <see cref="System.Int32"/> object key.
    /// </summary>
    /// <remarks>
    /// If <paramref name="str"/> is not a parsable integer, the key is set to -1.
    /// </remarks>
    /// <param name="str">The object key string to convert into string.</param>
    public void FromString(string str) {
        if (!int.TryParse(str, out this.m_key)) {
            this.m_key = -1;
        }
    }

    /// <summary>
    /// Get the key as a string representation.
    /// </summary>
    /// <returns>The string representation of the object.</returns>
    public override string ToString() => this.m_key.ToString();

    /// <summary>
    /// Checks if <paramref name="obj"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">Object to check against.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> has the same ID as called instance; Otherwise <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is ObjectKey k && k.m_key == this.m_key;

    /// <summary>
    /// Get the hash code of the object ID.
    /// </summary>
    /// <returns>Returns the hash code of the object.</returns>
    public override int GetHashCode() => this.m_key;

}
