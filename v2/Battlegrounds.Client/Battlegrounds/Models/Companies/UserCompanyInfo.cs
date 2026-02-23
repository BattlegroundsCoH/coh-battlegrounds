using System.Text.Json.Serialization;

namespace Battlegrounds.Models.Companies;

/// <summary>
/// Represents information about a user and their associated companies, including company files and metadata.
/// </summary>
/// <remarks>This type is typically used to transfer user-company relationships and related file details in
/// scenarios such as API responses or data serialization. The contained company file information includes identifiers,
/// modification timestamps, and versioning data for each file associated with a company.</remarks>
public sealed class UserCompanyInfo {

    /// <summary>
    /// Gets the unique identifier for the user associated with this instance.
    /// </summary>
    [JsonPropertyName("userId")]
    public required string UserId { get; init; }

    /// <summary>
    /// Gets the collection of companies and their associated files. Grouped by faction.
    /// </summary>
    [JsonPropertyName("companies")]
    public required Dictionary<string, List<CompanyFile>> Companies { get; init; }

    /// <summary>
    /// Represents a company file with identifying and versioning information.
    /// </summary>
    /// <remarks>Instances of this class are immutable. The properties provide essential metadata for tracking
    /// and referencing company files, including a unique identifier, last modification timestamp, and version
    /// number.</remarks>
    public sealed class CompanyFile {

        /// <summary>
        /// Gets the unique identifier for the company.
        /// </summary>
        [JsonPropertyName("guid")]
        public required string Id { get; init; }

        /// <summary>
        /// Gets the timestamp indicating when the item was last modified.
        /// </summary>
        [JsonPropertyName("modified")]
        public required string Modified { get; init; }

        /// <summary>
        /// Gets the version number associated with the current instance.
        /// </summary>
        [JsonPropertyName("version")]
        public required uint Version { get; init; }

    }

}
