using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace Battlegrounds.Networking.Communication;

/// <summary>
/// Static extension class for extending <see cref="Socket"/> functionality.
/// </summary>
public static class SocketExtensions {

    /// <summary>
    /// Send all bytes in input buffer.
    /// </summary>
    /// <param name="socket">The socket to send buffer data through.</param>
    /// <param name="buffer">The buffer to send.</param>
    internal static void SendAll(this Socket socket, byte[] buffer) {
        int i = 0;
        while (i < buffer.Length) {
            i += socket.Send(buffer[i..]);
        }
    }

    /// <summary>
    /// Receive all data coming through a socket.
    /// </summary>
    /// <param name="socket">The socket to receive data through.</param>
    /// <param name="buffer">The buffer to fill with received data.</param>
    /// <returns>The amount of bytes read.</returns>
    internal static int ReceiveAll(this Socket socket, out byte[] buffer) {

        try {

            // Read initial
            List<byte> bytes = new();
            byte[] _tmp = new byte[1024];
            int read = socket.Receive(_tmp);
            bytes.AddRange(_tmp[..read]);

            // Continue reading
            while (socket.Available > 0) {
                _tmp = new byte[1024];
                read = socket.Receive(_tmp);
                bytes.AddRange(_tmp[..read]);
            }

            // Copy
            buffer = bytes.ToArray();
            return buffer.Length;

        } catch (Exception e) {
            Trace.TraceError(e.Message);
        }

        // Return zero
        buffer = Array.Empty<byte>();
        return 0;

    }

}
