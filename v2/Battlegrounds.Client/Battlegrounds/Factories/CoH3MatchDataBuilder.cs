using System.IO;

using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Factories;

/// <summary>
/// Provides functionality to build and write Company of Heroes 3 match data in Lua format, including teams, companies,
/// and global match settings.
/// </summary>
/// <param name="lobby">The lobby instance containing team, company, and participant information for the match. Cannot be null.</param>
/// <param name="game">The game context providing access to match-specific settings and file paths. Cannot be null.</param>
public sealed class CoH3MatchDataBuilder(ILobby lobby, ICoH3Game game) {

    /// <summary>
    /// Gets the unique identifier for the match.
    /// </summary>
    public Guid MatchId { get; } = Guid.CreateVersion7();

    /// <summary>
    /// Asynchronously builds and returns a Lua-formatted string containing match data, including teams, companies, and
    /// global match settings.
    /// </summary>
    /// <remarks>The returned Lua string includes information about the match ID, teams, companies, and
    /// several global flags. The method executes on a background thread.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string with the serialized match
    /// data in Lua format.</returns>
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

    /// <summary>
    /// Asynchronously writes the specified match data to the match data file, overwriting any existing content.
    /// </summary>
    /// <param name="matchData">The match data to write to the file. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the data was
    /// written successfully.</returns>
    /// <exception cref="Exception">Thrown if an I/O error occurs while creating or writing to the match data file.</exception>
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
            .AddFieldValue("faction", MapCompanyFaction(slot.Faction))
            .AddFieldValue("difficulty", (byte)slot.Difficulty)
            .AddFieldValue("company", slot.CompanyId);
    }

    private static string MapCompanyFaction(string faction) => faction switch { // Too lazy to fix elsewhere, so just mapping here.
        "german" => "germans",
        _ => faction
    };

    private void BuildCompanyData(LuaSourceFileBuilder.TableBuilder table, Company company) {
        table.AddFieldValue("name", company.Name);
        table.AddNestedFieldTable("units", squadsTable => {
            HashSet<int> carriedSquads = [..(from squad in company.Squads
                                             where squad.HasPassenger
                                             select squad.Passenger!.PassengerSquadId)];
            foreach (var squad in company.Squads) {
                squadsTable.AddNestedTable(squad.Id, x => BuildCompanySquadData(x, squad, company, !carriedSquads.Contains(squad.Id)));
            }
        });
    }

    private void BuildCompanySquadData(LuaSourceFileBuilder.TableBuilder table, Squad squad, Company company, bool isVisible) {
        if (squad.HasCustomName) {
            table.AddFieldValue("name", squad.Name);
        }
        table.AddFieldValue("experience", squad.Experience)
            .AddFieldValue("rank", squad.Rank) // Rank is calculated based on experience and needed for the ingame UI to display the correct rank icon
            .AddFieldValue("blueprint", squad.Blueprint.Id)
            .AddFieldValue("phase", (int)squad.Phase) // Phase is an enum, but we store it as an int for Lua compatibility
            .AddFieldValue("category", (byte)squad.Blueprint.Category)
            .AddFieldValue("visible", isVisible) // If the squad is a passenger, it will be hidden in the UI and only visible when inspecting the transport, so we need to mark it as not visible in the match data
            .AddNestedFieldTable("cost", subTable => BuildCostData(subTable, squad.Blueprint.Cost)); // TODO: Make cost calculation based on transport and upgrades

        // Write upgrades table
        if (squad.Upgrades.Count > 0) {
            table.AddNestedFieldTable("upgrades", upgradeTable => {
                foreach (var upgrade in squad.Upgrades) {
                    upgradeTable.AddValue(upgrade.Id);
                }
            });
        }

        // Write slot items table. In CoH3, slot items can only be entities, so we only write the entity blueprint ID for each slot item.
        var slotItems = from item in squad.SlotItems
                        where item.EntityBlueprint != null // In CoH3, slot items can only be entities
                        select item;
        if (squad.SlotItems.Count > 0) {
            table.AddNestedFieldTable("items", itemsTable => {
                foreach (var item in squad.SlotItems) {
                    var companyItem = company.CapturedItems.FirstOrDefault(x => x.Id == item.CompanyItemId);
                    if (companyItem?.ItemBlueprint is not null) {
                        itemsTable.AddValue(companyItem.ItemBlueprint.Id);
                    }
                }
            });
        }

        // Write transport data if the squad has a transport.
        if (squad.HasTransport) {
            table.AddNestedFieldTable("transport", transportTable => {
                var transport = squad.Transport!;
                transportTable.AddFieldValue("blueprint", transport.TransportBlueprint.Id)
                    .AddFieldValue("dropoff", transport.DropOffOnly);
            });
        }

        // Write passenger data if the squad has a passenger.
        if (squad.HasPassenger) {
            table.AddFieldValue("passenger", squad.Passenger.PassengerSquadId); // Company Squad Id of the passenger squad. The actual passenger squad data will be written as a separate squad in the squads table, but it will be marked as not visible since it's a passenger and only visible when inspecting the transport.
        }

        // Write capture data
        if (squad.IsCapturedWeapon) {
            table.AddFieldValue("capture_weapon", squad.CapturedWeapon.WeaponEntityBlueprint!.Id); // Entity blueprint ID of the captured weapon to spawn
            table.AddFieldValue("crew_blueprint", squad.CapturedWeapon.CrewBlueprint!.Id); // Squad blueprint ID of the crew for the captured weapon (determines visually, what soldier models are used)
        }

    }

    private static void BuildCostData(LuaSourceFileBuilder.TableBuilder table, CostExtension cost) 
        => table.AddFieldValue("manpower", cost.Manpower)
            .AddFieldValue("fuel", cost.Fuel)
            .AddFieldValue("munitions", cost.Munitions);

}
