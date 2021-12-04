using System;
using System.Diagnostics;
using System.Linq;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Networking.LobbySystem;
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
            LobbyAPI handler = synchronizeObject as LobbyAPI;

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
            UploadResults(handler, matchResults, playerFiles);

            // Get self company (and invoke so host is also updated)
            if (this.GetLocalPlayerCompany() is Company company) {
                Trace.WriteLine($"Invoking company handler for host company '{company.Name}'", nameof(MultiplayerFinalizer));
                this.CompanyHandler?.Invoke(company);
            } else {
                Trace.WriteLine("Failed to find host company and will, therefore, not update host company. (Potentially fatal).", nameof(MultiplayerFinalizer));
            }

        }

        private void UploadResults(LobbyAPI api, ServerMatchResults matchResults, LobbyPlayerCompanyFile[] companyFiles) {

            // Loop over the files and trigger appropriate events
            for (int i = 0; i < companyFiles.Length; i++) {
                if (companyFiles[i].playerID == api.Self.ID) {
                    throw new NotImplementedException();
                } else {
                    api.ServerHandle.UploadCompany(companyFiles[i].playerID, companyFiles[i].playerCompanyData);
                }
            }

            // Upload result object
            if (api.ServerHandle.UploadResults(matchResults)) {

                // Release results
                api.ReleaseResults();

            }

        }


    }

}
