using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Online {

    public static class SocketExtension {

        public static void SendAll(this Socket socket, byte[] content) {
            int sent = 0;
            while (sent < content.Length) {
                sent += socket.Send(content[sent..^0]);
            }
        }

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
