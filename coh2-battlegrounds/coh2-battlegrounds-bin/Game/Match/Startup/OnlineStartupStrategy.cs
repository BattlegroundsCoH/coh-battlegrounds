using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Battlegrounds.Compiler;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Online;
using Battlegrounds.Online.Lobby;
using Battlegrounds.Online.Services;

namespace Battlegrounds.Game.Match.Startup {

    /// <summary>
    /// Startup Strategy specifically for games with more than one human player. Can be extended with custom behaviour.
    /// </summary>
    public class OnlineStartupStrategy : BaseStartupStrategy {

        /// <summary>
        /// Get or set the wait pulse while waiting for the other players to cancel the match (if they so desire).
        /// </summary>
        public WaitPulsePredicate StartMatchWait { get; set; }

        /// <summary>
        /// Get or set the amount of seconds all players have to press the stop button. (5 seconds by default).
        /// </summary>
        public uint StopMatchSeconds { get; set; } = 5;

        private List<Company> m_playerCompanies;
        private SessionInfo m_sessionInfo;
        private Session m_session;
        private int m_humanCount;

        public OnlineStartupStrategy() {
            this.m_playerCompanies = null;
            this.m_session = null;
        }

        public override bool OnBegin(object caller) { // This can be cancelled by host as well by sending a self-message through the connection object.

            // Get managed lobby
            ManagedLobby lobby = caller as ManagedLobby;

            // Send begin
            if (lobby.GetConnection() is Connection connection) {

                // Should stop?
                bool shouldStop = false;
                string sender = string.Empty;

                // Send starting message with timeout and wait for stop listener
                connection.SendMessageWithResponseListener(new Message(MessageType.LOBBY_STARTING, this.StopMatchSeconds.ToString(), string.Empty), 
                    MessageType.LOBBY_CANCEL,
                    x => {
                        shouldStop = true; sender = x.Argument1;
                    }
                );

                // If we get stop message, false is returned, otherwise, if we timeout, return true (received no stop message).
                // 1000 ms * StopMatchSeconds attempts = StopMatchSeconds seconds to stop match from starting.
                bool result = SyncService.WaitAndPulseUntil(() => shouldStop == true, this.StartMatchWait, this.StopMatchSeconds, 1000); 

                // Did we timeout?
                if (result) {
                    this.OnFeedback(caller, $"Match will soon begin");
                } else {
                    this.OnFeedback(caller, $"{sender} has stopped the match.");
                }

                // Return result
                return result;

            } else {
                
                return false; // No connection to lobby

            }

        }

        public override bool OnPrepare(object caller) {

            // Get managed lobby
            ManagedLobby lobby = caller as ManagedLobby;

            // TODO: Check if local player is participating - if not, continue, otherwise, error out.

            // Return true if company was assigned
            return this.GetLocalCompany(lobby.Self.ID);

        }

        public override bool OnCollectCompanies(object caller) {

            // Get managed lobby
            ManagedLobby lobby = caller as ManagedLobby;

            // Initialize variables
            bool success = false;
            this.m_playerCompanies = new List<Company>();

            // Calculate amount of human players (exluding the host).
            this.m_humanCount = -1;
            lobby.Teams.ForEach(x => x.ForEachMember(y => this.m_humanCount += y is HumanLobbyMember ? 1 : 0));

            // Keep an attempts counter
            int attempts = 0;

            // Keep track of members from which it was possible to find company files.
            HashSet<HumanLobbyMember> members = new HashSet<HumanLobbyMember>();

            // While not all human companies have been downloaded and attempts are still below 100
            while (members.Count != this.m_humanCount && attempts < 100) {

                // Go through each team and its members
                lobby.Teams.ForEach(x => {
                    x.ForEachMember(y => {
                        if (!members.Contains(y)) { // If we've not yet downloaded this members company
                            if (y is HumanLobbyMember && y.ID != lobby.Self.ID) { // and this member is a human and not self.

                                 // Define destination
                                string destination = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, $"{y.ID}_company.json");

                                // Try and download the company
                                if (lobby.GetLobbyCompany(y.ID, destination)) {

                                    // Read in the company file and add to list.
                                    Company company = Company.ReadCompanyFromFile(destination);
                                    company.Owner = y.ID.ToString();

                                    // Register member in downloaded set.
                                    this.m_playerCompanies.Add(company);
                                    members.Add(y as HumanLobbyMember);

                                    // Log that we downloaded user company
                                    Trace.WriteLine($"Downloaded company for user {y.ID} ({y.Name}) titled '{company.Name}'", "OnlineStartupStrategy");
                                    this.OnFeedback(caller, $"Received company from {y.Name}");

                                } else { // If fail, log and try again in 100ms
                                    Trace.WriteLine($"Failed to download company for user {y.ID}. Will attempt again in 100ms", "OnlineStartupStrategy");
                                    // TODO: Send message to user it wasn't possible to download their file.
                                }
                            }
                        }
                    });
                });

                // Wait 100ms to give users a chance to uploaded
                Thread.Sleep(100);

                // Increase attempts counter
                attempts++;

            }

            // Did we get all members?
            success = members.Count == this.m_humanCount;

            // Log
            Trace.WriteLine($"Found {members.Count} and expected {this.m_humanCount}, following {attempts} attempts, thus retrieving companies will return {success}", "OnlineStartupStrategy");

            // Log depending on outcome
            if (success) {

                // Log
                this.OnFeedback(null, $"Received all company files.");

            } else {
                this.OnFeedback(null, $"Failed to receive one or more company files.");
            }

