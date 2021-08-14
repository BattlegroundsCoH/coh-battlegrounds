using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Game.DataCompany;

using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Requests;
using Battlegrounds.Networking.DataStructures;

using BattlegroundsApp.Views;

namespace BattlegroundsApp.Models {

    public class LobbyMemberPlayModel : ILobbyPlayModel {

        private readonly LobbyHandler m_lobby;
        private readonly GameLobbyView m_view;

        private IMatchData m_playResults;
        private ISession m_session;
        private PlayCancelHandler m_cancelHandler;
        private ISynchronizedTimer m_timer;

        public bool CanCancel { get; private set; }

        public LobbyMemberPlayModel(GameLobbyView view, LobbyHandler lobby) {

            // Set fields
            this.m_lobby = lobby;
            this.m_view = view;

            // Add events
            lobby.Lobby.StartGame = this.OnStartMatch;
            lobby.Lobby.GamemodeAvailable = this.OnGamemodeAvailable;
            lobby.Lobby.ResultsAvailable = this.OnResultsAvailable;
            lobby.Lobby.GetCompany = this.OnGetCompany;

            // Get timer publish
            if (lobby.RequestHandler is ParticipantRequestHandler participant) {
                participant.ObjectPublished += x => {
                    if (x is ISynchronizedTimer y) {
                        Trace.WriteLine("Received timer object.", nameof(LobbyMemberPlayModel));
                        this.m_timer = y;
                        ISynchronizedTimer.RegisterEvents(y,
                            this.TimerOver,
                            this.StartCountdown,
                            this.CancelTimerExternal,
                            this.Pulse);
                    }
                };
            } else {
                Trace.WriteLine($"Error: Cannot register timer on requesthandler {lobby.RequestHandler.GetType().Name}", nameof(LobbyMemberPlayModel));
            }

        }

        public void StartCountdown() {
            Trace.WriteLine("Started countdown", nameof(LobbyMemberPlayModel));
            _ = this.m_view.UpdateGUI(() => {
                this.m_view.LobbyChat.DisplayMessage($"[System] The host has pressed the start match button.");
                this.CanCancel = true;
            });
        }

        private void Pulse(TimeSpan time) {
            if (time == TimeSpan.Zero) {
                return;
            }
            Trace.WriteLine($"Timer pulse : {time}", nameof(LobbyMemberPlayModel));
            _ = this.m_view.UpdateGUI(() => {
                this.m_view.LobbyChat.DisplayMessage($"[System] The match will start in {(int)time.TotalSeconds} seconds.");
            });
        }

        private void CancelTimerExternal(object arg) {
            Trace.WriteLine("Stopped countdown", nameof(LobbyMemberPlayModel));
            _ = this.m_view.UpdateGUI(() => {
                if (arg is null) {
                    arg = "the host";
                }
                this.m_view.LobbyChat.DisplayMessage($"[System] The match countdown was stopped by {arg}.");
                this.CanCancel = false;
            });
        }

        private void TimerOver() => this.m_view.UpdateGUI(() => {
            this.m_view.LobbyChat.DisplayMessage($"[System] The match will begin shortly.");
            this.CanCancel = false;
        });

        public void CreateSession(string guid) => this.m_session = new RemoteSession(guid);

        public void PlayGame(PlayCancelHandler cancelHandler) => this.m_cancelHandler = cancelHandler; // Assign cancel handler

        private void OnResultsAvailable(string jsondata) {

            try {
                // Save company
                File.WriteAllText("~latest_company.json", jsondata);
            } catch {
                Trace.WriteLine("Failed to save latest company result.", nameof(LobbyMemberPlayModel));
            }

            // Load company
            Company company = CompanySerializer.GetCompanyFromJson(jsondata);

            // Now save the company
            LobbyHostPlayModel.OnCompanySerialized(company);
            
            // Let client know the company was updated
            _ = this.m_view.UpdateGUI(() => {
                this.m_view.LobbyChat.DisplayMessage($"[System] Updated company '{company.Name}' with match results.");
            });

        }

        private void OnGamemodeAvailable(byte[] binary) {

            // Define save location
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\subscriptions\\coh2_battlegrounds_wincondition.sga";

            // Remove file if it already exists
            if (File.Exists(path)) {
                File.Delete(path);
            }

            // Write file
            File.WriteAllBytes(path, binary);

            // Let client know the company was updated
            _ = this.m_view.UpdateGUI(() => {
                this.m_view.LobbyChat.DisplayMessage($"[System] Downloaded gamemode file.");
            });

        }

        private async void OnStartMatch() {
            await Task.Run(() => {

                // Create session
                this.CreateSession(new Guid().ToString());

                // Create strategy and launch game
                MemberOverwatchStrategy strategy = new MemberOverwatchStrategy(this.m_session);
                strategy.Launch();
                
                // Inform client the game is starting
                _ = this.m_view.UpdateGUI(() => {
                    this.m_view.LobbyChat.DisplayMessage($"[System] Launching Company of Heroes 2.");
                });

                // If launched
                if (strategy.IsLaunched) {

                    // Wait for strategy to exit
                    strategy.WaitForExit();

                    // If perfect
                    if (strategy.IsPerfect()) {

                        // Get the result
                        this.m_playResults = strategy.GetResults();

                    } else {

                        // Log
                        Trace.WriteLine("Game crashed or scar error was detected. -- Currently no error is sent to HOST!!!!", nameof(LobbyMemberPlayModel));

                    }

                }

            });

        }

        private string OnGetCompany() {

            // Get self from team manager
            Company self = null;

            // Invoke the following on the GUI thread and wait for it to compute.
            _ = this.m_view.UpdateGUI(() => {
                self = this.m_view.GetLocalCompany();
            }).Wait();

            // Make sure we're valid
            if (self is not null) {

                // Get string
                string json = CompanySerializer.GetCompanyAsJson(self, false);
                Trace.WriteLine($"Uploading json data: {json}", nameof(LobbyMemberPlayModel));

                // Convert to json
                return json;

            } else {

                // Log failure
                Trace.WriteLine("Failed to find local company and returning NULL!", nameof(LobbyMemberPlayModel));

                // Return empty object
                return "{}";

            }

        }

        public void CancelGame() {

            // If we can stop, send cancel message
            if (this.CanCancel) {
                this.m_lobby.MatchStartTimer.Cancel(this.m_lobby.Self.Name);
            }

            // Invoke the cancel handler
            this.m_cancelHandler?.Invoke();

        }

    }

}
