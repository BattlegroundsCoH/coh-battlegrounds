using System.Text.Json.Serialization;

namespace Battlegrounds.Models.Gamemodes;

/// <summary>
/// Represents metadata for the latest win condition file, including its identifier, integrity information, size, and
/// upload timestamp.
/// </summary>
/// <param name="Tag">The unique tag identifying the win condition file version.</param>
/// <param name="Checksum">The checksum value used to verify the integrity of the win condition file.</param>
/// <param name="Size">The size of the win condition file, in bytes.</param>
/// <param name="UploadedAt">The date and time when the win condition file was uploaded.</param>
public sealed record LatestWinconditionDTO(
    [property:JsonPropertyName("tag")] string Tag,
    [property:JsonPropertyName("checksum")] string Checksum,
    [property:JsonPropertyName("size")] long Size,
    [property:JsonPropertyName("uploaded_at")] DateTime UploadedAt);
