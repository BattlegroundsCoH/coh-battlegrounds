namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents the result of an upload operation for a game mode.
/// </summary>
/// <remarks>Use this class to determine whether a game mode upload was successful. The <see cref="Failed"/>
/// property indicates if the upload encountered any issues.</remarks>
public sealed class UploadGamemodeResult {

    /// <summary>
    /// Gets a value indicating whether the operation has failed.
    /// </summary>
    /// <remarks>This property is set to <see langword="true"/> if the operation encountered an error;
    /// otherwise, it is <see langword="false"/>.</remarks>
    public bool Failed { get; init; }

}
