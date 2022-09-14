namespace Battlegrounds.Verification;

/// <summary>
/// Interface for providing a specific method of extracting a checksum of an element.
/// </summary>
public interface IChecksumElement : IChecksumPropertyItem {

    /// <summary>
    /// Get the checksum value associated with this element.
    /// </summary>
    ulong Checksum { get; }

}
