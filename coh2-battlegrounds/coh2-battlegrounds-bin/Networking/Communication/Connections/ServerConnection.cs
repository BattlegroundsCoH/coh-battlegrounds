using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Battlegrounds.Networking.Communication.Golang;

namespace Battlegrounds.Networking.Communication.Connections;

/// <summary>
/// Event handler for when a connection is lost unexpectedly.
/// </summary>
/// <param name="remoteTerminated">Flag marking if terminated remotely.</param>
public delegate void ConnectionLostHandler(bool remoteTerminated);

/// <summary>
/// Event handler for receiving messages.
/// </summary>
/// <param name="messageIdentifier">The identifier of the message chain. Use when responding to message.</param>
/// <param name="messageSender">The ID of the remote sender.</param>
/// <param name="messageContent">The actual message content.</param>
public delegate void MessageReceivedHandler(uint messageIdentifier, ulong messageSender, ContentMessage messageContent);

/// <summary>
/// Event handler for a callback on upload progress.
/// </summary>
/// <param name="currentChunk">The chunk that triggered this upload.</param>
/// <param name="expectedChunk">The expected amount of chunks to upload.</param>
/// <param name="isCancelled">Flag marking whether the operation was cancelled or not.</param>
public delegate void UploadProgressCallbackHandler(int currentChunk, int expectedChunk, bool isCancelled);

/// <summary>
/// Represents a connection to a server endpoint from local machine. Implements <see cref="IConnection"/>. This class cannot be inherited.
/// </summary>
public sealed class ServerConnection : IConnection {

    private bool m_listen;
    private readonly Socket m_socket;
    private readonly Thread m_lthread;
    private readonly Dictionary<ulong, Dictionary<uint, ContentMessage?>> m_receivedMessages;
    private readonly ReaderWriterLockSlim m_rwlock;

    /// <summary>
    /// Get if the connection is connected to remote endpoint.
    /// </summary>
    public bool IsConnected => this.m_socket.Connected;

    /// <summary>
    /// Get or set the handler for receiving messages. Can only be handled by one method at a time.
    /// </summary>
    public MessageReceivedHandler? MessageReceived { get; set; }

    /// <summary>
    /// Event triggered when the underlying socket connection is lost.
    /// </summary>
    public event ConnectionLostHandler? OnConnectionLost;

    /// <summary>
    /// Get the ID of this machine used for remote identification.
    /// </summary>
    public ulong SelfId { get; }

    private ServerConnection(Socket socket, ulong selfId) {
        this.m_socket = socket;
        this.m_receivedMessages = new() {
            [0] = new() // broker
        };
        this.m_lthread = new(this.Listen);
        this.m_rwlock = new();
        this.SelfId = selfId;
    }

    /// <summary>
    /// Shutdown server connection. Will attempt to inform remote endpoint before shutting down.
    /// </summary>
    public void Shutdown() {

        // Remove from interface
        NetworkInterface.UnregisterConnection(this);

        // Send disconnect message
        this.SendMessage(new Message() {
            CID = 0,
            Content = (new BrokerRequestMessage() { Request = BrokerRequestType.Disconnect }).Bytes(),
            Mode = MessageMode.Broker,
            Target = 0
        });

        // Stop listening
        this.m_listen = false;

        // Shutdown
        this.m_socket.Shutdown(SocketShutdown.Both);

        // Invoke connection lost
        this.OnConnectionLost?.Invoke(false);

    }
    
    /// <summary>
    /// Stop listening to remote endpoint.
    /// </summary>
    public void StopListen() {
        this.m_listen = false;
    }

