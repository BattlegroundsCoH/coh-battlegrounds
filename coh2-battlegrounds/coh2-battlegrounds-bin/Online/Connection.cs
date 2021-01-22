using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Battlegrounds.Online {

    /// <summary>
    /// Handler for handling response messages.
    /// </summary>
    /// <param name="message">The response message to handle.</param>
    public delegate void MessageHandler(Message message);

    /// <summary>
    /// Represents a server-client connection. Provides abstraction for <see cref="Socket"/> send and receive methods. This class cannot be inherited.
    /// </summary>
    public sealed class Connection {

        Thread m_processThread;

        volatile Queue<Message> m_messageQueue;
        volatile Dictionary<int, MessageHandler> m_identifierCallback;
        volatile Dictionary<MessageType, MessageHandler> m_typeCallback;
        volatile Socket m_socket;
        volatile bool m_isOpen;
        volatile bool m_isListening;

        /// <summary>
        /// The <see cref="Socket"/> the <see cref="Connection"/> instance is using.
        /// </summary>
        public Socket ConnectionSocket => this.m_socket;

        /// <summary>
        /// Flag for connection state.
        /// </summary>
        public bool IsConnected => this.m_socket?.Connected ?? false;

        /// <summary>
        /// The event to trigger when a <see cref="Message"/> has been received.
        /// </summary>
        public event MessageHandler OnMessage;

        /// <summary>
        /// Create a new <see cref="Connection"/> instance for a <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The socket to base <see cref="Connection"/> on.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ThreadStartException"/>
        /// <exception cref="OutOfMemoryException"/>
        public Connection(Socket socket) {

            this.m_socket = socket;
            this.m_isOpen = true;
            this.m_identifierCallback = new Dictionary<int, MessageHandler>();
            this.m_typeCallback = new Dictionary<MessageType, MessageHandler>();
            this.m_messageQueue = new Queue<Message>();
            this.m_isListening = false;

            this.Start();

        }

        /// <summary>
        /// Create a new <see cref="Connection"/> instance for a <see cref="Socket"/>. Can start or wait with listening for messages.
        /// </summary>
        /// <param name="socket">The socket to base <see cref="Connection"/> on.</param>
        /// <param name="startListening">Start listening for new messages once instantiated.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ThreadStartException"/>
        /// <exception cref="OutOfMemoryException"/>
        public Connection(Socket socket, bool startListening) {

            this.m_socket = socket;
            this.m_isOpen = startListening;
            this.m_identifierCallback = new Dictionary<int, MessageHandler>();
            this.m_typeCallback = new Dictionary<MessageType, MessageHandler>();
            this.m_messageQueue = new Queue<Message>();
            this.m_isListening = false;

            if (this.m_isOpen) {
                this.Start();
            }

        }

        private void MessageReceived(Socket source, Message message) {
            this.m_isListening = false;
            if (message.Descriptor == MessageType.SERVER_PING) {
                this.SendMessage(message.CreateResponse(MessageType.USER_PING));
            } else {
                Trace.WriteLine($"Received message <<{message}>> ({message.ToBytes().Length} bytes)", "Online-Service");
                this.HandleReceivedMessage(message);
            }
            if (this.m_isOpen) {
                if (source != this.m_socket) {
                    Trace.WriteLine("Socket-Mismatch!", "Online-Service");
                }
                this.Listen();
            }
        }

        private void HandleReceivedMessage(Message message) { // Handle the received message and invoke the proper callbacks
            if (this.m_typeCallback?.ContainsKey(message.Descriptor) ?? false) {
                this.m_typeCallback[message.Descriptor].Invoke(message);
            }
            if (this.m_identifierCallback?.ContainsKey(message.Identifier) ?? false) {
                this.m_identifierCallback[message.Identifier].Invoke(message);
            } else {
                this.OnMessage?.Invoke(message);
            }
        }

        private void InternalProccessor() {
            while (this.m_isOpen || this.m_messageQueue.Count > 0) {
                try {
                    if (this.m_messageQueue.Count > 0) {

                        Message topMessage = this.m_messageQueue.Dequeue();

                        lock (this.m_socket) {
                            byte[] msg = topMessage.ToBytes();
                            this.m_socket.SendAll(msg);
                            Trace.WriteLine($"Sent message <<{topMessage}>> ({msg.Length} bytes)", "Online-Service");
                        }

                        Thread.Sleep(120);

                    } else {
                        Thread.Sleep(60);
                    }
                } catch (ObjectDisposedException) {
                    break;
                }
            }
        }

        /// <summary>
        /// Start listening for <see cref="Message"/> data.
        /// </summary>
        public void Listen() {
            if (!this.m_isListening) {
                MessageSender.WaitForMessage(this.m_socket, this.MessageReceived);
                this.m_isListening = true;
            }
        }

        /// <summary>
        /// Enqueue a <see cref="Message"/> to be sent as soon as possible.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> instance to enqueue and send.</param>
        public void SendMessage(Message message)
            => this.m_messageQueue.Enqueue(message);

        /// <summary>
        /// Enqueue a <see cref="Message"/> to be sent as soon as possible and attach a response handler.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="responseHandler">The handler for handling the message response.</param>
        /// <returns>The <see cref="int"/> identifier that was assigned to the <see cref="Message"/>.</returns>
        public int SendMessageWithResponse(Message message, MessageHandler responseHandler) {
            Message.SetIdentifier(this.m_socket, message);
            this.SetIdentifierReceiver(message.Identifier, responseHandler);
            this.SendMessage(message);
            this.Listen();
            return message.Identifier;
        }

        /// <summary>
        /// Enqueue a <see cref="Message"/> to be sent as soon as possible and attach a response handler.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="messageType">The message type to listen for in response.</param>
        /// <param name="responseHandler">The handler for handling the message response.</param>
        public void SendMessageWithResponseListener(Message message, MessageType messageType, MessageHandler responseHandler) {
            Message.SetIdentifier(this.m_socket, message);
            this.SetTypeListener(messageType, responseHandler);
            this.SendMessage(message);
            this.Listen();
        }

        /// <summary>
        /// Send a message to the <see cref="Connection"/> as if sent from an external source.
        /// </summary>
        /// <param name="message">The message to self-handle.</param>
        public void SendSelfMessage(Message message) => this.HandleReceivedMessage(message);

        /// <summary>
        /// Start listening and sending <see cref="Message"/> data.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ThreadStartException"/>
        /// <exception cref="OutOfMemoryException"/>
        public void Start() {
            this.m_isOpen = true;
            this.m_processThread = new Thread(this.InternalProccessor);
            this.m_processThread.Start();
            this.m_isListening = false; // reset in next func call
            this.Listen();
        }

        /// <summary>
        /// Stop listening for and sending <see cref="Message"/> data.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException"/>
        /// <exception cref="ThreadStartException"/>
        /// <exception cref="SocketException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="System.Security.SecurityException"/>
        public void Stop() {

            this.m_isOpen = false;
            this.m_isListening = false;

            if (this.m_socket != null) {

                Task.Run(async () => { 
                    while (this.m_processThread.IsAlive) {
                        await Task.Delay(1);
                    }
                    this.m_socket.Shutdown(SocketShutdown.Both);
                    this.m_socket.Close();
                    this.m_socket = null;
                });

            }

        }

        /// <summary>
        /// Set intercepting <see cref="Message"/> identifier callback. When used, messages with identifier will be intercepted and the action invoked.
        /// This will bypass the <see cref="OnMessage"/> and <see cref="OnFile"/> events.
        /// </summary>
        /// <param name="identifier">The integer used to identify the message.</param>
        /// <param name="onMessage">The callback action to trigger when receiving message with identifier.</param>
        /// <exception cref="ArgumentException"/>
        /// <remarks>Remember to use <see cref="ClearIdentifierReceiver(int)"/> when done.</remarks>
        public void SetIdentifierReceiver(int identifier, MessageHandler onMessage) { 
            if (this.m_identifierCallback.ContainsKey(identifier)) {
                throw new ArgumentException($"The identifier '{identifier}' already has a callback.", nameof(identifier));
            } else {
                this.m_identifierCallback.Add(identifier, onMessage);
            }
        }

        /// <summary>
        /// Clear the callback associated with an identifier.
        /// </summary>
        /// <param name="identifier">The identifier callback to remove.</param>
        public void ClearIdentifierReceiver(int identifier) {
            if (this.m_identifierCallback.ContainsKey(identifier)) {
                this.m_identifierCallback.Remove(identifier);
            }
        }

        /// <summary>
        /// Set an intercepting <see cref="MessageType"/> callback. When used, messages with identifier will be intercepted and the action invoked.
        /// </summary>
        /// <remarks>
        /// Remember to use <see cref="ClearTypeListener(MessageType)"/> when done.
        /// </remarks>
        /// <param name="messageType">The message type to listen for.</param>
        /// <param name="onMessage">The callback handler to handle the message.</param>
        /// <exception cref="ArgumentException"/>
        public void SetTypeListener(MessageType messageType, MessageHandler onMessage) {
            if (this.m_typeCallback.ContainsKey(messageType)) {
                throw new ArgumentException($"The message type '{messageType}' already has an attached listener (Cannot allow multiple listeners).", nameof(messageType));
            } else {
                this.m_typeCallback.Add(messageType, onMessage);
            }
        }

        /// <summary>
        /// Clear the listener associated with the <see cref="MessageType"/>.
        /// </summary>
        /// <param name="messageType">The message type to clear listener from.</param>
        public void ClearTypeListener(MessageType messageType) {
            if (this.m_typeCallback.ContainsKey(messageType)) {
                this.m_typeCallback.Remove(messageType);
            }
        }

        /// <summary>
        /// Get if there's a <see cref="MessageHandler"/> associated with <paramref name="messageType"/>.
        /// </summary>
        /// <param name="messageType">The <see cref="MessageType"/> to get listener sate of.</param>
        /// <returns>Will return <see langword="true"/> if there's a <see cref="MessageHandler"/> registered for <paramref name="messageType"/>. Otherwise <see langword="false"/>.</returns>
        public bool HasTypeListener(MessageType messageType) => this.m_typeCallback.ContainsKey(messageType);

        /// <summary>
        /// Override the type listener for <paramref name="messageType"/> with <paramref name="handler"/>.
        /// </summary>
        /// <remarks>
        /// Will register <paramref name="handler"/> as <see cref="MessageHandler"/> even if there's no previously associated <see cref="MessageHandler"/>.
        /// </remarks>
        /// <param name="messageType">The <see cref="MessageType"/> to override listener of.</param>
        /// <param name="handler">The <see cref="MessageHandler"/> to associate with <paramref name="messageType"/>.</param>
        public void OverrideTypeListener(MessageType messageType, MessageHandler handler) {
            this.ClearTypeListener(messageType);
            this.SetTypeListener(messageType, handler);
        }

    }

}
