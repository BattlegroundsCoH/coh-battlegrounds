using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Battlegrounds.Online {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class Connection {

        Thread m_processThread;

        volatile Queue<Message> m_messageQueue;
        volatile Dictionary<int, Action<Message>> m_identifierCallback;
        volatile Socket m_socket;
        volatile bool m_isOpen;

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
            this.m_identifierCallback = new Dictionary<int, Action<Message>>();
            this.m_messageQueue = new Queue<Message>();

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
            this.m_identifierCallback = new Dictionary<int, Action<Message>>();
            this.m_messageQueue = new Queue<Message>();

            if (m_isOpen) {
                this.Start();
            }

        }

        private void _MessageReceived(Socket source, Message message) {
            Console.WriteLine(message.Descriptor);
            if (this.m_identifierCallback?.ContainsKey(message.Identifier) ?? false) {
                this.m_identifierCallback[message.Identifier].Invoke(message);
            } else {
                this.OnMessage?.Invoke(message);
            }
            if (this.m_isOpen) {
                if (source != this.m_socket) {
                    Console.WriteLine("Socket-Mismatch!");
                }
                Console.WriteLine("Listening");
                this.Listen();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Listen()
            => MessageSender.WaitForMessage(this.m_socket, this._MessageReceived);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(Message message)
            => this.m_messageQueue.Enqueue(message);

        /// <summary>
        /// 
        /// </summary>
        public void Start() {
            this.m_isOpen = true;
            this.m_processThread = new Thread(this._InternalProccessor);
            this.m_processThread.Start();
            this.Listen();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop() {

            this.m_isOpen = false;
            this.m_processThread.Abort();
            
            if (this.m_socket != null) {
                
                this.m_socket.Shutdown(SocketShutdown.Both);
                this.m_socket.Close();
                this.m_socket = null;

            }

        }

        private void _InternalProccessor() {
            while (this.m_isOpen) {

                if (this.m_messageQueue.Count > 0) {

                    Message topMessage = this.m_messageQueue.Dequeue();

                    lock (this.m_socket) {
                        this.m_socket.SendAll(topMessage.ToBytes());
                    }

                    Thread.Sleep(50);

                } else {
                    Thread.Sleep(100);
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="onMessage"></param>
        public void SetIdentifierReceiver(int identifier, Action<Message> onMessage) { 
            if (m_identifierCallback.ContainsKey(identifier)) {
                throw new Exception();
            } else {
                m_identifierCallback.Add(identifier, onMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        public void ClearIdentifierReceiver(int identifier) {
            if (m_identifierCallback.ContainsKey(identifier)) {
                m_identifierCallback.Remove(identifier);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="filepath"></param>
        /// <param name="identifier"></param>
        public void SendFile(string receiver, string filepath, int identifier = -1) {

            long len = new FileInfo(filepath).Length;
            if (len >= 64000000) {
                throw new Exception($"Attempt to send file of size {len / 1000.0 / 1000.0} MB, this is not allowed!. Only files smaller than 64MB can be sent.");
            }

            try {

                Message message = new Message(Message_Type.LOBBY_SENDFILE, Path.GetFileName(filepath), receiver) {
                    Identifier = identifier,
                    FileData = File.ReadAllBytes(filepath)
                };
                Message.SetIdentifier(this.m_socket, message);

                this.m_messageQueue.Enqueue(message);

            } catch (Exception e) {
                Console.WriteLine("[Client-Error] " + e.Message);
            }

        }


    }

}