    private List<Message>? ReceiveMessages() {

        // Store slice
        byte[] slice = Array.Empty<byte>();
        int received = this.m_socket.ReceiveBufferSize;

        // Create list 
        List<Message> messages = new List<Message>();

        // Try receive
        try {

            // Define big buffer list
            List<byte> bigBuffer = new();

            // Exhaust incoming data buffer
            while (this.m_socket.Available > 0) {

                // Prepare fresh buffer
                byte[] buffer = new byte[this.m_socket.ReceiveBufferSize];

                // Receive bytes
                received = this.m_socket.Receive(buffer);
                if (received == 0) { // EOF
                    return null;
                }

                // Add read bytes to big buffer
                bigBuffer.AddRange(buffer[..received]);

            }

            // Start interpreting messages
            slice = bigBuffer.ToArray();
            while (slice.Length > 0) {

                // Read next message in data
                var msg = GoMarshal.BytesUnmarshal(slice, out byte[] remaining);

                // Add found message to message list
                messages.Add(msg);

                // Update slice
                slice = remaining;

            }

            // Return found messages
            return messages;

        } catch (Exception ex) {
            Trace.WriteLine(ex, nameof(ServerConnection));
            return null;
        }

    }

    private void Listen() {

        while (this.m_listen) {

            // Get next message queue
            var messages = this.ReceiveMessages();

            // If null; connection was lost to server
            if (messages is null) {
                try {
                    if (!this.m_socket.Connected) {
                        this.OnConnectionLost?.Invoke(true);
                        this.m_listen = false;
                        return;
                    }
                } catch (Exception e) {
                    Trace.WriteLine(e, nameof(ServerConnection));
                    this.OnConnectionLost?.Invoke(true);
                    this.m_listen = false;
                    return;
                }
                continue;
            }

            // Get lock
            this.m_rwlock.EnterWriteLock();

            // Create list of events to invoke OUTSIDE write lock
            List<Message> eventList = new();

            // Loop over all messages
            for (int i = 0; i < messages.Count; i++) {

                // Define base message
                var baseMsg = messages[i];

                // Add
                if (this.m_receivedMessages.ContainsKey(baseMsg.Sender) && this.m_receivedMessages[baseMsg.Sender].ContainsKey(baseMsg.CID)) {
                    this.m_receivedMessages[baseMsg.Sender][baseMsg.CID] = GoMarshal.JsonUnmarshal<ContentMessage>(baseMsg.Content);
                } else {
                    eventList.Add(baseMsg);
                }

            }

            // Release lock
            this.m_rwlock.ExitWriteLock();

            // Loop over events and invoke sequentially (on new thread)
            if (eventList.Count > 0) {
                Task.Run(() => {
                    foreach (var baseMsg in eventList) {
                        this.MessageReceived?.Invoke(baseMsg.CID, baseMsg.Sender, GoMarshal.JsonUnmarshal<ContentMessage>(baseMsg.Content));
                    }
                });
            }

        }

    }

    /// <summary>
    /// Send a <see cref="Message"/> to remote endpoint.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    public void SendMessage(Message msg) {

        // Create binary representation of message
        List<byte> buffer = new(1 + 4 + 16 + 2 + msg.Content.Length);
        buffer.Add((byte)msg.Mode);
        buffer.AddRange(BitConverter.GetBytes(msg.CID));
        buffer.AddRange(BitConverter.GetBytes(msg.Target));
        buffer.AddRange(BitConverter.GetBytes(msg.Sender));
        buffer.AddRange(BitConverter.GetBytes(msg.ContentLength));
        buffer.AddRange(msg.Content);

        // Get lock
        this.m_rwlock.EnterWriteLock();

        try {

            if (this.m_receivedMessages.ContainsKey(msg.Target)) {
                this.m_receivedMessages[msg.Target][msg.CID] = null; // create entry
            } else {
                this.m_receivedMessages.Add(msg.Target, new() { [msg.CID] = null }); // add new entry to the entrie map
            }

        } catch (SocketException soc) {
            Trace.WriteLine($"SendMessage - Socket Exception: {soc.Message}", nameof(ServerConnection));
        }

        // Release lock
        this.m_rwlock.ExitWriteLock();

