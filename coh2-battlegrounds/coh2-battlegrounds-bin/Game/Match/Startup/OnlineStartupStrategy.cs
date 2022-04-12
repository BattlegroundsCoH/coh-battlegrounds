using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

using Battlegrounds.Compiler;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Networking;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Game.Match.Startup {

    /// <summary>
    /// Startup Strategy specifically for games with more than one human player. Can be extended with custom behaviour.
    /// </summary>
    public class OnlineStartupStrategy : BaseStartupStrategy {

        /// <summary>
        /// Get or set the amount of seconds all players have to press the stop button. (5 seconds by default).
        /// </summary>
        public uint StopMatchSeconds { get; set; } = 5;

        /// <summary>
        /// Get or set boolean flag whether the host has cancelled the match.
        /// </summary>
        public bool CancelledByHost { get; set; } = false;

        private List<Company>? m_playerCompanies;
        private SessionInfo m_sessionInfo;
        private Session? m_session;

        public OnlineStartupStrategy() {
            this.m_session = null;
        }

        public override bool OnBegin(object caller) { // This can be cancelled by host as well by sending a self-message through the connection object.

            // Get managed lobby
            if (caller is not LobbyAPI lobby) {
                return false;
            }

            // Return result
            return lobby.StartMatch(this.StopMatchSeconds) && !this.CancelledByHost;

        }

        public override bool OnPrepare(object caller) {

            // Get managed lobby
            if (caller is not LobbyAPI lobby) {
                return false;
            }

            // TODO: Check if local player is participating - if not, continue, otherwise, error out.

            // Return true if company was assigned
            return this.GetLocalCompany(lobby.Self.ID);

        }

        public override bool OnCollectCompanies(object caller) {

            // Get managed lobby
            if (caller is not LobbyAPI lobby) {
                return false;
            }

            // Initialize variables
            this.m_playerCompanies = new List<Company>();

            // Request companies
            lobby.RequestCompanyFile();

            // Wait slightly
            Thread.Sleep(100);

            // Create match API
            LobbyMatchAPI context = new(lobby);

            // Attempt counter
            DateTime time = DateTime.Now;

            // Flag to determine if all player companies were received.
            bool allFlag = false;

            // Wait for all companies to be uploaded
            while ((DateTime.Now - time).TotalSeconds <= 15.0) {
                allFlag = context.HasAllPlayerCompanies();
                if (allFlag) {
                    break;
                }
                Trace.WriteLine($"Not all companies delivered after {(DateTime.Now - time).TotalSeconds}s", nameof(OnlineStartupStrategy));
                Thread.Sleep(1000);
            }

            // If still false -> Bail
            if (!allFlag) {

                // Log
                this.OnFeedback(null, $"Server reported not all players uploaded company file.");

                // Halt method execution here
                return false;

            } else {

                // Log
                Trace.WriteLine($"Server reported all company files uploaded within {(DateTime.Now-time).TotalSeconds:0.00}s.", nameof(OnlineStartupStrategy));

            }

            // Collect companies
            int count = context.CollectPlayerCompanies(x => {

                try {

                    // Read in the company file and add to list.
                    Company company = CompanySerializer.GetCompanyFromJson(x.playerCompanyData);
                    company.Owner = x.playerID.ToString();

                    // Register company
                    this.m_playerCompanies.Add(company);

                    // Log
                    Trace.WriteLine($"Downloaded company from user {x.playerID} titled '{company.Name}'", nameof(OnlineStartupStrategy));

                } catch (Exception e) {

                    // Log
                    Trace.WriteLine($"Failed to download company from user {x.playerID}.", nameof(OnlineStartupStrategy));
                    Trace.WriteLine($"Exception says:\n{e}", nameof(OnlineStartupStrategy));

                }

            });

            // Get human count:
            uint humans = lobby.GetPlayerCount(humansOnly: true);

            // Check if received all
            if (count == humans - 1) {

                // Log
                this.OnFeedback(null, $"Received all company files.");

                return true;

            } else {

                // Log
                this.OnFeedback(null, $"Failed to receive {(count > 0 ? "all" : "any")} company files.");

                // Return false => Error
                return false;

            }

        }

        public override bool OnCollectMatchInfo(object caller) {

            // Ensure player companies have been collected
            if (this.m_playerCompanies is null)
                return false;

            // Invoke the external session info collector
            SessionInfo? info = this.SessionInfoCollector?.Invoke();
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
            if (caller is not LobbyAPI lobby) {
                return false;
            }

            // If no session, we cannot compile
            if (this.m_session is null) {
                this.OnFeedback(caller, "No session object was created!");
                return false;
            }

            // Create compiler
            ISessionCompiler compiler = this.GetSessionCompiler();
            compiler.SetCompanyCompiler(this.GetCompanyCompiler());

            // Send feedback that match data is being compiled
            this.OnFeedback(caller, "Compiling match data into gamemode.");

            // Compile session
            if (!SessionUtility.CompileSession(compiler, this.m_session, NetworkInterface.APIObject)) {
                return false;
            }

            // Send feedback that match data was compiled
            this.OnFeedback(caller, "Gamemode has been compiled and is being uploaded");

            // Return true
            return UploadGamemode(lobby);

        }

        private static bool UploadGamemode(LobbyAPI api) {

            // Get path to win condition
            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\subscriptions\\coh2_battlegrounds_wincondition.sga";

            // Verify the archive file actually exists
            if (File.Exists(sgapath)) {

                // Read binary
                byte[] gamemode = File.ReadAllBytes(sgapath);

                // Upload gamemode
                if (api.ServerHandle.UploadGamemode(gamemode, (a, b) => Trace.WriteLine($"Gamemode upload: {a}/{b}", nameof(OnlineStartupStrategy))) == UploadResult.UPLOAD_SUCCESS) {

                    // Instruct players to download gamemode
                    api.ReleaseGamemode();

                    // Return true
                    return true;

                }

            } 

            // Failed to compile correctly
            return false;

        }

        public override bool OnWaitForStart(object caller) { // Wait for all players to notify they've downloaded and installed the gamemode.

            // Get lobby
            if (caller is not LobbyAPI lobby) {
                return false;
            }

            // Tell context to launch
            lobby.LaunchMatch();

            // Return true -> All players have downloaded the gamemode.
            return true;

        }

        public override bool OnWaitForAllToSignal(object caller) { // Wait for all players to signal they've launched

            // Return true -> All players have launched
            return true;

        }

        public override bool OnStart(object caller, [NotNullWhen(true)] out IPlayStrategy? playStrategy) { // Launch CoH2 with Overwatch strategy

            // Set play strategy to null
            playStrategy = null;

            // Ensure play strategy factory exists
            if (this.PlayStrategyFactory is null) {
                this.OnFeedback(caller, "No play strategy set...");
                return false;
            }

            if (this.m_session is null) {
                this.OnFeedback(caller, "No session object was created!");
                return false;
            }

            // Use the overwatch strategy (and launch).
            playStrategy = this.PlayStrategyFactory.CreateStrategy(this.m_session);
            playStrategy.Launch();

            // Inform host they'll now be launching
            this.OnFeedback(null, $"Launching game...");

            // Return true
            return playStrategy.IsLaunched;

        }

    }

}
