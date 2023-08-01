using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using Battlegrounds.Logging;

namespace Battlegrounds.Compiler;

/// <summary>
/// Represents an archive definition with a table of contents and an output file path.
/// </summary>
public sealed class ArchiveDefinition {

    private static readonly Logger logger = Logger.CreateLogger();

    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions() {
#if DEBUG
        WriteIndented = true
#endif
    };

    /// <summary>
    /// Represents a table of contents within an archive.
    /// </summary>
    public readonly struct ArchiveTOC {

        /// <summary>
        /// Gets or sets the alias for this table of contents.
        /// </summary>
        [JsonPropertyName("alias")]
        public string Alias { get; init; }

        /// <summary>
        /// Gets or sets the name for this table of contents.
        /// </summary>
        [JsonPropertyName("toc_name")]
        public string TocName { get; init; }

        /// <summary>
        /// Gets or sets an array of archive files that belong to this table of contents.
        /// </summary>
        [JsonPropertyName("files")]
        public ArchiveFile[] Files { get; init; }

    }

    /// <summary>
    /// Represents a file within an archive.
    /// </summary>
    public readonly struct ArchiveFile {

        /// <summary>
        /// Gets or sets the name of this archive file.
        /// </summary>
        [JsonPropertyName("file_name")]
        public string FileName { get; init; }

        /// <summary>
        /// Gets or sets the relative path of this archive file.
        /// </summary>
        [JsonPropertyName("relative_path")]
        public string RelativePath { get; init; }

        /// <summary>
        /// Gets or sets the verification type for this archive file.
        /// </summary>
        [JsonPropertyName("verification")]
        public string Verification { get; init; }

        /// <summary>
        /// Gets or sets the storage type for this archive file.
        /// </summary>
        [JsonPropertyName("storage")]
        public string Storage { get; init; }

        /// <summary>
        /// Gets or sets the encryption type for this archive file.
        /// </summary>
        [JsonPropertyName("encryption")]
        public string Encryption { get; init; }

    }

    /// <summary>
    /// Gets or sets an array of table of contents within this archive.
    /// </summary>
    [JsonPropertyName("toc")]
    public ArchiveTOC[] TableOfContents { get; set; } = Array.Empty<ArchiveTOC>();

    /// <summary>
    /// Gets or sets the output file path for this archive definition.
    /// </summary>
    [JsonPropertyName("output_filepath")]
    public string OutputFilepath { get; set; } = "a.sga";

    /// <summary>
    /// Reads an archive definition from the specified file path.
    /// </summary>
    /// <param name="filepath">The file path to read the archive definition from.</param>
    /// <returns>An archive definition object if the file is successfully read, or null otherwise.</returns>
    public bool ToFile(string filepath) {
        try {

            // Delete file if exists
            if (File.Exists(filepath)) {
                File.Delete(filepath);
            }

            // Open for writing
            using var fs = File.OpenWrite(filepath);

            // Store file data
            JsonSerializer.Serialize(fs, this, options: serializerOptions);

        } catch (Exception e) {
            logger.Exception(e);
            return false;
        }
        return true;
    }

}
