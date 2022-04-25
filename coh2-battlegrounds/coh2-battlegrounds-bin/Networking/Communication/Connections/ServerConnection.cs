using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.Memory;

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
    public ulong SelfID { get; }

    private ServerConnection(Socket socket, ulong selfId) {
        this.m_socket = socket;
        this.m_receivedMessages = new() {
            [0] = new() // broker
        };
        this.m_lthread = new(this.Listen);
        this.m_rwlock = new();
        this.SelfID = selfId;
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

        try {

            // Define big buffer list
            List<byte> bigBuffer = new();

            // Exhaust incoming data buffer
            int received = this.m_socket.ReceiveBufferSize;
            while (received == this.m_socket.ReceiveBufferSize) {

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

            // Create list 
            List<Message> messages = new List<Message>();

            // Start interpreting messages
            byte[] slice = bigBuffer.ToArray();
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
                this.OnConnectionLost?.Invoke(true);
                this.m_listen = false;
                return;
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
            Task.Run(() => {
                foreach (var baseMsg in eventList) {
                    this.MessageReceived?.Invoke(baseMsg.CID, baseMsg.Sender, GoMarshal.JsonUnmarshal<ContentMessage>(baseMsg.Content));
                }
            });

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

        if (this.m_receivedMessages.ContainsKey(msg.Target)) {
            this.m_receivedMessages[msg.Target][msg.CID] = null; // create entry
        } else {
            this.m_receivedMessages.Add(msg.Target, new() { [msg.CID] = null }); // add new entry to the entrie map
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
    /// Sends the file contents to the server. The file being sent is based on a byte ID and both endpoints must agree on this endpoint.
    /// </summary>
    /// <remarks>
    /// A call to this function will block thread execution until file is uploaded or server has aborted. Invoke inside a thread for
    /// non-blocking behaviour and use <paramref name="progressHandler"/> to detect upload completion.
    /// </remarks>
    /// <param name="contents">The file contents to upload.</param>
    /// <param name="filetyp">The unique ID of the file type being sent.</param>
    /// <param name="cid">The unique ID to use while communicating with the server. Used to get a response to a request.</param>
    /// <param name="sender">The ID of the sender. Use to identify different users of the same <paramref name="filetyp"/>.</param>
    /// <param name="progressHandler">Callback handler that reports on progress.</param>
    /// <param name="chunkSize"></param>
    /// <returns>If file was uploaded, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool SendFile(byte[] contents, byte filetyp, uint cid, UploadProgressCallbackHandler? progressHandler = null, ulong sender = 0, uint chunkSize = 1024) {

        // Calculate how many chunks to send
        int chunks = (int)Math.Ceiling(contents.Length / (double)chunkSize);

        // Set p,q
        int p = 0;
        int q = Math.Min(contents.Length, (int)chunkSize);

        // Do initial progress call
        progressHandler?.Invoke(0, chunks, false);

        // Send chunks
        for (int i = 0; i < chunks; i++) {

            // Grab chunk
            var chunk = contents.Slice(p, q);

            // Send
            try {

                // Get flags
                var state = i switch {
                    0 => UploadState.Init,
                    _ => i == chunks - 1 ? UploadState.Terminate : UploadState.Chunk
                };

                Trace.WriteLine($"SendFile: {p}, {q}, {chunks} {i} {state}", nameof(ServerConnection));

                // Construct upload message
                var umsg = new UploadCallMessage(filetyp, state, chunk);
                var msg = new Message {
                    CID = cid,
                    Mode = MessageMode.FileUpload,
                    Sender = this.SelfID,
                    Target = sender,
                    Content = GoMarshal.JsonMarshal(umsg)
                };

                // Send reply (TODO: Add server side content message response)
                this.SendAndAwaitReply(msg);
                
                // Update event
                progressHandler?.Invoke(i + 1, chunks, false);

            } catch (Exception e) {
                
                // Log exception
                Trace.WriteLine(e, nameof(ServerConnection));
                
                // Invoke callback with isCancelled flag set to true
                progressHandler?.Invoke(i, chunks, true);
                return false;

            }

            // Update offsets
            p += chunk.Length;
            q = (int)Math.Min(contents.Length, p + chunkSize);

        }

        // Return true -> AOK
        return true;

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
                    if (introduction.Host) {
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
