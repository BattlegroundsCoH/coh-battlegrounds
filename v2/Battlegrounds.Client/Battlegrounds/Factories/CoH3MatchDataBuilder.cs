﻿using System.IO;

using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Factories;

public sealed class CoH3MatchDataBuilder(ILobby lobby, ICoH3Game game) {

    public Guid MatchId { get; } = Guid.CreateVersion7();

    public Task<string> BuildMatchData() => Task.Run(() => {
        LuaSourceFileBuilder luaSourceFileBuilder = new();
        luaSourceFileBuilder
            .DeclareGlobal("match_id", MatchId.ToString())
            .DeclareTable("teams", table =>
                table.AddNestedTable(teamTable => BuildTeamData(teamTable, 1, lobby.Team1))
                    .AddNestedTable(teamTable => BuildTeamData(teamTable, 2, lobby.Team2)))
            .DeclareTable("companies", table => {
                foreach (var company in lobby.Companies) {
                    table.AddNestedTable(company.Key, subTable => BuildCompanyData(subTable, company.Value));
                }
            })
            .DeclareGlobal("bg_is_dev", false)
            .DeclareGlobal("bg_is_realistic_damage_model", false)  // TODO: Get from game settings
            .DeclareGlobal("bg_is_supply_mode", false); // TODO: Get from game settings
        return luaSourceFileBuilder.ToString();
    });

    public async Task<bool> WriteMatchData(string matchData) {
        try {
            using var fs = File.Open(game.MatchDataPath, FileMode.Create);
            using var writer = new StreamWriter(fs, Consts.UTF8);
            await writer.WriteAsync(matchData);
        } catch (IOException ex) {
            throw new Exception($"Failed to create match data file: {ex.Message}", ex);
        }
        return true;
    }

    private void BuildTeamData(LuaSourceFileBuilder.TableBuilder table, int teamNumber, Team team) {
        table.AddFieldValue("team", teamNumber)
            .AddFieldValue("team_name", team.TeamAlias)
            .AddNestedFieldTable("players", playersTable => {
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
        table
            .AddFieldValue("id", participant.LobbyId)
            .AddFieldValue("name", participant.ParticipantName)
            .AddFieldValue("faction", slot.Faction)
            .AddFieldValue("difficulty", (byte)slot.Difficulty)
            .AddFieldValue("company", slot.CompanyId);
    }

    private void BuildCompanyData(LuaSourceFileBuilder.TableBuilder table, Company company) {
        table.AddFieldValue("name", company.Name);
        table.AddNestedFieldTable("units", squadsTable => {
            foreach (var squad in company.Squads) {
                squadsTable.AddNestedTable(x => BuildCompanySquadData(x, squad));
            }
        });
    }

    private void BuildCompanySquadData(LuaSourceFileBuilder.TableBuilder table, Squad squad) {
        if (squad.HasCustomName) {
            table.AddFieldValue("name", squad.Name);
        }
        table.AddFieldValue("experience", squad.Experience)
            .AddFieldValue("rank", squad.Rank) // Rank is calculated based on experience and needed for the ingame UI to display the correct rank icon
            .AddFieldValue("blueprint", squad.Blueprint.Id)
            .AddFieldValue("phase", (int)squad.Phase) // Phase is an enum, but we store it as an int for Lua compatibility
            .AddFieldValue("category", (byte)squad.Blueprint.Category)
            .AddNestedFieldTable("cost", subTable => BuildCostData(subTable, squad.Blueprint.Cost)); // TODO: Make cost calculation based on transport and upgrades
        var items = from item in squad.SlotItems
                       where item.UpgradeBlueprint != null
                       select item.UpgradeBlueprint;
        var upgradeList = squad.Upgrades.Concat(items).ToList();
        if (upgradeList.Count > 0) {
            table.AddNestedFieldTable("upgrades", upgradeTable => {
                foreach (var upgrade in upgradeList) {
                    upgradeTable.AddValue(upgrade.Id);
                }
            });
        }
        if (squad.HasTransport) {
            table.AddNestedFieldTable("transport", transportTable => {
                var transport = squad.Transport!;
                transportTable.AddFieldValue("blueprint", transport.TransportBlueprint.Id)
                    .AddFieldValue("dropoff", transport.DropOffOnly);
            });
        }
    }

    private static void BuildCostData(LuaSourceFileBuilder.TableBuilder table, CostExtension cost) 
        => table.AddFieldValue("manpower", cost.Manpower)
            .AddFieldValue("fuel", cost.Fuel)
            .AddFieldValue("munitions", cost.Munitions);

}
