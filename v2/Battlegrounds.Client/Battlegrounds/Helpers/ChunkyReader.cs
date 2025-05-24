using System.IO;
using System.Text;

namespace Battlegrounds.Helpers;

public sealed class ChunkyReader(Stream source) : IDisposable {

    private readonly Stream _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly BinaryReader _reader = new BinaryReader(source, Encoding.UTF8, true);

    private readonly Dictionary<string, Chunk> _topChunks = [];

    public IReadOnlyDictionary<string, Chunk> Chunks => _topChunks;

    public long Position => _source.Position;

    public void Dispose() {
        ((IDisposable)_source).Dispose();
    }

    public void Parse() {

        byte[] chunkyHeader = Encoding.ASCII.GetBytes("Relic Chunky\r\n\u001A\0");
        byte[] actualChunkyHeader = _reader.ReadBytes(chunkyHeader.Length);
        if (!actualChunkyHeader.SequenceEqual(chunkyHeader)) {
            throw new InvalidDataException($"Invalid Chunky header: {BitConverter.ToString(actualChunkyHeader)}");
        }

        uint version = _reader.ReadUInt32();
        if (version != 4) {
            throw new InvalidDataException($"Unsupported Chunky version: {version}");
        }

        uint platform = _reader.ReadUInt32();
        if (platform != 1) {
            throw new InvalidDataException($"Unsupported Chunky platform: {platform}");
        }

        try {

            Stack<Chunk> chunkStack = [];
            while (_source.Position < _source.Length) {

                if (chunkStack.Count > 0) {
                    Chunk topChunk = chunkStack.Peek();
                    long endPos = topChunk.EndOfHeader + topChunk.Size;
                    if (_source.Position == endPos) {
                        chunkStack.Pop();
                        continue; // Finished with the folder chunk
                    }
                }

                Chunk? chunk = ReadChunk();
                if (chunk is null) {
                    break; // End of stream or invalid chunk
                }

                if (chunk.IsFolder) {
                    if (chunkStack.Count == 0) {
                        _topChunks.Add(chunk.Id, chunk);
                    }
                    chunkStack.Push(chunk);
                } else {
                    _source.Seek(chunk.Size, SeekOrigin.Current); // Skip the data for now
                    if (chunkStack.Count > 0) {
                        chunkStack.Peek().Chunks.Add(chunk.Id, chunk);
                    } else {
                        _topChunks.Add(chunk.Id, chunk);
                    }
                }

            }

        } catch (Exception ex) {
            throw new InvalidDataException("Error parsing Chunky file.", ex);
        }

    }

    private Chunk? ReadChunk() {

        long startPos = _source.Position;

        byte[] type = _reader.ReadBytes(4);
        bool isFolder = type.SequenceEqual(Encoding.ASCII.GetBytes("FOLD"));
        if (!isFolder && !type.SequenceEqual(Encoding.ASCII.GetBytes("DATA"))) {
            _source.Seek(-4, SeekOrigin.Current); // Rewind to read the next chunk type
            return null; // Invalid chunk type
        }

        string id = Encoding.ASCII.GetString(_reader.ReadBytes(4));
        if (id.Any(c => !char.IsLetterOrDigit(c))) {
            _source.Seek(-8, SeekOrigin.Current); // Rewind to read the next chunk type
            return null; // Invalid ID (EOF?)
        }

        uint version = _reader.ReadUInt32();
        uint size = _reader.ReadUInt32();

        uint nameSize = _reader.ReadUInt32();
        byte[] nameBytes = _reader.ReadBytes((int)nameSize);
        string name = Encoding.UTF8.GetString(nameBytes);

        return new Chunk(isFolder, id, version, size, name, startPos, _source.Position, []);

    }

    public void GoToChunk(Chunk chunk) {
        ArgumentNullException.ThrowIfNull(chunk);
        if (chunk.IsFolder) throw new InvalidOperationException("Cannot seek to a folder chunk directly.");
        _source.Seek(chunk.EndOfHeader, SeekOrigin.Begin);
    }

    public uint ReadUInt32() {
        if (_source.Position + 4 > _source.Length) {
            throw new EndOfStreamException("Attempted to read past the end of the stream.");
        }
        return _reader.ReadUInt32();
    }

    public ulong ReadUInt64() {
        if (_source.Position + 8 > _source.Length) {
            throw new EndOfStreamException("Attempted to read past the end of the stream.");
        }
        return _reader.ReadUInt64();
    }

    public void Advance(long offset) {
        if (_source.Position + offset > _source.Length || _source.Position + offset < 0) {
            throw new EndOfStreamException("Attempted to advance past the end of the stream.");
        }
        _source.Seek(offset, SeekOrigin.Current);
    }

    public byte ReadByte() {
        if (_source.Position + 1 > _source.Length) {
            throw new EndOfStreamException("Attempted to read past the end of the stream.");
        }
        return _reader.ReadByte();
    }

    public string ReadUTF16String(int utf8StrLen) {
        if (utf8StrLen == -1)
            utf8StrLen = (int)_reader.ReadUInt32(); // Read length if -1 is specified
        uint actualLength = (uint)utf8StrLen * 2; // UTF-16 is 2 bytes per character
        if (_source.Position + actualLength > _source.Length) {
            throw new EndOfStreamException("Attempted to read past the end of the stream.");
        }
        byte[] bytes = _reader.ReadBytes((int)actualLength);
        return Encoding.Unicode.GetString(bytes);
    }

    public string ReadASCIIString(int asciiStrLen) {
        if (asciiStrLen == -1)
            asciiStrLen = (int)_reader.ReadUInt32(); // Read length if -1 is specified
        if (_source.Position + asciiStrLen > _source.Length) {
            throw new EndOfStreamException("Attempted to read past the end of the stream.");
        }
        byte[] bytes = _reader.ReadBytes(asciiStrLen);
        return Encoding.ASCII.GetString(bytes);
    }


    public sealed record Chunk(bool IsFolder, string Id, uint Version, uint Size, string Name, long StartOfHeader, long EndOfHeader, Dictionary<string, Chunk> Chunks);

}
