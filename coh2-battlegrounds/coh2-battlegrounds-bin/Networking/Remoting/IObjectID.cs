namespace Battlegrounds.Networking.Remoting;

/// <summary>
/// Interface for ID of objects that can be accessed remotely.
/// </summary>
public interface IObjectID {

    /// <summary>
    /// Convert the Id into a string representation.
    /// </summary>
    /// <returns>A string representation of the ID.</returns>
    public string ToString();

    /// <summary>
    /// Parse the <paramref name="str"/> input into a <see cref="IObjectID"/> representation.
    /// </summary>
    /// <param name="str">The string value to convert into a <see cref="IObjectID"/> object.</param>
    /// <exception cref="System.ArgumentException"/>
    public void FromString(string str);

}
