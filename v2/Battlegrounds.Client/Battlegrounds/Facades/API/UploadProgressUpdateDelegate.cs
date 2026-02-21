namespace Battlegrounds.Facades.API;

/// <summary>
/// Represents a method that handles upload progress updates, providing information about the current progress,
/// completion status, and total size of the upload.
/// </summary>
/// <remarks>This delegate can be used to report incremental progress during an upload operation. Implementations
/// should ensure that UI updates or other side effects are thread-safe if invoked from background threads.</remarks>
/// <param name="progress">The current progress of the upload, expressed as a value between 0.0 and 1.0, where 1.0 indicates 100% completion.</param>
/// <param name="completed">A value indicating whether the upload has completed. Set to <see langword="true"/> if the upload is finished;
/// otherwise, <see langword="false"/>.</param>
/// <param name="size">The total size of the upload, in bytes.</param>
public delegate void UploadProgressUpdateDelegate(float progress, bool completed, ulong size);
