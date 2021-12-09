using System;
using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.Memory;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;

using static Battlegrounds.Networking.LobbySystem.LobbyAPIStructs;

namespace Battlegrounds.Networking.LobbySystem {
    
    public class LobbyAPI {

        private static readonly TimeSpan __cacheTime = TimeSpan.FromSeconds(15);

        private ServerConnection m_connection;
        private bool m_isHost;
        private uint m_cidcntr;

        private ObjectCache<LobbyTeam> m_allies;
        private ObjectCache<LobbyTeam> m_axis;
        private ObjectCache<LobbyTeam> m_obs;
        private ObjectCache<Dictionary<string, string>> m_settings;

        public bool IsHost => this.m_isHost;

        public LobbyTeam Allies => this.m_allies.GetCachedValue(() => this.GetTeam(0));

        public LobbyTeam Axis => this.m_axis.GetCachedValue(() => this.GetTeam(1));

        public LobbyTeam Observers => this.m_obs.GetCachedValue(() => this.GetTeam(2));

        public Dictionary<string, string> Settings => this.m_settings.GetCachedValue(this.GetSettings);

        public ServerAPI ServerHandle { get; }

        public SteamUser Self { get; }

        public event Action<LobbyMessage> OnChatMessage;

        public event Action OnLobbySelfUpdate;

        public event Action<LobbyTeam> OnLobbyTeamUpdate;

        public event Action<int, LobbySlot> OnLobbySlotUpdate;

        public event Action<int, int, LobbyMember> OnLobbyMemberUpdate;

        public event Action<int, int, LobbyCompany> OnLobbyCompanyUpdate;

        public event Action<string, string> OnLobbySettingUpdate;

        public LobbyAPI(bool isHost, SteamUser self, ServerConnection connection, ServerAPI serverAPI) {

            // Store ref to server handle
            this.ServerHandle = serverAPI;

            // Set internal refs
            this.m_connection = connection;
            this.m_isHost = isHost;
            this.m_cidcntr = 100;

            // Store self (id)
            this.Self = self;

            // Add private hook
            this.m_connection.MessageReceived = this.OnMessage;

            // Create get lobby message
            Message msg = new Message() {
                CID = this.m_cidcntr++,
                Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = "GetLobby", Arguments = Array.Empty<string>() }),
                Mode = MessageMode.BrokerCall,
                Sender = this.m_connection.SelfID,
                Target = 0
            };

