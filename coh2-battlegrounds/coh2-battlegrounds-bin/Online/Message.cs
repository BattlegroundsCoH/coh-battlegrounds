using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * This file was copied directly from the serverside.
 * Do not modify this code.
 * - CoDiEx
 */

namespace Battlegrounds.Online {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public enum Message_Type : byte {

        ERROR_MESSAGE, // Error message

        INFORMATION_MESSAGE, // Information message

        CONFIRMATION_MESSAGE, // Confirms

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

        LOBBY_SENDFILE, // Send file

        LOBBY_REQUEST_COMPANY,

        LOBBY_REQUEST_RESULTS,

        LOBBY_STARTMATCH, // Tell clients to start match

        LOBBY_SYNCMATCH, // Tell clients to sync match

        LOBBY_STATE,

        LOBBY_GETSTATE,

        LOBBY_PLAYERNAMES,

        LOBBY_SETHOST, // Message sent to client that they're now the host.

        USER_SETUSERDATA, // Set the user data

        SERVER_CLOSE, // Close the server

    }

    public sealed class Message {

        public const string MESSAGE_INVALID_REQUEST = "Invalid Request";

        public int Identifier;

        public Message_Type Descriptor;

        public string Argument1;
        public string Argument2;

        public byte[] FileData;

        public Message() {
            this.Descriptor = Message_Type.ERROR_MESSAGE;
            this.Identifier = -1;
            this.Argument1 = string.Empty;
            this.Argument2 = string.Empty;
        }

        public Message(Message_Type type) {
            this.Descriptor = type;
            this.Identifier = -1;
            this.Argument1 = string.Empty;
            this.Argument2 = string.Empty;
        }

        public Message(Message_Type type, string arg1) {
            this.Descriptor = type;
            this.Identifier = -1;
            this.Argument1 = arg1;
            this.Argument2 = string.Empty;
        }

        public Message(Message_Type type, string arg1, string arg2) {
            this.Descriptor = type;
            this.Identifier = -1;
            this.Argument1 = arg1;
            this.Argument2 = arg2;
        }

        public Message CreateResponse(Message_Type type, string arg1 = "", string arg2 = "") {
            Message message = new Message(type, arg1, arg2) {
                Identifier = this.Identifier
            };
            return message;
        }

        public void EncodeStringAsFile(string content)
            => this.FileData = Encoding.ASCII.GetBytes(content);

        public byte[] ToBytes() {

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter binaryWriter = new BinaryWriter(ms)) {

                byte[] arg1 = Encoding.ASCII.GetBytes(this.Argument1);
                byte[] arg2 = Encoding.ASCII.GetBytes(this.Argument2);

                int len1 = arg1.Length;
                int len2 = arg2.Length;
                int len3 = (FileData == null) ? 0 : FileData.Length + sizeof(UInt16);

                ushort totalSize = (ushort)(len1 + len2 + len3 + (sizeof(Int32) + (sizeof(byte) * 5) + sizeof(bool)));

                binaryWriter.Write((UInt16)totalSize);
                binaryWriter.Write((byte)this.Descriptor);
                binaryWriter.Write((Int32)this.Identifier);

                binaryWriter.Write((byte)len1);
                binaryWriter.Write(arg1);

                binaryWriter.Write((byte)len2);
                binaryWriter.Write(arg2);

                if (FileData != null && FileData.Length == 0) {
                    FileData = null;
                }

                binaryWriter.Write(FileData != null);

                if (FileData != null) {
                    binaryWriter.Write((UInt16)FileData.Length);
                    binaryWriter.Write(FileData);
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

                        byte l1 = subReader.ReadByte();
                        m.Argument1 = Encoding.ASCII.GetString(subReader.ReadBytes(l1));

                        byte l2 = subReader.ReadByte();
                        m.Argument2 = Encoding.ASCII.GetString(subReader.ReadBytes(l2));
                        if (subReader.ReadBoolean()) {
                            m.FileData = subReader.ReadBytes(subReader.ReadUInt16());
                        }

                    } catch (Exception e) {
                        m.Descriptor = Message_Type.ERROR_MESSAGE;
                        m.Argument1 = $"Failed to read message of length {size} - {e.Message}";
                    }

                    messages.Add(m);

                }

            } catch {

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
                (DateTime.UtcNow.TimeOfDay + DateTime.Now.TimeOfDay).ToString() + 
                Guid.NewGuid().ToString()).Aggregate(0, (a, b) => a += b);

        public override string ToString() => $"{this.Descriptor}: \"{this.Argument1}\" : \"{this.Argument2}\"";

    }

    public static class MessageListExtension {
        public static void Invoke(this List<Message> messages, Action<Message> action) => messages.ForEach(x => action(x));
    }

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
