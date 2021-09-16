using System.Diagnostics;
using System.Linq;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.Playing;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Game.Match.Finalizer {

    /// <summary>
    /// Finalize strategy for online matches. Extension of <see cref="SingleplayerFinalizer"/>.
    /// </summary>
    public class MultiplayerFinalizer : SingleplayerFinalizer {

        public override void Synchronize(object synchronizeObject) {

            // Log
            Trace.WriteLine("Synchronizing end of match results.", nameof(MultiplayerFinalizer));

            // Make sure we log this unfortunate event
            if (this.CompanyHandler is null) {
                Trace.WriteLine("{Warning} -- The company handler is NULL and changes will therefore not be saved!", nameof(MultiplayerFinalizer));
            }

            // Get handler
            LobbyHandler handler = synchronizeObject as LobbyHandler;

            // Get player results
            var playerFiles = this.m_companies.Where(x => x.Key.SteamID != BattlegroundsInstance.Steam.User.ID).Select(
                x => new LobbyPlayerCompanyFile(x.Key.SteamID, CompanySerializer.GetCompanyAsJson(x.Value))).ToArray();

            // Get overall match results
            ServerMatchResults matchResults = new() {
                Gamemode = this.m_matchData.Session.Gamemode.Name,
                Map = this.m_matchData.Session.Scenario.RelativeFilename,
                Option = this.m_matchData.Session.GamemodeOption,
                Length = this.m_matchData.Length,
                LengthTicks = this.m_matchData.Length.Ticks,
            };

            // Inform members the results are available
            handler.MatchContext.UploadResults(matchResults, playerFiles);

            // Get self company (and invoke so host is also updated)
            if (this.GetLocalPlayerCompany() is Company company) {
                Trace.WriteLine($"Invoking company handler for host company '{company.Name}'", nameof(MultiplayerFinalizer));
                this.CompanyHandler?.Invoke(company);
            } else {
                Trace.WriteLine("Failed to find host company and will, therefore, not update host company. (Potentially fatal).", nameof(MultiplayerFinalizer));
            }

        }

    }

}