            // Return success value;
            return success;

        }

        public override bool OnCollectMatchInfo(object caller) {

            // Invoke the external session info collector
            var info = this.SessionInfoCollector?.Invoke();
            if (info.HasValue) {
                
                // Get session info
                this.m_sessionInfo = info.Value;

                // Add self company (if any)
                if (this.LocalCompany is not null) {
                    this.m_playerCompanies.Insert(0, this.LocalCompany);
                }

                // Zip/Map the companies to their respective SessionParticipant instances.
                Session.ZipCompanies(this.m_playerCompanies.ToArray(), ref this.m_sessionInfo);

                // Create session
                this.m_session = Session.CreateSession(this.m_sessionInfo);

                // Return true
                return true;

            } else {

                // Return false (Something went wrong)
                return false;

            }

        }

        public virtual ICompanyCompiler GetCompanyCompiler() => new CompanyCompiler();

        public virtual ISessionCompiler GetSessionCompiler() => new SessionCompiler();

        public override bool OnCompile(object caller) {

            // Get managed lobby
            ManagedLobby lobby = caller as ManagedLobby;

            // Create compiler
            var compiler = this.GetSessionCompiler();
            compiler.SetCompanyCompiler(this.GetCompanyCompiler());

            // Send feedback that match data is being compiled
            this.OnFeedback(caller, "Compiling match data into gamemode.");

            // Compile session
            if (!SessionUtility.CompileSession(compiler, this.m_session)) {
                return false;
            }

            // Send feedback that match data was compiled
            this.OnFeedback(caller, "Gamemode has been compiled and is being uploaded");

            // Return true
            return UploadGamemode(lobby);

        }

        private static bool UploadGamemode(ManagedLobby lobby) {

            // Get the connection
            Connection connection = lobby.GetConnection();
            if (connection is null) {
                return false;
            }

            // Get path to win condition
            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\subscriptions\\coh2_battlegrounds_wincondition.sga";

            // Verify the archive file actually exists
            if (File.Exists(sgapath)) {

                // Upload (and make sure the file was uploaded)
                if (FileHub.UploadFile(sgapath, "gamemode.sga", lobby.LobbyFileID)) {

                    // Return true
                    return true;

                } else {

                    // Failed to upload gamemode
                    return false;

                }

            } else {

                // Failed to compile correctly
                return false;

            }

        }

        public override bool OnWaitForStart(object caller) { // Wait for all players to notify they've downloaded and installed the gamemode.

            // Get lobby
            var lobby = caller as ManagedLobby;

            // Get the connection
            Connection connection = lobby.GetConnection();
            if (connection is null) {
                return false;
            }

            // The "ok" messages
            int sentOK = 0;

            // The response handler
            void OnResponse(Message message) {
                if (message.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                    sentOK++;
                } else if (message.Descriptor == MessageType.ERROR_MESSAGE) {
                    this.OnCancel(caller, "Failed to receive gamemode.");
                } else {
                    Trace.WriteLine(message.Argument1, "OnlineStartupStrategy");
                }
            }

            // Notify lobby players the gamemode is available
            int id = connection.SendMessageWithResponse(new Message(MessageType.LOBBY_NOTIFY_GAMEMODE), OnResponse);

            // Wait until condition
            bool timeout = SyncService.WaitUntil(() => sentOK >= this.m_humanCount, 100, 50).Then(() => { 
                // TODO: Handle
            });

            // Clear the identifier
            connection.ClearIdentifierReceiver(id);

            // return false if timed out
            if (timeout) {
                return false;
            }

            // Return true -> All players have downloaded the gamemode.
            return true;

        }

        public override bool OnWaitForAllToSignal(object caller) { // Wait for all players to signal they've launched

            // Get lobby
            var lobby = caller as ManagedLobby;

            // Get the connection
            Connection connection = lobby.GetConnection();
            if (connection is null) {
                return false;
            }

            // The "ok" messages
            int sentOK = 0;

            // The response handler
            void OnResponse(Message message) {
                if (message.Descriptor == MessageType.CONFIRMATION_MESSAGE) {
                    sentOK++;
                    this.OnFeedback(null, $"{message.Argument1} has started the game.");
                } else if (message.Descriptor == MessageType.ERROR_MESSAGE) {
                    this.OnCancel(caller, "Failed to make player start game.");
                } else {
                    Trace.WriteLine(message.Argument1, "OnlineStartupStrategy");
                }
            }

            // Notify lobby players the gamemode is available
            int id = connection.SendMessageWithResponse(new Message(MessageType.LOBBY_STARTMATCH), OnResponse);

            // Wait until condition
            bool timeout = SyncService.WaitUntil(() => sentOK >= this.m_humanCount, 100, 50).Then(() => {
                // TODO: Handle
            });

            // Clear the identifier
            connection.ClearIdentifierReceiver(id);

            // return false if timed out
            if (timeout) {
                return false;
            }

            // Return true -> All players have launched
            return true;

        }

        public override bool OnStart(object caller, out IPlayStrategy playStrategy) { // Launch CoH2 with Overwatch strategy

            // Use the overwatch strategy (and launch).
            playStrategy = new OverwatchStrategy(this.m_session);
            playStrategy.Launch();

            // Inform host they'll now be launching
            this.OnFeedback(null, $"Launching game...");

            // Return true
            return playStrategy.IsLaunched;

        }

    }

}
