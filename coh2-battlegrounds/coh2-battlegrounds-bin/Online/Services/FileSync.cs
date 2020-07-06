using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileSync {
    
        private class FileSyncUserState {
            public string userID;
            public bool userDone;
        }

        public class SyncFile {
            public byte[] Data;
            public string Name;
            public string From;
        }

        List<SyncFile> m_syncedFiledata;
        List<FileSyncUserState> m_syncStates;
        ManagedLobby m_lobbySync;
        bool m_isDone;
        bool m_syncFailed;

        /// <summary>
        /// 
        /// </summary>
        public bool IsSynced => this.m_isDone;

        /// <summary>
        /// 
        /// </summary>
        public bool SyncFailed => this.m_syncFailed;

        /// <summary>
        /// The maximum amount of minutes to wait before declaring failure to sync.
        /// </summary>
        public int SyncMaxTimeoutMinutes { get; set; } = 5;

        /// <summary>
        /// The time in milliseconds before a re-send operation can be considered.
        /// </summary>
        public int SyncResendAttempt { get; set; } = 75000;

        /// <summary>
        /// 
        /// </summary>
        public SyncFile[] SyncedFiles => this.m_syncedFiledata?.ToArray();

        public FileSync(ManagedLobby lobby, string fileToSync) {

            this.m_isDone = false;
            this.m_syncFailed = false;
            this.m_lobbySync = lobby;

            this.SetupUsers();
            this.SyncSend(fileToSync);

        }

        public FileSync(ManagedLobby lobby, Message getterMessage) {

            this.m_isDone = false;
            this.m_syncFailed = false;
            this.m_lobbySync = lobby;
            this.m_syncedFiledata = new List<SyncFile>();
            this.SetupUsers();

            this.SyncGet(getterMessage);

        }

        private void SetupUsers() {

            string[] playernames = this.m_lobbySync.GetPlayerNames();
            m_syncStates = new List<FileSyncUserState>();

            for (int i = 0; i < m_syncStates.Count; i++) {
                m_syncStates.Add(new FileSyncUserState {
                    userID = playernames[i],
                    userDone = false
                });
            }

        }

        private void SyncGet(Message getterMessage) {

            DateTime start = DateTime.Now;

            void response(Message x) {
                if (x.Descriptor == Message_Type.LOBBY_SENDFILE) {
                    int i = this.m_syncStates.FindIndex(y => y.userID.CompareTo(x.Argument2) == 0);
                    if (i >= 0) {
                        this.m_syncStates[i].userDone = true;
                        this.m_syncedFiledata.Add(new SyncFile() {
                            Data = x.FileData,
                            From = x.Argument2,
                            Name = x.Argument1,
                        });
                    }
                } else {
                    Console.WriteLine("Wut");
                }
            }

            Message.SetIdentifier(this.m_lobbySync.WorkerConnection.ConnectionSocket, getterMessage);
            int lookForIdentifier = getterMessage.Identifier;
            this.m_lobbySync.WorkerConnection.SetIdentifierReceiver(lookForIdentifier, response);
            this.m_lobbySync.WorkerConnection.SendMessage(getterMessage);

            while (!this.m_isDone) {
                Console.WriteLine("Attempting to sync...");
                int waitMinutes = (DateTime.Now - start).Minutes;
                if (waitMinutes >= this.SyncMaxTimeoutMinutes) {
                    this.m_isDone = this.m_syncFailed = true;
                    break;
                } else {

                    int receivedACK = 0;
                    lock (this.m_syncStates) {
                        receivedACK = this.m_syncStates.Count(x => x.userDone);
                    }
                    Console.WriteLine($"Files synced [{receivedACK}/{this.m_syncStates.Count}]");
                    if (receivedACK != this.m_syncStates.Count) {
                        Thread.Sleep(this.SyncResendAttempt);
                        lock (this.m_syncStates) {
                            for (int i = 0; i < this.m_syncStates.Count; i++) {
                                if (!this.m_syncStates[i].userDone) { // Resend to all those who havent returned the file.
                                    this.m_lobbySync.WorkerConnection.SendMessage(new Message(getterMessage.Descriptor, this.m_syncStates[i].userID) { Identifier = lookForIdentifier });
                                }
                            }
                        }
                    } else {
                        this.m_isDone = true;
                        break;
                    }

                }

            }

            this.m_lobbySync.WorkerConnection.ClearIdentifierReceiver(lookForIdentifier);
            Console.WriteLine($"Files synced [{this.m_syncStates.Count}/{this.m_syncStates.Count}]");

        }

        private void SyncSend(string file) {

            DateTime start = DateTime.Now;

            void response(Message x) {
                if (x.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                    int i = this.m_syncStates.FindIndex(y => y.userID.CompareTo(x.Argument1) == 0);
                    if (i >= 0) {
                        this.m_syncStates[i].userDone = true;
                    }
                }
            }

            int lookForIdentifier = this.m_lobbySync.SendFile(ManagedLobby.SEND_ALL, file);
            this.m_lobbySync.WorkerConnection.SetIdentifierReceiver(lookForIdentifier, response);

            while (!this.m_isDone) {

                int waitMinutes = (DateTime.Now - start).Minutes;
                if (waitMinutes >= this.SyncMaxTimeoutMinutes) {
                    this.m_isDone = this.m_syncFailed = true;
                    break;
                } else {

                    int receivedACK = 0;
                    lock (this.m_syncStates) {
                        receivedACK = this.m_syncStates.Count(x => x.userDone);
                    }
                    if (receivedACK != this.m_syncStates.Count) {
                        Thread.Sleep(this.SyncResendAttempt);
                        lock (this.m_syncStates) {
                            for (int i = 0; i < this.m_syncStates.Count; i++) {
                                if (!this.m_syncStates[i].userDone) { // Resend to all those who havent received the message.
                                    this.m_lobbySync.SendFile(this.m_syncStates[i].userID, file, lookForIdentifier);
                                }
                            }
                        }
                    } else {
                        this.m_isDone = true;
                        break;
                    }

                }

            }

            this.m_lobbySync.WorkerConnection.ClearIdentifierReceiver(lookForIdentifier);

        }

    }

}
