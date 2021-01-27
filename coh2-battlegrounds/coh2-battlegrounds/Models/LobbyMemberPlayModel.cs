using System;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.Json;
using Battlegrounds.Online;
using Battlegrounds.Online.Lobby;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Verification;

using BattlegroundsApp.Views;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Models {
    
    public class LobbyMemberPlayModel : ILobbyPlayModel {

        private int m_maxTime = 0;
        private int m_remainingTime = 0;
        private bool m_canStop = false;
        private ManagedLobby m_lobby;
        private GameLobbyView m_view;
        private IMatchData m_playResults;
        private ISession m_session;
        private PlayCancelHandler m_cancelHandler;

        public bool CanCancel => this.m_canStop;

        public LobbyMemberPlayModel(GameLobbyView view, ManagedLobby lobby) {
            this.m_lobby = lobby;
            this.m_view = view;
        }

        public void StartCountdown(int time) {
            this.m_canStop = true;
            this.m_maxTime = time;
            this.m_remainingTime = time;
            this.DoCountdownAndUpdate();
        }

        private async void DoCountdownAndUpdate() {
            this.m_remainingTime = this.m_maxTime;
            while (this.m_remainingTime > 0) {
                _ = this.m_view.UpdateGUI(() => {
                    this.m_view.StartGameBttn.Content = $"Stop Match ({this.m_remainingTime}s)";
                    this.m_view.StartGameBttn.ToolTip = "Stop the match from starting";
                });
                await Task.Delay(1000);
                this.m_remainingTime--;
            }
            this.m_canStop = false;
            _ = this.m_view.UpdateGUI(() => {
                this.m_view.StartGameBttn.Content = $"Start Match";
                this.m_view.StartGameBttn.ToolTip = "Only the host can start the match";
            });
        }

        public void CreateSession(string guid) => this.m_session = new RemoteSession(guid);

        public void PlayGame(PlayCancelHandler cancelHandler) {

            // Assign cancel handler
            this.m_cancelHandler = cancelHandler;

            // Set listeners
            this.m_lobby.AddListener(MessageType.LOBBY_STARTMATCH, this.OnStartMatch);
            this.m_lobby.AddListener(MessageType.LOBBY_SYNCMATCH, this.OnSyncMatch);
            this.m_lobby.AddListener(MessageType.LOBBY_NOTIFY_GAMEMODE, this.OnGamemodeAvailable);
            this.m_lobby.AddListener(MessageType.LOBBY_NOTIFY_MATCH, this.OnResultsAvailable);
            this.m_lobby.AddListener(MessageType.LOBBY_SYSINFO, this.OnSysInfo, true);

        }

        private void OnSysInfo(Message message) {
            this.m_view.UpdateGUI(() => {
                this.m_view.lobbyChat.AppendText($"{message.Argument1}\n");
            });
        }

        private void OnResultsAvailable(Message message) {

            // Define save location
            string path = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.SESSION_FOLDER, "updated_company.json");

            // Download file
            if (this.m_lobby.DownloadFile($"{this.m_lobby.Self.ID}_company.json", path)) {

                // Download company
                Company company = null;

                try {

                    // Try and load the company
                    company = Company.ReadCompanyFromFile(path);

                    // Now save the company
                    PlayerCompanies.SaveCompany(company);

                } catch (ChecksumViolationException check) {

                    // Answer host
                    this.m_lobby.AnswerMessage(message, MessageType.ERROR_MESSAGE, check.Message);

                    // Inform user the gamemode was NOT validated
                    this.m_view.UpdateGUI(() => {
                        this.m_view.lobbyChat.AppendText($"[System] Updated company failed validation.\n");
                    });

                    // Stop here
                    return;

                }

                // Answer host
                this.m_lobby.AnswerMessage(message, MessageType.CONFIRMATION_MESSAGE, string.Empty);

                // Inform user the gamemode was NOT downloaded
                this.m_view.UpdateGUI(() => {
                    this.m_view.lobbyChat.AppendText($"[System] Updated company \"{company.Name}\" with match information.\n");
                });

            } else {

                // Answer host
                this.m_lobby.AnswerMessage(message, MessageType.ERROR_MESSAGE, "Failed to download.");

                // Inform user the gamemode was NOT downloaded
                this.m_view.UpdateGUI(() => {
                    this.m_view.lobbyChat.AppendText($"[System] Updated company failed to download.\n");
                });

            }

        }

        private void OnGamemodeAvailable(Message message) {

            // Define save location
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 2\\mods\\gamemode\\subscriptions\\coh2_battlegrounds_wincondition.sga";

            // Try and download
            if (this.m_lobby.DownloadFile("gamemode.sga", path)) {
                
                // Answer host
                this.m_lobby.AnswerMessage(message, MessageType.CONFIRMATION_MESSAGE, string.Empty);

                // Inform user the gamemode was downloaded
                this.m_view.UpdateGUI(() => {
                    this.m_view.lobbyChat.AppendText($"[System] Gamemode was downloaded and installed.\n");
                });

            } else {
                
                // Answer host
                this.m_lobby.AnswerMessage(message, MessageType.ERROR_MESSAGE, "Failed to download.");

                // Inform user the gamemode was NOT downloaded
                this.m_view.UpdateGUI(() => {
                    this.m_view.lobbyChat.AppendText($"[System] Gamemode failed to download - Stopping match.\n");
                });

            }

        }
        
        private void OnSyncMatch(Message message) {

            // Create session
            this.CreateSession(message.Argument1);

            // Get json playback
            if (this.m_playResults is JsonPlayback playback) {

                // Serialize and convert to bytes
                string json = playback.SerializeAsJson();
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                // Upload company
                if (this.m_lobby.UploadPlayback(bytes)) {
                    this.m_lobby.AnswerMessage(message, MessageType.CONFIRMATION_MESSAGE, string.Empty);
                } else {
                    this.m_lobby.AnswerMessage(message, MessageType.ERROR_MESSAGE, "Failed to upload.");
                }

            } else {
                // TODO: Handle
            }

        }
        
        private void OnStartMatch(Message message) {

            // Response callback for letting the host know the game was launched.
            void Respond(bool isLaunched) {
                if (isLaunched) {
                    this.m_lobby.AnswerMessage(message, MessageType.CONFIRMATION_MESSAGE, string.Empty);
                } else {
                    this.m_lobby.AnswerMessage(message, MessageType.ERROR_MESSAGE, "Failed to launch game.");
                }
            }

            // Launch game
            this.WatchoverPlaySession(Respond);

        }

        private async void WatchoverPlaySession(Action<bool> launchCallback) {
            await Task.Run(() => {

                // Create strategy and launch game
                MemberOverwatchStrategy strategy = new MemberOverwatchStrategy(this.m_session);
                strategy.Launch();

                // Invoke callback
                launchCallback?.Invoke(strategy.IsLaunched);

                // If launched
                if (strategy.IsLaunched) {

                    // Wait for strategy to exit
                    strategy.WaitForExit();

                    // If perfect
                    if (strategy.IsPerfect()) {

                        // Get the result
                        this.m_playResults = strategy.GetResults();

                    } else {
                        
                        // Let the host know a problem occured
                        this.m_lobby.SendClientProblem(true, "Game crashed or scar error was detected.");

                    }

                }

            });
        }

        public void CancelGame() {

            // If we can stop, send cancel message
            if (this.m_canStop) {
                this.m_lobby.SendCancelMessage();
            }

            // Unsubscribe from events
            // TODO: Implement

            // Invoke the cancel handler
            this.m_cancelHandler?.Invoke();

        }
    
    }

}
