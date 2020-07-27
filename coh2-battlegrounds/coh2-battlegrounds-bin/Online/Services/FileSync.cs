using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Utility class for synchronizing files between a <see cref="ManagedLobby"/> by resending requests until all players have reported positively back. This clss cannot be inherited.
    /// </summary>
    public sealed class FileSync {
    
        private class FileSyncUserState {
            public string userID;
            public bool userDone;
        }

        /// <summary>
        /// Synchronized file.
        /// </summary>
        public class SyncFile {
            public byte[] Data;
            public string Name;
            public string From;
        }

        List<SyncFile> m_syncedFiledata;
        List<FileSyncUserState> m_syncStates;
        Func<Task<bool>> m_action;
        ManagedLobby m_lobbySync;
        bool m_isDone;
        bool m_syncFailed;

        /// <summary>
        /// Are all files synced.
        /// </summary>
        public bool IsSynced => this.m_isDone;

        /// <summary>
        /// This the sync fail (one or more lobby members failed to send or retrieve files after the max sync time expired.
        /// </summary>
        public bool SyncFailed => this.m_syncFailed;

        /// <summary>
        /// The maximum amount of minutes to wait before declaring failure to sync.
        /// </summary>
        public int SyncMaxTimeoutMinutes { get; set; } = 2;

        /// <summary>
        /// The time in milliseconds before a re-send operation can be considered.
        /// </summary>
        public int SyncResendAttempt { get; set; } = 7500;

        /// <summary>
        /// All the files that were synced.
        /// </summary>
        public SyncFile[] SyncedFiles => this.m_syncedFiledata?.ToArray();

        /// <summary>
        /// Send a file to all <see cref="ManagedLobby"/> members.
        /// </summary>
        /// <param name="lobby">The lobby instance to sync.</param>
        /// <param name="fileToSync">The path of the file to sync.</param>
        public FileSync(ManagedLobby lobby, string fileToSync) {

            this.m_isDone = false;
            this.m_syncFailed = false;
            this.m_lobbySync = lobby;

            this.m_action = async () => {
                await this.SetupUsers();
                await this.SyncSend(fileToSync);
                return true;
            };

        }

        /// <summary>
        /// Retrieve a file from all members of a <see cref="ManagedLobby"/> (caller excepted).
        /// </summary>
        /// <param name="lobby">The lobby instance to sync</param>
        /// <param name="getterMessage">The message to send to retrieve file.</param>
        public FileSync(ManagedLobby lobby, Message getterMessage) {

            this.m_isDone = false;
            this.m_syncFailed = false;
            this.m_lobbySync = lobby;
            this.m_syncedFiledata = new List<SyncFile>();

            this.m_action = async () => {
                await this.SetupUsers();
                await this.SyncGet(getterMessage);
                return true;
            };

        }

        public async Task<bool> Sync() {
            bool result = await this.m_action.Invoke();
            return result; // TODO: Timeout stuff
        }

        private async Task<bool> SetupUsers() {

            string[] playernames = await this.m_lobbySync.GetPlayerNamesAsync();
            m_syncStates = new List<FileSyncUserState>();

            for (int i = 0; i < playernames.Length; i++) {
                m_syncStates.Add(new FileSyncUserState {
                    userID = playernames[i],
                    userDone = false
                });
            }

            return true;

        }

        private async Task<bool> SyncGet(Message getterMessage) {

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
                }
            }

            Message.SetIdentifier(this.m_lobbySync.WorkerConnection.ConnectionSocket, getterMessage);
            int lookForIdentifier = getterMessage.Identifier;
            this.m_lobbySync.WorkerConnection.SetIdentifierReceiver(lookForIdentifier, response);
            this.m_lobbySync.WorkerConnection.SendMessage(getterMessage);

            while (!this.m_isDone) {
                Trace.WriteLine("Attempting to sync...");
                int waitMinutes = (DateTime.Now - start).Minutes;
                if (waitMinutes >= this.SyncMaxTimeoutMinutes) {
                    this.m_isDone = this.m_syncFailed = true;
                    break;
                } else {

                    int receivedACK = 0;
                    lock (this.m_syncStates) {
                        receivedACK = this.m_syncStates.Count(x => x.userDone);
                    }
                    Trace.WriteLine($"Files synced [{receivedACK}/{this.m_syncStates.Count}]");
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
                await Task.Delay(1);
            }

            this.m_lobbySync.WorkerConnection.ClearIdentifierReceiver(lookForIdentifier);

            return true;

        }

        private async Task<bool> SyncSend(string file) {

            DateTime start = DateTime.Now;

            void response(Message x) {
                if (x.Descriptor == Message_Type.CONFIRMATION_MESSAGE) {
                    int i = this.m_syncStates.FindIndex(y => y.userID.CompareTo(x.Argument1) == 0);
                    if (i >= 0) {
                        this.m_syncStates[i].userDone = true;
                    }
                }
            }

            int lookForIdentifier = this.m_lobbySync.SendFile(ManagedLobby.SEND_ALL, file, true);
            this.m_lobbySync.WorkerConnection.SetIdentifierReceiver(lookForIdentifier, response);

            while (!this.m_isDone) {
                Trace.WriteLine("Attempting to sync...");
                int waitMinutes = (DateTime.Now - start).Minutes;
                if (waitMinutes >= this.SyncMaxTimeoutMinutes) {
                    this.m_isDone = this.m_syncFailed = true;
                    break;
                } else {

                    int receivedACK = 0;
                    lock (this.m_syncStates) {
                        receivedACK = this.m_syncStates.Count(x => x.userDone);
                    }
                    Trace.WriteLine($"Files synced [{receivedACK}/{this.m_syncStates.Count}]");
                    if (receivedACK != this.m_syncStates.Count) {
                        Thread.Sleep(this.SyncResendAttempt);
                        lock (this.m_syncStates) {
                            for (int i = 0; i < this.m_syncStates.Count; i++) {
                                if (!this.m_syncStates[i].userDone) { // Resend to all those who havent received the message.
                                    this.m_lobbySync.SendFile(this.m_syncStates[i].userID, file, lookForIdentifier, true);
                                }
                            }
                        }
                    } else {
                        this.m_isDone = true;
                        break;
                    }

                }
                await Task.Delay(1);
            }

            this.m_lobbySync.WorkerConnection.ClearIdentifierReceiver(lookForIdentifier);

            return true;

        }

    }

}
