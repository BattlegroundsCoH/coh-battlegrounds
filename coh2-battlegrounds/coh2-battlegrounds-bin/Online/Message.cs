using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Battlegrounds.Online {

    public enum Message_Type : byte {

        ERROR_MESSAGE, // Error message

        INFORMATION_MESSAGE, // Information message

        CONFIRMATION_MESSAGE, // Confirms

        ORDINARY_RESPONSE, // Ordinary response to a simple query (Identifier must exist for response message to work)

        GET_LOBBIES, // Get the list of lobbies

        GET_LOBBIES_RETURN, // This is a returned lobby

        LOBBY_CREATE, // Create lobby

        LOBBY_REMOVE, // Remove lobby

        LOBBY_JOIN, // Join lobby

        LOBBY_LEAVE, // Leave lobby

        LOBBY_KICK, // User was kicked (To players in lobby: *player* was kicked)

        LOBBY_KICKED, // The user was kicked (Sent to the player who was kicked - to let them know they were kicked)

        LOBBY_INFO, // Get lobby info

        LOBBY_UPDATE, // Update lobby info

        LOBBY_CHATMESSAGE, // Send chatmessage

        LOBBY_METAMESSAGE, // Send metamessage

        LOBBY_REQUEST_COMPANY,

        LOBBY_REQUEST_RESULTS,

        LOBBY_STARTMATCH, // Tell clients to start match

        LOBBY_SYNCMATCH, // Tell clients to sync match

        LOBBY_STATE,

        LOBBY_GETSTATE,

        LOBBY_PLAYERNAMES,

        LOBBY_PLAYERIDS,

        LOBBY_GETPLAYERID,

        LOBBY_SETHOST, // Message sent to client that they're now the host.

        LOBBY_STARTING, // Host has pressed the start button

        LOBBY_CANCEL, // Tell the lobby to cancel match start

        LOBBY_NOTIFY_GAMEMODE, // Notify lobby members the gamemode is available

        USER_SETUSERDATA, // Set the user data

        USER_PING, // User ping-back (SERVER_PING -> reponse = USER_PING)

        SERVER_PING, // Ping from server (Server -> Client)

        SERVER_CLOSE, // Close the server

    }

    public sealed class Message {

        public const string MESSAGE_INVALID_REQUEST = "Invalid Request";

        public int Identifier;

        public Message_Type Descriptor;

        public string Argument1;
        public string Argument2;
        public string Argument3;

        public Message() {
            this.Descriptor = Message_Type.ERROR_MESSAGE;
            this.Identifier = -1;
            this.Argument1 = string.Empty;
            this.Argument2 = string.Empty;
            this.Argument3 = string.Empty;
        }

        public Message(Message_Type type) {
            this.Descriptor = type;
            this.Identifier = -1;
            this.Argument1 = string.Empty;
            this.Argument2 = string.Empty;
            this.Argument3 = string.Empty;
        }

        public Message(Message_Type type, string arg1 = "", string arg2 = "", string arg3 = "") {
            this.Descriptor = type;
            this.Identifier = -1;
            this.Argument1 = arg1;
            this.Argument2 = arg2;
            this.Argument3 = arg3;
        }

        public Message CreateResponse(Message_Type type, string arg1 = "", string arg2 = "", string arg3 = "") {
            Message message = new Message(type, arg1, arg2, arg3) {
                Identifier = this.Identifier
            };
            return message;
        }

        public byte[] ToBytes() {

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter binaryWriter = new BinaryWriter(ms)) {

                byte[] arg1 = Encoding.UTF8.GetBytes(this.Argument1);
                byte[] arg2 = Encoding.UTF8.GetBytes(this.Argument2);
                byte[] arg3 = Encoding.UTF8.GetBytes(this.Argument3);

                ushort len1 = (ushort)arg1.Length;
                ushort len2 = (ushort)arg2.Length;
                ushort len3 = (ushort)arg3.Length;

                int ushortSize = (len1 == 0) ? 0 : sizeof(ushort) + ((len2 == 0) ? 0 : sizeof(ushort) + ((len3 == 0) ? 0 : sizeof(ushort)));
                ushort totalSize = (ushort)(len1 + len2 + len3 + ushortSize + (sizeof(ushort) + sizeof(int) + (sizeof(byte) * 3)));

                binaryWriter.Write((ushort)totalSize);
                binaryWriter.Write((byte)this.Descriptor);
                binaryWriter.Write((int)this.Identifier);

                if (len1 > 0) {
                    binaryWriter.Write((ushort)len1);
                    binaryWriter.Write(arg1);
                    if (len2 > 0) {
                        binaryWriter.Write((ushort)len2);
                        binaryWriter.Write(arg2);
                        if (len3 > 0) {
                            binaryWriter.Write((ushort)len3);
                            binaryWriter.Write(arg3);
                        }
                    }
                }

                // Write end bytes
                binaryWriter.Write((byte)'\x04');
                binaryWriter.Write((byte)'\x06');

            }

            return ms.ToArray();

        }

        public static List<Message> GetMessages(byte[] bytes) {

            List<Message> messages = new List<Message>();

            try {

                using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));

                while (reader.BaseStream.Position < reader.BaseStream.Length) {

                    Message m = new Message();

                    ushort size = reader.ReadUInt16();

                    try {

                        using BinaryReader subReader = new BinaryReader(new MemoryStream(reader.ReadBytes(size)));

                        m.Descriptor = (Message_Type)subReader.ReadByte();
                        m.Identifier = subReader.ReadInt32();

                        int p = subReader.PeekChar();
                        if (p != -1 && subReader.BaseStream.Position + 2 < size) {

                            ushort l1 = subReader.ReadUInt16();
                            m.Argument1 = Encoding.ASCII.GetString(subReader.ReadBytes(l1));

                            p = subReader.PeekChar();
                            if (p != -1 && subReader.BaseStream.Position + 2 < size) {

                                ushort l2 = subReader.ReadUInt16();
                                m.Argument2 = Encoding.ASCII.GetString(subReader.ReadBytes(l2));

                                p = subReader.PeekChar();
                                if (p != -1 && subReader.BaseStream.Position + 2 < size) {

                                    ushort l3 = subReader.ReadUInt16();
                                    m.Argument3 = Encoding.ASCII.GetString(subReader.ReadBytes(l3));

                                }
                            }
                        }

                    } catch (Exception e) {
                        m.Descriptor = Message_Type.ERROR_MESSAGE;
                        m.Argument1 = $"Failed to read message of length {size} - {e.Message}";
                    }

                    messages.Add(m);

                }

            } catch (Exception e) {
                Console.WriteLine($"[Server.IO] {e.Message}");
            }

            return messages;

        }

        public static void SetIdentifier(Socket socket, Message message) {
            if (message.Identifier != -1) { // ignore if already assigned
                return;
            }
            try {
                message.Identifier = GetIdentifier(socket);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        internal static int GetIdentifier(Socket socket)
            => Encoding.ASCII.GetBytes(
                (socket.RemoteEndPoint as IPEndPoint).Address.ToString() +
                DateTime.UtcNow.TimeOfDay.ToString() +
                Guid.NewGuid().ToString()).Aggregate(0, (a, b) => a += b);

        public override string ToString() => $"{this.Descriptor}: \"{this.Argument1}\" : \"{this.Argument2}\" : \"{this.Argument3}\"";

    }

    public static class MessageListExtension {
        public static void Invoke(this List<Message> messages, Action<Message> action) => messages.ForEach(x => action(x));
    }

}
