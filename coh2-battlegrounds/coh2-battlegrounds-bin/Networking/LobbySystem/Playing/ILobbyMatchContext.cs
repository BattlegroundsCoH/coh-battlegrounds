using Battlegrounds.Networking.DataStructures;
using Battlegrounds.Networking.Requests;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem.Playing {

    /// <summary>
    /// Represents a company file paired with the company owner.
    /// </summary>
    public readonly struct LobbyPlayerCompanyFile {

        /// <summary>
        /// The ID of the player owning the company.
        /// </summary>
        public readonly ulong playerID;

        /// <summary>
        /// The json data representing the company.
        /// </summary>
        public readonly string playerCompanyData;

        /// <summary>
        /// Initialize a new <see cref="LobbyPlayerCompanyFile"/> object.
        /// </summary>
        /// <param name="pid">The ID of the player owning the company.</param>
        /// <param name="company">The json data representing the company.</param>
        public LobbyPlayerCompanyFile(ulong pid, string company) {
            this.playerID = pid;
            this.playerCompanyData = company;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerCompanyFile"></param>
    public delegate void PlayerCompanyCallback(LobbyPlayerCompanyFile playerCompanyFile);

    public interface ILobbyMatchContext {

        /// <summary>
        /// Get the request handler that handles server communication.
        /// </summary>
        IManagingRequestHandler RequestHandler { get; }

        /// <summary>
        /// Get the lobby tied to the context.
        /// </summary>
        ILobby Lobby { get; }

        /// <summary>
        /// Request all playing lobby members to upload their company files.
        /// </summary>
        void RequestCompanies();

        /// <summary>
        /// Upload the .sga gamemode to the lobby and instruct lobby members to download it.
        /// </summary>
        /// <param name="rawBinaryData">The raw .sga binary data to upload.</param>
        bool UploadGamemode(byte[] rawBinaryData);

        /// <summary>
        /// Upload the company results following a match.
        /// </summary>
        /// <param name="matchResults">Provides additional results for the server to process.</param>
        /// <param name="results">The relevant company file results.</param>
        void UploadResults(ServerMatchResults matchResults, LobbyPlayerCompanyFile[] results);

        /// <summary>
        /// Instruct lobby members to start the game. Will also trigger the host StartGame lobby event.
        /// </summary>
        void LaunchMatch();

        /// <summary>
        /// Report an error to the server.
        /// </summary>
        /// <param name="errorCode">The error code given to this specifc error instance.</param>
        /// <param name="errorMessage">The error message to send.</param>
        void ReportError(int errorCode, string errorMessage);

        /// <summary>
        /// Get the <see cref="LobbyPlayerCompanyFile"/> for specified player.
        /// </summary>
        /// <param name="playerID">The ID of the player to get company file from.</param>
        LobbyPlayerCompanyFile GetPlayerCompany(ulong playerID);

        /// <summary>
        /// Get if all players have uploaded their company.
        /// </summary>
        /// <returns>True iff all playing players have uploaded their company.</returns>
        bool HasAllPlayerCompanies();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyCallback"></param>
        /// <returns></returns>
        int CollectPlayerCompanies(PlayerCompanyCallback companyCallback);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool HasAllGamemode();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool HasAllResults();

        /// <summary>
        /// Get a starting timer that can be used to warn lobby members the game is about to start.
        /// </summary>
        /// <param name="countdownSeconds">The amount of seconds to count down.</param>
        /// <param name="syncGraceSeconds">The amount of time (in seconds) to allow for synchronization problems.</param>
        /// <returns>A <see cref="ISynchronizedTimer"/> that can be used to count down.</returns>
        ISynchronizedTimer GetStartTimer(int countdownSeconds, double syncGraceSeconds);

    }

}
