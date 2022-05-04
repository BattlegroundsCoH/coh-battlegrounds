using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Util;

namespace Battlegrounds.Game.DataSource;

/// <summary>
/// Represents a chunky file containing <see cref="Chunk"/> data.
/// </summary>
public class ChunkyFile {

    protected byte[]? m_header;
    protected byte[]? m_unknowns;
    protected int m_version;
    protected List<Chunk> m_chunks;

    /// <summary>
    /// Initialsie a new <see cref="ChunkyFile"/> instance.
    /// </summary>
    public ChunkyFile() {
        this.m_chunks = new List<Chunk>();
    }

    /// <summary>
    /// Load a chunky file from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="stream">The binary reader (stream) to read binary chunky file data from.</param>
    /// <returns>If file was read <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool LoadFile(BinaryReader stream) {

        // Make sure the header is valid
        if (!this.VerifyHeader(stream)) {
            return false;
        }

        // Read the version
        this.m_version = stream.ReadInt32();

        // Verify version
        if (this.m_version != 3) {
            return false;
        }

        // Skip 4 bytes
        stream.Skip(4);

        // Read dummy bytes
        stream.ReadBytes(stream.ReadInt32() - 28);

        // While there's content to read
        while (stream.BaseStream.Position < stream.BaseStream.Length) {

            // Read chunk
            Chunk chunk = new Chunk(this.m_version);
            if (chunk.ReadChunk(stream)) {
                this.m_chunks.Add(chunk);
            } else {
                break; // done reading or reading into some other file content (should check the status of the chunk)
            }

        }

        // Return true
        return true;

    }

    private bool VerifyHeader(BinaryReader stream) {
        this.m_header = stream.ReadBytes(16);
        return ByteUtil.Match(this.m_header, "Relic Chunky\x0D\x0A\x1A\x00");
    }

    /// <summary>
    /// Lookup a root chunk by name in the chunk file
    /// </summary>
    /// <param name="name">The name of the chunk to find</param>
    /// <returns>The first chunk with name or null</returns>
    public Chunk? this[string name] => this.m_chunks.FirstOrDefault(x => x.Name.CompareTo(name) == 0);

    /// <summary>
    /// Walks the chunky file and the sub chunks, following the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Ordered string path to walk of the file.</param>
    /// <returns>The chunk at the end of the path or nothing</returns>
    public Chunk? Walk(params string[] path) {
        
        // Verify input length
        if (path.Length is 0)
            return null;

        // Grab first
        var res = this[path[0]];

        // Repeat until walked through all
        for (int i = 1; i < path.Length; i++) {
            if (res is null) {
                return null;
            }
            res = res[path[i]];
        }

        // Return last
        return res;

    }

}
