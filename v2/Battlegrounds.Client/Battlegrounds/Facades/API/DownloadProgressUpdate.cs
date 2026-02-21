namespace Battlegrounds.Facades.API;

/// <summary>
/// Delegate for handling download progress updates during game mode downloads from the server. This delegate is invoked
/// </summary>
/// <param name="bytesDownloaded">The number of bytes that have been downloaded so far during the operation.</param>
/// <param name="totalBytes">The total number of bytes expected to be downloaded. This value can be null if the total size is unknown.</param>
public delegate void DownloadProgressUpdateDelegate(long bytesDownloaded, long? totalBytes);
