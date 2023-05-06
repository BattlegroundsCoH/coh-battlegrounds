namespace Battlegrounds.Resources;

/// <summary>
/// Interface for an icon source.
/// </summary>
public interface IIconSource {

    /// <summary>
    /// Get the container name of the icon.
    /// </summary>
    string Container { get; }

    /// <summary>
    /// Get the identifier of the icon.
    /// </summary>
    string Identifier { get; }

}
