using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Battlegrounds.Online {
    
    internal static class MessageSender {

        internal static void SendMessage(IPEndPoint endpoint, Message message, Action<Socket, Message> response) {

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {

                sender.Connect(endpoint);

                byte[] bytes = message.ToBytes();

                sender.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, x => { sender.EndSend(x); if (response != null) { WaitForMessage(sender, response); } }, null);

            } catch (Exception e) {
                Trace.WriteLine(e.Message);
            }

        }

        internal static void SendMessage(Socket socket, Message message, Action<Socket, Message> response) {

            try {

                byte[] bytes = message.ToBytes();
                socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, x => { socket.EndSend(x); if (response != null) { WaitForMessage(socket, response); } }, null);

            } catch (Exception e) {
                Trace.WriteLine(e.Message);
            }

        }

        internal static void WaitForMessage(Socket socket, Action<Socket, Message> response) {
            try {
                byte[] buffer = new byte[2048];
                List<byte> backBuffer = new List<byte>();
                void Received(IAsyncResult result) {
                    try {
                        int received = socket.EndReceive(result);
                        if (received > 0) {
                            backBuffer.AddRange(buffer[0..received]);
                            if (backBuffer.Count > 2 && backBuffer[^1] == 0x06 && backBuffer[^2] == 0x04) {
                                Message.GetMessages(backBuffer.ToArray()).Invoke(x => response(socket, x));
                            } else {
                                socket.BeginReceive(buffer, 0, 2048, 0, Received, null);
                            }
                        } else {
                            if (backBuffer.Count > 0) {
                                Trace.WriteLine("Backbuffer contains content...");
                            }
                        }
                    } catch { }
                }
                socket.BeginReceive(buffer, 0, 2048, 0, Received, null);
            } catch { }
        }

    }

}
