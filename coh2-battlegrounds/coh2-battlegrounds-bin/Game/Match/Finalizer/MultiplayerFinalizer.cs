using System.Diagnostics;
using System.Linq;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Lobby.Match;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Game.Match.Finalizer {

    /// <summary>
    /// Finalize strategy for online matches. Extension of <see cref="SingleplayerFinalizer"/>.
    /// </summary>
    public class MultiplayerFinalizer : SingleplayerFinalizer {

        public override void Synchronize(object synchronizeObject) {

            // Make sure we log this unfortunate event
            if (this.CompanyHandler is null) {
                Trace.WriteLine("{Warning} -- The company handler is NULL and changes will therefore not be saved!", nameof(MultiplayerFinalizer));
            }

            // Get handler
            LobbyHandler handler = synchronizeObject as LobbyHandler;

            // Get player results
            LobbyPlayerCompanyFile[] playerFiles = this.m_companies.Select(x => new LobbyPlayerCompanyFile(x.Key.SteamID, CompanySerializer.GetCompanyAsJson(x.Value, false))).ToArray();
            ServerMatchResults matchResults = new() {
                Gamemode = this.m_matchData.Session.Gamemode.Name,
                Map = this.m_matchData.Session.Scenario.RelativeFilename,
                Option = this.m_matchData.Session.GamemodeOption,
                Length = this.m_matchData.Length,
                LengthTicks = this.m_matchData.Length.Ticks,
            };

            // Inform members the results are available
            handler.MatchContext.UploadResults(matchResults, playerFiles);

        }

    }

}
