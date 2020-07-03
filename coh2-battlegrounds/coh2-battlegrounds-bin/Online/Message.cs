using System;
using System.IO;
using System.Text;

namespace Battlegrounds.Online {

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

        LOBBY_KICKED, // The user was kicked (self was kicked)

        LOBBY_INFO, // Get lobby info

        LOBBY_UPDATE, // Update lobby info

        LOBBY_CHATMESSAGE, // Send chatmessage

        LOBBY_METAMESSAGE, // Send metamessage

        LOBBY_UPLOAD, // Upload file in lobby

        LOBBY_DOWNLOAD, // Download file in lobby

        LOBBY_STARTMATCH, // Tell clients to start match

        LOBBY_SYNCMATCH, // Tell clients to sync match

        LOBBY_STATE,

        LOBBY_GETSTATE,

        LOBBY_SETHOST,

        USER_SETUSERDATA, // Set the user data

        SERVER_CLOSE, // Close the server

    }

    public sealed class Message {

        public const string MESSAGE_INVALID_REQUEST = "Invalid Request";

        public Message_Type Descriptor;

        public string Argument1;
        public string Argument2;

        public byte[] FileData;

        public Message() {
            this.Descriptor = Message_Type.ERROR_MESSAGE;
            this.Argument1 = string.Empty;
            this.Argument2 = string.Empty;
        }

        public Message(Message_Type type) {
            this.Descriptor = type;
            this.Argument1 = string.Empty;
            this.Argument2 = string.Empty;
        }

        public Message(Message_Type type, string arg1) {
            this.Descriptor = type;
            this.Argument1 = arg1;
            this.Argument2 = string.Empty;
        }

        public Message(Message_Type type, string arg1, string arg2) {
            this.Descriptor = type;
            this.Argument1 = arg1;
            this.Argument2 = arg2;
        }

        public void EncodeStringAsFile(string content)
            => this.FileData = Encoding.ASCII.GetBytes(content);

        public byte[] ToBytes() {

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter binaryWriter = new BinaryWriter(ms)) {

                binaryWriter.Write((byte)this.Descriptor);

                byte[] arg1 = Encoding.ASCII.GetBytes(this.Argument1);
                int len1 = arg1.Length;
                binaryWriter.Write((Int32)len1);
                binaryWriter.Write(arg1);

                byte[] arg2 = Encoding.ASCII.GetBytes(this.Argument2);
                int len2 = arg2.Length;
                binaryWriter.Write((Int32)len2);
                binaryWriter.Write(arg2);

                binaryWriter.Write(FileData != null);

                if (FileData != null) {
                    binaryWriter.Write((Int32)FileData.Length);
                    binaryWriter.Write(FileData);
                }

                // Write end bytes
                binaryWriter.Write((byte)'\x04');
                binaryWriter.Write((byte)'\x06');

            }

            return ms.ToArray();

        }

        public static Message GetMessage(byte[] bytes) {

            Message m = new Message();

            try {
                using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
                m.Descriptor = (Message_Type)reader.ReadByte();
                m.Argument1 = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
                m.Argument2 = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
                if (reader.ReadBoolean()) {
                    m.FileData = reader.ReadBytes(reader.ReadInt32());
                }
            } catch {
                m.Descriptor = Message_Type.ERROR_MESSAGE;
                m.Argument1 = "Failed to read message";
            }

            return m;

        }

    }

}
