using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Battlegrounds.Online {
    
    /// <summary>
    /// 
    /// </summary>
    public static class MessageSender {
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="message"></param>
        /// <param name="response"></param>
        public static void SendMessage(IPEndPoint endpoint, Message message, Action<Socket, Message> response) {

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {

                sender.Connect(endpoint);

                byte[] bytes = message.ToBytes();

                sender.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, x => { sender.EndSend(x); if (response != null) { WaitForMessage(sender, response); } }, null);

            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <param name="response"></param>
        public static void SendMessage(Socket socket, Message message, Action<Socket, Message> response) {

            try {

                byte[] bytes = message.ToBytes();
                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, x => { socket.EndSend(x); if (response != null) { WaitForMessage(socket, response); } }, null);

            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="response"></param>
        public static void WaitForMessage(Socket socket, Action<Socket, Message> response) {
            byte[] buffer = new byte[1024];
            List<byte> backBuffer = new List<byte>();
            void Received(IAsyncResult result) {
                int received = socket.EndReceive(result);
                if (received > 0) {
                    backBuffer.AddRange(buffer[0..received]);
                    if (backBuffer.Count > 2 && backBuffer[^1] == 0x06 && backBuffer[^2] == 0x04) {
                        response(socket, Message.GetMessage(backBuffer.ToArray()));
                        backBuffer.Clear();
                    } else {
                        socket.BeginReceive(buffer, 0, 1024, 0, Received, null);
                    }
                }
            }
            socket.BeginReceive(buffer, 0, 1024, 0, Received, null);
        }

    }

}
