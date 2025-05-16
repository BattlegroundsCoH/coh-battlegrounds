using System.IO;
using System.Text;

using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Factories;

public sealed class CoH3MatchDataBuilder(ILobby lobby, CoH3 game) {

    public Task<string> BuildMatchData() => Task.Run(() => {
        LuaSourceFileBuilder luaSourceFileBuilder = new();
        BuildTeamData(luaSourceFileBuilder);
        return luaSourceFileBuilder.ToString();
    });

    public async Task<bool> WriteMatchData(string matchData) {
        try {
            using var fs = File.Open(game.MatchDataPath, FileMode.Create);
            using var writer = new StreamWriter(fs, Encoding.UTF8);

            LuaSourceFileBuilder luaSourceFileBuilder = new();
            BuildTeamData(luaSourceFileBuilder);

            await writer.WriteAsync(matchData);

        } catch (IOException ex) {
            throw new Exception($"Failed to create match data file: {ex.Message}", ex);
        }
        return true;
    }

    private void BuildTeamData(LuaSourceFileBuilder luaSourceFile) {

        var teamData = new Dictionary<string, object>[2];
        teamData[0] = new Dictionary<string, object> {
            { "team", 1 },
            { "team_name", lobby.Team1.TeamAlias },
            { "players", BuildTeamPlayerInfo(lobby.Team1) }
        };

        teamData[1] = new Dictionary<string, object> {
            { "team", 2 },
            { "team_name", lobby.Team2.TeamAlias },
            { "players", BuildTeamPlayerInfo(lobby.Team2) }
        };

        luaSourceFile.DeclareTable("teams", table => table.AddArray(teamData));
    }

    private IEnumerable<Dictionary<string, object>> BuildTeamPlayerInfo(Team team) {
        return from slot in team.Slots
               where !slot.Hidden && !slot.Locked
               select BuildPlayerInfo(slot);
    }

    private Dictionary<string, object> BuildPlayerInfo(Team.Slot slot) {
        var participant = lobby.Participants.FirstOrDefault(x => x.ParticipantId == slot.ParticipantId)
            ?? throw new Exception($"Unable to find participant with ID {slot.ParticipantId}");
        return new Dictionary<string, object>() {
            { "name", participant.ParticipantName },
            { "faction", slot.Faction },
            { "difficulty", slot.Difficulty },
            { "company", slot.CompanyId }
        };
    }

}