        // Actually send
        this.m_socket.Send(buffer.ToArray());

    }

    /// <summary>
    /// Get a reply to a message chain from a specific remote sender.
    /// </summary>
    /// <param name="cid">The message ID to listen for.</param>
    /// <param name="from">The sender to expect message from.</param>
    /// <param name="blockAwait">Set if the call should block execution until a response is received.</param>
    /// <param name="maxWait">The amount of time to wait before considering the reply not received. A null value will fall back to <see cref="NetworkInterface.TimeoutMilliseconds"/>.</param>
    /// <returns>If a reply is received or found, then the <see cref="ContentMessage"/>; Otherwise <see langword="null"/>.</returns>
    public ContentMessage? GetReply(uint cid, ulong from, bool blockAwait, TimeSpan? maxWait = null) {
        if (blockAwait) {
            var wt = maxWait is null ? TimeSpan.FromMilliseconds(NetworkInterface.TimeoutMilliseconds) : maxWait.Value;
            var now = DateTime.Now;
            while ((DateTime.Now - now).TotalMilliseconds <= wt.TotalMilliseconds) {
                this.m_rwlock.EnterReadLock();
                if (this.m_receivedMessages[from].TryGetValue(cid, out var content) && content is not null) {
                    this.m_rwlock.ExitReadLock();
                    return content;
                }
                this.m_rwlock.ExitReadLock();
                Thread.Sleep(5);
            }
        }
        this.m_rwlock.EnterReadLock();
        this.m_receivedMessages[from].TryGetValue(cid, out var msg);
        this.m_rwlock.ExitReadLock();
        return msg;
    }

    /// <summary>
    /// Send a message and await a reply to the message.
    /// </summary>
    /// <param name="msg">The message to send.</param>
    /// <param name="waitTime">The time to wait for the reply.</param>
    /// <returns>The received <see cref="ContentMessage"/> if any; Otherwise <see langword="null"/>.</returns>
    public ContentMessage? SendAndAwaitReply(Message msg, TimeSpan? waitTime = null) {
        
        // Send message
        this.SendMessage(msg);
        
        // Send
        return GetReply(msg.CID, msg.Target, true, waitTime);

    }

    /// <summary>
    /// Attempt to connect to remote server.
    /// </summary>
    /// <param name="ipaddress">The address of the remote server to connect to.</param>
    /// <param name="port">The port to try and connect on.</param>
    /// <param name="introduction">The introduction message to send to the server.</param>
    /// <param name="lobbyID">[Out] The ID of the lobby that was hosted/joined. Only valid if return value is not <see langword="null"/>.</param>
    /// <returns>If connection was established a <see cref="ServerConnection"/> instance; Otherwise <see langword="null"/>.</returns>
    public static ServerConnection? ConnectToServer(string ipaddress, int port, IntroMessage introduction, out ulong lobbyID) {

        // Set socket
        Socket? socket = null;
        lobbyID = introduction.LobbyUID;

        // Try
        try {

            // Create new socket
            socket = new(SocketType.Stream, ProtocolType.Tcp);

            // Connect to server
            socket.Connect(ipaddress, port);

            // Marshal intro message
            byte[] intro = GoMarshal.JsonMarshal(introduction);

            // Send
            socket.Send(intro);

            // Get response
            byte[] response = new byte[1024];

            // Wait for response
            int received = socket.Receive(response);
            if (received > 0) {

                // Unmarshal
                Message responseMsg = GoMarshal.BytesUnmarshal(response[..received], out _);

                // Check response
                ContentMessage content = GoMarshal.JsonUnmarshal<ContentMessage>(responseMsg.Content);

                // If not ok, err -> else we connected and joined/hosted
                if (content.MessageType != ContentMessgeType.OK) {
                    throw new Exception($"Server response type was not 'OK' but {content.MessageType} ({content.StrMsg})");
                } else {
                    if (introduction.Type == 0) {
                        lobbyID = content.Who;
                    }
                }

            } else {
                throw new Exception("Connection terminated or received no response to intro message.");
            }

        } catch (Exception ex) {
            Trace.WriteLine(ex, nameof(ServerConnection));
            return null;
        }

        // Create connection object
        ServerConnection conn = new(socket, introduction.PlayerUID);

        // Start listen thread
        conn.m_listen = true;
        conn.m_lthread.Start();

        // Register in network interface
        NetworkInterface.RegisterConnection(conn);

        // Return connection
        return conn;

    }

}
