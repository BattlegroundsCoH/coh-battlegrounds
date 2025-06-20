using System.Text;

using DotNet.Testcontainers.Configurations;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Test;

public sealed class TestLogger<T> : ILogger<T>, IOutputConsumer, IDisposable {

    private readonly MemoryStreamLogger _memoryStreamLogger;

    public bool Enabled => true;

    public Stream Stdout => _memoryStreamLogger;

    public Stream Stderr => _memoryStreamLogger;

    public TestLogger() {
        _memoryStreamLogger = new MemoryStreamLogger(this);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;

    public void Dispose() {
        _memoryStreamLogger.Dispose();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        TestContext.Out.WriteLine($"[{logLevel}] {typeof(T).Name}: {formatter(state, exception)}");
    }

    private class MemoryStreamLogger(TestLogger<T> logger) : Stream {

        public override void Write(byte[] buffer, int offset, int count) {
            var logLine = Encoding.UTF8.GetString(buffer, offset, count);
            logger.Log(LogLevel.None, "{Message}", logLine);
        }

        #region Unused Stream Members
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        #endregion
    }

}
