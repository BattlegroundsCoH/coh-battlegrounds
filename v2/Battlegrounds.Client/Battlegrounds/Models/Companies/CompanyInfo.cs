using System.Text.Json.Serialization;

namespace Battlegrounds.Models.Companies;

/// <summary>
/// Represents company information retrieved from a remote source, including identification, ownership, faction, and
/// file metadata.
/// </summary>
/// <remarks>This data transfer object is typically used to deserialize company details from external systems. All
/// properties are immutable and must be provided during initialization.</remarks>
public sealed class CompanyInfo { // DTO from remote

	/*
     GUID     string  `json:"guid"`
	UserID   string  `json:"userId"`
	Faction  Faction `json:"faction"`
	Modified string  `json:"modified"`
	Size     int64   `json:"size"`     // Size of company file
	Checksum string  `json:"checksum"` // SHA-256 of company file
	Version  int     `json:"version"`  // Version of the company
     */

	/// <summary>
	/// Gets the unique identifier for the company.
	/// </summary>
	[JsonPropertyName("guid")]
	public required string Id { get; init; }

	/// <summary>
	/// Gets the unique identifier for the user.
	/// </summary>
	[JsonPropertyName("userId")]
	public required string UserId { get; init; }

	/// <summary>
	/// Gets the faction associated with the company.
	/// </summary>
	[JsonPropertyName("faction")]
	public required string Faction { get; init; }

	/// <summary>
	/// Gets the timestamp indicating when the company was last modified.
	/// </summary>
	[JsonPropertyName("modified")]
	public required DateTime Modified { get; init; }

	/// <summary>
	/// Gets the size of the company file in bytes.
	/// </summary>
	[JsonPropertyName("size")]
	public required long Size { get; init; }

	/// <summary>
	/// Gets the SHA-256 checksum of the company file.
	/// </summary>
	[JsonPropertyName("checksum")]
	public required string Checksum { get; init; }

	/// <summary>
	/// Gets the version number associated with the current instance.
	/// </summary>
	[JsonPropertyName("version")]
	public uint Version { get; init; } = 0;

}
