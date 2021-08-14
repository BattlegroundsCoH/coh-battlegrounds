using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Battlegrounds.Compiler;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Lobby.Match;
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

        private readonly ManualResetEventSlim m_beginWaitHandle;

        public OnlineStartupStrategy() {
            this.m_session = null;
            this.m_beginWaitHandle = new(false);
        }

        public override bool OnBegin(object caller) { // This can be cancelled by host as well by sending a self-message through the connection object.

            // Get managed lobby
            LobbyHandler lobby = caller as LobbyHandler;
            lobby.API.SetLobbyGuid(lobby.Connection.ConnectionID);

            // Should stop?
            bool shouldStop = false;

            // Get timer
            lobby.MatchStartTimer = lobby.MatchContext.GetStartTimer(5, 1.0);
            lobby.MatchStartTimer.OnPulse += x => this.StartMatchWait?.Invoke((int)x.TotalSeconds);
            lobby.MatchStartTimer.OnTimedDown += () => this.m_beginWaitHandle.Set();
            lobby.MatchStartTimer.OnCancel += x => {
                shouldStop = true;
                this.m_beginWaitHandle.Set();
            };
            lobby.MatchStartTimer.Start(); // Dont forget to start the timer...

            // Wait
            this.m_beginWaitHandle.Wait();

            // Did we timeout?
            if (!shouldStop) {
                this.OnFeedback(caller, $"Match will soon begin");
            } else {
                this.OnFeedback(caller, $"The match countdown was stopped.");
            }

            // Return result
            return !shouldStop;

        }

        public override bool OnPrepare(object caller) {

            // Get managed lobby
            LobbyHandler lobby = caller as LobbyHandler;

            // TODO: Check if local player is participating - if not, continue, otherwise, error out.

            // Return true if company was assigned
            return this.GetLocalCompany(lobby.Self.ID);

        }

        public override bool OnCollectCompanies(object caller) {

            // Get managed lobby
            LobbyHandler lobby = caller as LobbyHandler;
            ILobbyMatchContext context = lobby.MatchContext;

            // Initialize variables
            this.m_playerCompanies = new List<Company>();

            // Request companies
            context.RequestCompanies();

            // Wait slightly
            Thread.Sleep(100);

            // Attempt counter
            DateTime time = DateTime.Now;

            // Wait for all companies to be uploaded
            while ((DateTime.Now - time).TotalSeconds < 5 && !context.HasAllPlayerCompanies()) {
                Thread.Sleep(100);
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

                } catch {

                    // Log
                    Trace.WriteLine($"Failed to download company from user {x.playerID}.", nameof(OnlineStartupStrategy));

                }

            });

            // Log depending on outcome
            if (count > 0) {

                // Log
                this.OnFeedback(null, $"Received all company files.");

            } else {

                // Log
                this.OnFeedback(null, $"Failed to receive one or more company files.");

            }

            // Return success value;
            return count == lobby.Lobby.Humans - 1;

        }

        public override bool OnCollectMatchInfo(object caller) {

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
            LobbyHandler lobby = caller as LobbyHandler;

            // Create compiler
            ISessionCompiler compiler = this.GetSessionCompiler();
            compiler.SetCompanyCompiler(this.GetCompanyCompiler());

            // Send feedback that match data is being compiled
            this.OnFeedback(caller, "Compiling match data into gamemode.");

            // Compile session
            if (!SessionUtility.CompileSession(compiler, this.m_session, lobby.API)) {
                return false;
            }

            // Send feedback that match data was compiled
            this.OnFeedback(caller, "Gamemode has been compiled and is being uploaded");

            // Return true
            return UploadGamemode(lobby.MatchContext);

        }

        private static bool UploadGamemode(ILobbyMatchContext matchContext) {

            // Get path to win condition
            string sgapath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\subscriptions\\coh2_battlegrounds_wincondition.sga";

            // Verify the archive file actually exists
            if (File.Exists(sgapath)) {

                // Read binary
                byte[] gamemode = File.ReadAllBytes(sgapath);

                // Upload gamemode
                return matchContext.UploadGamemode(gamemode);

            } else {

                // Failed to compile correctly
                return false;

            }

        }

        public override bool OnWaitForStart(object caller) { // Wait for all players to notify they've downloaded and installed the gamemode.

            // Get lobby
            LobbyHandler lobby = caller as LobbyHandler;

            // Tell context to launch
            lobby.MatchContext.LaunchMatch();

            // TODO: Add verification check

            // Return true -> All players have downloaded the gamemode.
            return true;

        }

        public override bool OnWaitForAllToSignal(object caller) { // Wait for all players to signal they've launched

            // Return true -> All players have launched
            return true;

        }

        public override bool OnStart(object caller, out IPlayStrategy playStrategy) { // Launch CoH2 with Overwatch strategy

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
