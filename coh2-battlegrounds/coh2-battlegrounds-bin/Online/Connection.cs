using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Battlegrounds.Online {
    
    /// <summary>
    /// 
    /// </summary>
    public class Connection {

        Socket m_socket;
        bool m_isOpen;

        /// <summary>
        /// 
        /// </summary>
        public Socket ConnectionSocket => m_socket;

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected => m_socket.Connected;

        /// <summary>
        /// 
        /// </summary>
        public event Action<Message> OnMessage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        public Connection(Socket socket) {

            this.m_socket = socket;
            this.m_isOpen = true;

            this.Start();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="startListening"></param>
        public Connection(Socket socket, bool startListening) {

            this.m_socket = socket;
            this.m_isOpen = startListening;

            if (m_isOpen) {
                this.Start();
            }

        }

        private void _MessageReceived(Socket source, Message message) {
            this.OnMessage?.Invoke(message);
            if (this.m_isOpen) {
                if (source != this.m_socket) {
                    Console.WriteLine("Socket-Mismatch!");
                }
                MessageSender.WaitForMessage(this.m_socket, this._MessageReceived);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(Message message) {
            MessageSender.SendMessage(this.m_socket, message, this._MessageReceived);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start() {
            this.m_isOpen = true;
            Console.WriteLine("Listening");
            MessageSender.WaitForMessage(this.m_socket, this._MessageReceived);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop() {

            this.m_isOpen = false;
            
            if (this.m_socket != null) {
                
                this.m_socket.Shutdown(SocketShutdown.Both);
                this.m_socket.Close();
                this.m_socket = null;

            }

        }

    }

}
