using System;
using System.Collections.Generic;
using System.IO;

using Battlegrounds.Util;

namespace Battlegrounds.Game.DataSource.Playback.CoH2;

/// <summary>
/// Class representation of a tick (update) in the game
/// </summary>
public sealed class GameTick : IPlaybackTick {

    private uint m_tick;
    private uint m_bundleCount;
    private TimeSpan m_tickTime;

    private readonly List<IPlaybackEvent> m_events;

    /// <summary>
    /// The timestamp of the <see cref="GameTick"/>.
    /// </summary>
    public TimeSpan Timestamp => m_tickTime;

    /// <summary>
    /// The events that occured within the span of the <see cref="GameTick"/>.
    /// </summary>
    public IList<IPlaybackEvent> Events => m_events;

    /// <summary>
    /// Create a new <see cref="GameTick"/> instance
    /// </summary>
    public GameTick() {
        m_tick = 0;
        m_events = new List<IPlaybackEvent>();
    }

    /// <summary>
    /// Parse a <see cref="GameTick"/> using a <see cref="BinaryReader"/> to extract the bundled data and populate the <see cref="GameEvent"/> list with results.
    /// </summary>
    /// <param name="reader"></param>
    public void Parse(BinaryReader reader) {

        // Skip the first byte
        reader.Skip(1);

        // Read the tick index
        m_tick = reader.ReadUInt32();

        // Calculate the timestamp
        m_tickTime = new TimeSpan((long)(10000000.0 * m_tick / 8.0));

        // Skip four bytes
        reader.Skip(4);

        // Read the amount of bundles
        m_bundleCount = reader.ReadUInt32();

        // Loop through the bundes
        for (uint i = 0; i < m_bundleCount; i++) {

            // Skip the first eight bytes
            reader.Skip(8);

            // Read the amount of events within the bundle
            uint count = reader.ReadUInt32();

            // Skip a byte
            reader.Skip(1);

            // If bundle is not empty
            if (count != 0) {

                // Byte offset
                uint j = 0;

                // While there's content to read
                while (count > j) {

                    // Read length
                    ushort length = reader.ReadUInt16();

                    // Parse game event and add to our list of events
                    GameEvent ge = new GameEvent(m_tickTime, reader.ReadBytes(length - 2));
                    m_events.Add(ge);

                    // Increment offset
                    j += length;

                }

            }

        }

    }

}
