using System.IO;

namespace Battlegrounds.Facades.API;

internal sealed class ProgressStream(Stream innerStream, long totalBytes, UploadProgressUpdateDelegate? progressCallback) : Stream {

    private readonly Stream _innerStream = innerStream;
    private readonly UploadProgressUpdateDelegate? _progressCallback = progressCallback;
    private readonly long _totalBytes = totalBytes;
    private long _bytesRead;

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _innerStream.Length;
    public override long Position {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count) {
        int bytesRead = _innerStream.Read(buffer, offset, count);
        ReportProgress(bytesRead);
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        int bytesRead = await _innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        ReportProgress(bytesRead);
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        int bytesRead = await _innerStream.ReadAsync(buffer, cancellationToken);
        ReportProgress(bytesRead);
        return bytesRead;
    }

    private void ReportProgress(int bytesRead) {
        _bytesRead += bytesRead;
        _progressCallback?.Invoke(_bytesRead, _bytesRead == _totalBytes, (ulong)_totalBytes);
    }

    public override void Flush() => _innerStream.Flush();
    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);
    public override void SetLength(long value) => _innerStream.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

}