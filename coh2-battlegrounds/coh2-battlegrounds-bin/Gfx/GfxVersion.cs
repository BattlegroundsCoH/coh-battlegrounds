namespace Battlegrounds.Gfx;

/// <summary>
/// Enum representing a version for a <see cref="StandardGfxMap"/>.
/// </summary>
public enum GfxVersion : int {

    /// <summary>
    /// The version is unspecified
    /// </summary>
    Unspecified,

    /// <summary>
    /// Version 1 (Original Version)
    /// </summary>
    /// <remarks>
    /// Basic data structure (Header, Manifest, ImageData)
    /// </remarks>
    V1 = 100,

    /// <summary>
    /// Version 2 (Updated version)
    /// </summary>
    /// <remarks>
    /// Faulty, same setup as Version 1 but ImageData may be compressed
    /// </remarks>
    V2 = 200,

    /// <summary>
    /// Version 2 (Fixed Version 2)
    /// </summary>
    /// <remarks>
    /// Fixed version of V2; use this when storing pure key/value data
    /// </remarks>
    V2_1 = 210,

    /// <summary>
    /// Version  3 (Latest Version)
    /// </summary>
    /// <remarks>
    /// Slightly more advanced data structure, storage setup is close to V2
    /// </remarks>
    V3 = 300,

    /// <summary>
    /// Specifies the latest version to use (Currently V3).
    /// </summary>
    Latest = V3

}
