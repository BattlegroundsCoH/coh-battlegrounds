using System.Diagnostics;
using System.Linq;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Game.Match.Finalizer;

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
        if (synchronizeObject is not OnlineLobbyHandle handler) {
            Trace.WriteLine("{Error} -- The synchronizeObject is NULL and changes will therefore not be saved anywhere!", nameof(MultiplayerFinalizer));
            return;
        }

        // Get match data
        if (this.m_matchData is not IAnalyzedMatch data) {
            Trace.WriteLine("{Error} -- The match data is NULL and changes will therefore not be saved anywhere!", nameof(MultiplayerFinalizer));
            return;
        }

        // Get player results
        var playerFiles = this.m_companies.Where(x => x.Key.SteamID != BattlegroundsInstance.Steam.User.ID).Select(
            x => new LobbyPlayerCompanyFile(x.Key.SteamID, CompanySerializer.GetCompanyAsJson(x.Value, indent: false))).ToArray();

        // Get overall match results
        ServerMatchResults matchResults = new() {
            Gamemode = data.Session.Gamemode.Name,
            Map = data.Session.Scenario.RelativeFilename,
            Option = data.Session.GamemodeOption,
            Length = data.Length,
            LengthTicks = data.Length.Ticks,
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

    private static void UploadResults(OnlineLobbyHandle api, ServerMatchResults matchResults, LobbyPlayerCompanyFile[] companyFiles) {

        // Loop over the files and trigger appropriate events
        for (int i = 0; i < companyFiles.Length; i++) {
            if (companyFiles[i].playerID != api.Self.ID) {
                if (api.ServerHandle.UploadCompany(companyFiles[i].playerID, companyFiles[i].playerCompanyData,
                    (a,b) => Trace.WriteLine($"Uploading company {a}/{b} for player {companyFiles[i].playerID}", nameof(MultiplayerFinalizer))) != UploadResult.UPLOAD_SUCCESS) {
                    Trace.WriteLine($"Failed to upload result company for player {companyFiles[i].playerID}", nameof(MultiplayerFinalizer));
                }
            }
        }

        // Upload result object
        if (api.ServerHandle.UploadResults(matchResults)) {

            // Release results
            api.ReleaseResults();

        }

    }

}
