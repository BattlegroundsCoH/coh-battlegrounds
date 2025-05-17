using System.IO;
using System.Text;

using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;
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

            luaSourceFileBuilder.DeclareTable("companies", table => {
                foreach (var company in lobby.Companies) {
                    table.AddNestedTable(company.Key, subTable => BuildCompanyData(subTable, company.Value));
                }
            });

            await writer.WriteAsync(matchData);

        } catch (IOException ex) {
            throw new Exception($"Failed to create match data file: {ex.Message}", ex);
        }
        return true;
    }

    private void BuildTeamData(LuaSourceFileBuilder luaSourceFile) {
        luaSourceFile.DeclareTable("teams", table => {
            BuildTeamData(table, 1, lobby.Team1);
            BuildTeamData(table, 2, lobby.Team2);
        });
    }

    private void BuildTeamData(LuaSourceFileBuilder.TableBuilder table, int teamNumber, Team team) {
        table.AddKeyValue("team", teamNumber)
            .AddKeyValue("team_name", team.TeamAlias)
            .AddNestedTable("players", playersTable => {
                var players = from slot in team.Slots
                              where !slot.Hidden && !slot.Locked
                              select slot;
                foreach (var slot in players) {
                    playersTable.AddNestedTable(x => BuildPlayerInfo(x, slot));
                }
            });
    }

    private void BuildPlayerInfo(LuaSourceFileBuilder.TableBuilder table, Team.Slot slot) {
        var participant = lobby.Participants.FirstOrDefault(x => x.ParticipantId == slot.ParticipantId)
            ?? throw new Exception($"Unable to find participant with ID {slot.ParticipantId}");
        table.AddKeyValue("name", participant.ParticipantName)
            .AddKeyValue("faction", slot.Faction)
            .AddKeyValue("difficulty", slot.Difficulty)
            .AddKeyValue("company", slot.CompanyId);
    }

    private void BuildCompanyData(LuaSourceFileBuilder.TableBuilder table, Company company) {
        table.AddKeyValue("name", company.Name);
        foreach (var squad in company.Squads) {
            table.AddNestedTable(squad.Id, subTable => BuildCompanySquadData(subTable, squad));
        }
    }

    private void BuildCompanySquadData(LuaSourceFileBuilder.TableBuilder table, Squad squad) {
        if (squad.HasCustomName) {
            table.AddKeyValue("name", squad.Name);
        }
        table.AddKeyValue("experience", squad.Experience)
            .AddKeyValue("blueprint", squad.Blueprint.Id)
            .AddKeyValue("category", (byte)squad.Blueprint.Cateogry)
            .AddNestedTable("cost", subTable => BuildCostData(subTable, squad.Blueprint.Cost)); // TODO: Make cost calculation based on transport and upgrades
        var items = from item in squad.SlotItems
                       where item.UpgradeBlueprint != null
                       select item.UpgradeBlueprint;
        var upgradeList = squad.Upgrades.Concat(items).ToList();
        if (upgradeList.Count > 0) {
            table.AddNestedTable("upgrades", upgradeTable => {
                foreach (var upgrade in upgradeList) {
                    upgradeTable.AddValue(upgrade.Id);
                }
            });
        }
        if (squad.HasTransport) {
            table.AddNestedTable("transport", transportTable => {
                var transport = squad.Transport!;
                transportTable.AddKeyValue("blueprint", transport.TransportBlueprint.Id)
                    .AddKeyValue("dropoff", transport.DropOffOnly);
            });
        }
    }

    private static void BuildCostData(LuaSourceFileBuilder.TableBuilder table, CostExtension cost) 
        => table.AddKeyValue("manpower", cost.Manpower)
            .AddKeyValue("fuel", cost.Fuel)
            .AddKeyValue("munitions", cost.Munitions);

}