            // Make pull lobby request
            ContentMessage? reply = this.m_connection.SendAndAwaitReply(msg);
            if (reply is ContentMessage response && GoMarshal.JsonUnmarshal<LobbyRemote>(response.Raw) is LobbyRemote remoteLobby) {

                // Make sure teams are valid
                if (remoteLobby.Teams is null || remoteLobby.Teams.Any(x => x is null)) {
                    throw new Exception("Received **NULL** team data.");
                }

                // Set API on team objects
                remoteLobby.Teams.ForEach(x => x.SetAPI(this));

                // Create team data
                this.m_allies = new(remoteLobby.Teams[0], __cacheTime);
                this.m_axis = new(remoteLobby.Teams[1], __cacheTime);
                this.m_obs = new(remoteLobby.Teams[2], __cacheTime);
                this.m_settings = new(remoteLobby.Settings, __cacheTime);

            } else {

                // Throw exception -> Failed to fully connect.
                throw new Exception("Failed to retrieve light-lobby version.");

            }

        }

        private LobbyTeam GetTeamInstanceByIndex(int tid) => tid switch {
            0 => this.m_allies.Value,
            1 => this.m_axis.Value,
            2 => this.m_obs.Value,
            _ => throw new IndexOutOfRangeException($"Team ID '{tid}' is out of bounds.")
        };

        private void OnMessage(uint cid, ulong sender, ContentMessage message) {

            // Check if error message and display it
            if (message.MessageType is ContentMessgeType.Error) {
                Trace.WriteLine(message.StrMsg, nameof(LobbyAPI));
                return;
            }

            // Switch on strmsg
            switch (message.StrMsg) {
                case "Message":

                    // Unmarshal
                    LobbyMessage lobbyMessage = GoMarshal.JsonUnmarshal<LobbyMessage>(message.Raw);
                    //lobbyMessage.Timestamp = ... // TODO: Fix

                    // Trigger event
                    this.OnChatMessage?.Invoke(lobbyMessage);

                    break;
                case "Notify.Company":

                    // Get call
                    var companyCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                    break;
                case "Notify.Team":

                    // This sends the whole team object
                    var newTeam = GoMarshal.JsonUnmarshal<LobbyTeam>(message.Raw);

                    // Trigger team update
                    this.OnLobbyTeamUpdate?.Invoke(newTeam);

                    break;
                case "Notify.Slot":

                    // This sends the whole team object
                    var newSlot = GoMarshal.JsonUnmarshal<LobbySlot>(message.Raw);

                    // Trigger team update
                    this.OnLobbySlotUpdate?.Invoke((int)message.Who, newSlot);

                    break;
                case "Notify.Member":

                    break;
                case "Notify.Setting":

                    // Get call
                    var settingCall = GoMarshal.JsonUnmarshal<RemoteCallMessage>(message.Raw);

                    // Decode
                    var (settingKey, settingValue) = settingCall.Decode<string, string>();

                    // Notify
                    this.OnLobbySettingUpdate?.Invoke(settingKey, settingValue);

                    break;
                default:
                    Trace.WriteLine($"Unsupported API event: {message.StrMsg}", nameof(LobbyAPI));
                    break;
            }

        }

        public void Disconnect() {
            this.m_connection.Shutdown();
        }

        public LobbyTeam GetTeam(int tid)
            => RemoteCall<LobbyTeam>("GetTeam", tid); // TODO: Trigger team refresh invoked

        public LobbyMember GetLobbyMember(ulong mid)
            => RemoteCall<LobbyMember>("GetLobbyMember", mid);

        public LobbyCompany GetCompany(ulong mid)
            => RemoteCall<LobbyCompany>("GetCompany", mid);

        public Dictionary<string, string> GetSettings()
            => RemoteCall<Dictionary<string, string>>("GetSettings");

        public uint GetPlayerCount()
            => RemoteCall<uint>("GetPlayerCount");

        private static string EncBool(bool b) => b ? "1" : "0";

        public void SetCompany(int tid, int sid, LobbyCompany company) {

            // Invoke remotely
            this.RemoteVoidCall("SetCompany", tid, sid, EncBool(company.IsAuto), EncBool(company.IsNone), company.Name, company.Army, company.Strength, company.Specialisation);

            // Trigger self update
            this.OnLobbyCompanyUpdate?.Invoke(tid, sid, company);

        }

        public void MoveSlot(ulong mid, int tid, int sid)
            => RemoteVoidCall("MoveSlot", mid, tid, sid);

        public void AddAI(int tid, int sid, int difficulty, LobbyCompany company) {
        
            // Make sure we can
            if (!this.m_isHost) {
                throw new InvokePermissionAccessDeniedException("Cannot invoke remote method that requires host-privellige");
            }

            // Call AI
            RemoteVoidCall("AddAI", tid, sid, difficulty, EncBool(company.IsAuto), EncBool(company.IsNone), company.Name, company.Army, company.Strength, company.Specialisation);

            // Update team
            var t = tid == 0 ? this.m_allies : this.m_axis;
            t.Value.Slots[sid].Occupant = new() {
                AILevel = difficulty,
                Company = company,
                Role = 3,
                API = this
            };
            t.Value.Slots[sid].State = 1;

            // Trigger self update
            this.OnLobbySelfUpdate?.Invoke();

        }

        public void RemoveOccupant(int tid, int sid) {
            
            // Trigger remote call
            RemoteVoidCall("RemoveOccupant", tid, sid);

            // Clear slot
            var t = tid == 0 ? this.m_allies : this.m_axis;
            t.Value.Slots[sid].Occupant = null;
            t.Value.Slots[sid].State = 0;

            // Update self
            this.OnLobbySelfUpdate?.Invoke();

        }

        public void LockSlot(int tid, int sid)
            => RemoteVoidCall("LockSlot", tid, sid);

        public void UnlockSlot(int tid, int sid)
            => RemoteVoidCall("UnlockSlot", tid, sid);

        public void GlobalChat(ulong mid, string msg)
            => RemoteVoidCall("GlobalChat", mid, msg);

        public void TeamChat(ulong mid, string msg)
            => RemoteVoidCall("TeamChat", mid, msg);

        public void SetLobbySetting(string setting, string value) {
            if (this.m_isHost) {
                RemoteVoidCall("SetLobbySetting", setting, value);
            }
        }

        public bool SetTeamsCapacity(int newCapacity) {
            if (this.m_isHost) {                
                return this.RemoteCall<bool>("SetTeamsCapacity", newCapacity);
            }
            return true;
        }

        public bool StartMatch(double cancelTime) {
            return false;
        }

        public void LaunchMatch() {
            if (this.m_isHost) {
                this.RemoteVoidCall("LaunchGame");
            }
        }

        public void RequestCompanyFiles(params ulong[] members) {

        }


        public void ReleaseGamemode() {
            if (this.m_isHost) {
                this.RemoteVoidCall("ReleaseGamemode");
            }
        }

        public void ReleaseResults() {
            if (this.m_isHost) {
                this.RemoteVoidCall("ReleaseResults");
            }
        }

        private T RemoteCall<T>(string method, params object[] args) {
            
            // Create message
            Message msg = new Message() {
                CID = this.m_cidcntr++,
                Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = args.Map(x => x.ToString()) }),
                Mode = MessageMode.BrokerCall,
                Sender = this.m_connection.SelfID,
                Target = 0
            };

            // Send and await response
            if (this.m_connection.SendAndAwaitReply(msg) is ContentMessage response) {
                if (response.MessageType == ContentMessgeType.Error) {
                    throw new Exception(response.StrMsg);
                }
                if (response.StrMsg == "Primitive") {
                    // It feels nasty using 'dynamic'
                    return (T)(dynamic)(response.DotNetType switch {
                        nameof(Boolean) => response.Raw[0] == 1,
                        nameof(UInt32) => BitConverter.ToUInt32(response.Raw),
                        _ => throw new NotImplementedException($"Support for primitive response of type '{response.DotNetType}' not implemented.")
                    });
                } else {
                    if (GoMarshal.JsonUnmarshal<T>(response.Raw) is T settings) {
                        if (settings is IAPIObject apiobj) {
                            apiobj.SetAPI(this);
                        }
                        return settings;
                    }
                }
            }
            
            // TODO: Warning
            return default;

        }

        private void RemoteVoidCall(string method, params object[] args) {
            
            // Create message
            Message msg = new Message() {
                CID = this.m_cidcntr++,
                Content = GoMarshal.JsonMarshal(new RemoteCallMessage() { Method = method, Arguments = args.Map(x => x.ToString()) }),
                Mode = MessageMode.BrokerCall,
                Sender = this.m_connection.SelfID,
                Target = 0
            };
            
            // Send but ignore response
            this.m_connection.SendMessage(msg);

        }

    }

}
