using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Battlegrounds.Online {

    /// <summary>
    /// Extension class to <see cref="Socket"/>.
    /// </summary>
    public static class SocketExtension {

        /// <summary>
        /// Send all the byte content in an ordered manner.
        /// </summary>
        /// <param name="socket">The socket to use when sending the byte content.</param>
        /// <param name="content">The byte content to send.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SocketException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static void SendAll(this Socket socket, byte[] content) {
            int sent = 0;
            while (sent < content.Length) {
                sent += socket.Send(content[sent..^0]);
            }
        }

        /// <summary>
        /// Send all the byte content in an ordered and asynchronous manner.
        /// </summary>
        /// <param name="socket">The socket to use when sending the byte content.</param>
        /// <param name="content">The byte content to send.</param>
        /// <returns>The total amount of bytes sent or 0 if no bytes were sent.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SocketException"/>
        /// <exception cref="ObjectDisposedException"/>
        public static async Task<int> SendAllAsync(this Socket socket, byte[] content) {
            int total = 0;
            int sent;
            while (total < content.Length) {
                sent = await socket.SendAsync(content[total..^0], SocketFlags.None);
                total += sent;
            }
            return total;
        }

    }

}
