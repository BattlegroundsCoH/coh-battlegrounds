namespace Battlegrounds.Networking.Server;

/// <summary>
/// Enum describing the result of an upload file API call.
/// </summary>
public enum UploadResult {
    
    /// <summary>
    /// File was uploaded.
    /// </summary>
    UPLOAD_SUCCESS,
    
    /// <summary>
    /// Request had invalid GUID.
    /// </summary>
    UPLOAD_INVALIDGUID,
    
    /// <summary>
    /// Request had invalid player ID.
    /// </summary>
    UPLOAD_INVALIDPLAYER,
    
    /// <summary>
    /// Request had no GUID.
    /// </summary>
    UPLOAD_MISSINGGUID,
    
    /// <summary>
    /// Request had no Player ID.
    /// </summary>
    UPLOAD_MISSINGPLAYER,
    
    /// <summary>
    /// Attempt to upload invalid filetype.
    /// </summary>
    UPLOAD_INVALIDFILETYPE,

    /// <summary>
    /// Atttempt to upload file without proper access.
    /// </summary>
    UPLOAD_ACCESS_DENIED,

    /// <summary>
    /// Upload failed for unknown reasons
    /// </summary>
    UPLOAD_ERROR_UNDEFINED,

}

/// <summary>
/// Enum describing the result of a download API call
/// </summary>
public enum DownloadResult {

    /// <summary>
    /// Requested file was successfully downloaded
    /// </summary>
    DOWNLOAD_SUCCESS,

    /// <summary>
    /// Request had invalid GUID.
    /// </summary>
    DOWNLOAD_INVALIDGUID,

    /// <summary>
    /// Request had invalid player ID.
    /// </summary>
    DOWNLOAD_INVALIDPLAYER,

    /// <summary>
    /// Request had no GUID.
    /// </summary>
    DOWNLOAD_MISSINGGUID,

    /// <summary>
    /// Request had no Player ID.
    /// </summary>
    DOWNLOAD_MISSINGPLAYER,

    /// <summary>
    /// Access was denied
    /// </summary>
    DOWNLOAD_ACCESS_DENIED,

    /// <summary>
    /// Unknown error occured
    /// </summary>
    DOWNLOAD_ERROR_UNDEFINED,

}

/// <summary>
/// Enum describing the outcome of a match.
/// </summary>
public enum ServerMatchResultsOutcome {
    
    /// <summary>
    /// Outcome is unknown or undefined.
    /// </summary>
    UNDEFINED,

    /// <summary>
    /// Axis win (WEH or OKW)
    /// </summary>
    AXIS_WIN,

    /// <summary>
    /// Allied wis (AEF, UKF, or SOV)
    /// </summary>
    ALLIES_WIN,

    /// <summary>
    /// No winners => No losers
    /// </summary>
    NO_WIN,

    /// <summary>
    /// Fatal Scar Error occured
    /// </summary>
    SCAR_ERROR,

    /// <summary>
    /// One or more parties crashed
    /// </summary>
    BUGSPLAT_ERROR

}
