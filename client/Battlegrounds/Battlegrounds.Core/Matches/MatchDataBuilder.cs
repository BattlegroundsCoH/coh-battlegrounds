using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Games.Blueprints;
using Battlegrounds.Core.Games.Scripts;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Matches;

public sealed class MatchDataBuilder(ILogger<MatchDataBuilder> logger) {

    private readonly ILogger<MatchDataBuilder> _logger = logger;

    public bool Build(ScarScriptWriter scarWriter, MatchData matchData) {

        // Write team setup
        scarWriter.BeginTable("bg_teams")
            // Write teeam 1
            .BeginTable()
                .WriteCollection("players", matchData.Team1, item => WriteTeamPlayer(scarWriter, item))
            .EndTable()
            // Write team 2
            .BeginTable()
                .WriteCollection("players", matchData.Team2, item => WriteTeamPlayer(scarWriter, item))
            .EndTable()
            .EndTable();

        // Iterate company setup
        var alreadyWritten = new HashSet<Guid>();
        var blueprints = new HashSet<Blueprint>();
        scarWriter.EmptyTable("bg_companies");
        foreach (var company in matchData.Team1.Select(x => x.Company).Union(matchData.Team2.Select(x => x.Company))) {
            if (alreadyWritten.Contains(company.Id))
                continue;
            scarWriter.AssignTo($"bg_companies[\"{company.Id}\"]").BeginTable()
                .Serialize(company, (w,c) => WriteCompany(w,c,blueprints))
                .EndTable();
            alreadyWritten.Add(company.Id);
        }

        // Iterate blueprints
        scarWriter.EmptyTable("bg_uidata");
        foreach (var bp in blueprints) {
            scarWriter.AssignTo($"bg_uidata[\"{bp.ScarReferenceId}\"]").BeginTable()
                .AssignTo("icon").String(bp.ScarReferenceId)
                .Field("name").String(bp.ScarReferenceId).NewLine()
                .EndTable();
        }

        return true;

    }

    private void WriteTeamPlayer(ScarScriptWriter scarScriptWriter, MatchPlayer player) {
        scarScriptWriter.AssignTo("name").String(player.Name);
        scarScriptWriter.Field("difficulty").Integer(player.Difficulty);
        scarScriptWriter.Field("company").String(player.Company.Id.ToString());
        scarScriptWriter.Field("faction").String(player.Company.Faction.ScarReferenceId).NewLine();
    }

    private void WriteCompany(ScarScriptWriter scarScriptWriter, ICompany company, ISet<Blueprint> blueprints) {
        scarScriptWriter.AssignTo("name").String(company.Name).NewLine();
        scarScriptWriter.WriteCollection("units", company.Squads, item => {
            
            scarScriptWriter.AssignTo("squadId").Integer(item.SquadId)
                .Field("blueprint").String(item.Blueprint.ScarReferenceId)
                .Field("experience").Number(item.Experience)
                .Field("category").Integer(0);

            if (!string.IsNullOrEmpty(item.Name)) {
                throw new NotImplementedException("Custom unit names not implemented");
            }
            
            if (item.Transport is not null) {
                throw new NotImplementedException("Custom transport not implemented");
            }
            
            if (item.Crew is not null) {
                throw new NotImplementedException("Custom crew not implemented");
            }
            
            if (item.Items is not null && item.Items.Count > 0) {
                throw new NotImplementedException("Unit items not implemented");
            }
            
            if (item.Upgrades is not null && item.Upgrades.Count > 0) {
                throw new NotImplementedException("Unit upgrades not implemented");
            }
            
            scarScriptWriter.NewLine();
            blueprints.Add(item.Blueprint);

        });
    }

}
