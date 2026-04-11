using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using NSubstitute;

namespace Battlegrounds.Test.Models.Lobbies;

/// <summary>
/// Unit tests for <see cref="SingleplayerLobby"/>.
/// Validates lobby state management, participant handling, slot configuration,
/// event emission, and full singleplayer lobby flow.
/// </summary>
[TestFixture]
[TestOf(typeof(SingleplayerLobby))]
public sealed class SingleplayerLobbyTests {

    // ── Shared infrastructure ────────────────────────────────────────────────

    private static readonly Map DefaultMap = new("TestMap", "A test map", 4, "preview", "test_map_4p");
    private static readonly Map SmallMap = new("SmallMap", "A small 1v1 map", 2, "preview_small", "small_map_2p");
    private static readonly Map LargeMap = new("LargeMap", "A large 4v4 map", 8, "preview_large", "large_map_8p");

    private static Game CreateMockGame() {
        var game = Substitute.For<Game>();
        game.Id.Returns("CoH3");
        game.FactionIds.Returns(["british_africa", "germans"]);
        game.GetFactionAlliance("british_africa").Returns(FactionAlliance.Allies);
        game.GetFactionAlliance("germans").Returns(FactionAlliance.Axis);
        return game;
    }

    private static LobbySetup CreateSetup(Participant? self = null, Map? map = null) {
        var game = CreateMockGame();
        self ??= new Participant(0, "local-player", "Player One", false, true);
        var team1Slots = new Team.Slot[4];
        var team2Slots = new Team.Slot[4];
        for (int i = 0; i < 4; i++) {
            string? pid = i == 0 ? self.ParticipantId : null;
            team1Slots[i] = new Team.Slot(i, pid, i == 0 ? "british_africa" : "", "", AIDifficulty.HUMAN, i >= 2, false);
            team2Slots[i] = new Team.Slot(i, null, "", "", AIDifficulty.HUMAN, i >= 2, false);
        }
        return new LobbySetup {
            Name = "Singleplayer Lobby",
            Self = self,
            Team1 = new Team(TeamType.Allies, "Allies", team1Slots),
            Team2 = new Team(TeamType.Axis, "Axis", team2Slots),
            Settings = [
                new LobbySetting {
                    Name = LobbySetting.SETTING_GAMEMODE,
                    Type = LobbySettingType.Selection,
                    Options = [new("Domination", "domination"), new("Victory Points", "victory_points")]
                }
            ],
            Game = game,
            Map = map ?? DefaultMap,
            Participants = [self]
        };
    }

    private static SingleplayerLobby CreateLobby(LobbySetup? setup = null) {
        setup ??= CreateSetup();
        return new SingleplayerLobby(
            setup.Value,
            Substitute.For<IBattlegroundsServerAPI>(),
            Substitute.For<ICompanyService>());
    }

    /// <summary>
    /// Reads the next lobby event with a timeout to avoid hanging tests.
    /// </summary>
    private static async Task<LobbyEvent?> ConsumeEventAsync(SingleplayerLobby lobby, int timeoutMs = 1000) {
        using var cts = new CancellationTokenSource(timeoutMs);
        try {
            var task = lobby.GetNextEvent();
            if (task.IsCompleted)
                return task.Result;
            return await task.AsTask().WaitAsync(cts.Token);
        } catch (OperationCanceledException) {
            return null;
        }
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Test]
    public void Name_ReflectsSetup() {
        using var lobby = CreateLobby();
        Assert.That(lobby.Name, Is.EqualTo("Singleplayer Lobby"));
    }

    [Test]
    public void IsHost_IsAlwaysTrue() {
        using var lobby = CreateLobby();
        Assert.That(lobby.IsHost, Is.True);
    }

    [Test]
    public void IsActive_IsTrueAfterConstruction() {
        using var lobby = CreateLobby();
        Assert.That(lobby.IsActive, Is.True);
    }

    [Test]
    public void IsReady_IsAlwaysTrue() {
        using var lobby = CreateLobby();
        Assert.That(lobby.IsReady, Is.True);
    }

    [Test]
    public void Teams_AreInitializedFromSetup() {
        using var lobby = CreateLobby();

        using (Assert.EnterMultipleScope()) {
            Assert.That(lobby.Team1.TeamType, Is.EqualTo(TeamType.Allies));
            Assert.That(lobby.Team2.TeamType, Is.EqualTo(TeamType.Axis));
            Assert.That(lobby.Team1.Slots, Has.Length.EqualTo(4));
            Assert.That(lobby.Team2.Slots, Has.Length.EqualTo(4));
        }
    }

    [Test]
    public void Map_IsInitializedFromSetup() {
        using var lobby = CreateLobby();
        Assert.That(lobby.Map, Is.EqualTo(DefaultMap));
    }

    [Test]
    public void Game_IsInitializedFromSetup() {
        var setup = CreateSetup();
        using var lobby = CreateLobby(setup);
        Assert.That(lobby.Game, Is.SameAs(setup.Game));
    }

    [Test]
    public void Companies_IsEmptyInitially() {
        using var lobby = CreateLobby();
        Assert.That(lobby.Companies, Is.Empty);
    }

    [Test]
    public void Settings_AreInitializedFromSetup() {
        using var lobby = CreateLobby();

        Assert.That(lobby.Settings, Has.Count.EqualTo(1));
        Assert.That(lobby.Settings[0].Name, Is.EqualTo(LobbySetting.SETTING_GAMEMODE));
    }

    [Test]
    public void Participants_ContainsLocalPlayer() {
        using var lobby = CreateLobby();

        Assert.That(lobby.Participants, Has.Count.EqualTo(1));
        Assert.That(lobby.Participants.Any(p => p.ParticipantId == "local-player"), Is.True);
    }

    [Test]
    public void GetRealPlayersCount_AlwaysReturnsOne() {
        using var lobby = CreateLobby();
        Assert.That(lobby.GetRealPlayersCount(), Is.EqualTo(1));
    }

    // ── Constructor validation ────────────────────────────────────────────────

    [Test]
    public void Constructor_ThrowsOnNullServerAPI() {
        var setup = CreateSetup();
        Assert.That(() => new SingleplayerLobby(setup, null!, Substitute.For<ICompanyService>()), Throws.ArgumentNullException);
    }

    [Test]
    public void Constructor_ThrowsOnNullCompanyService() {
        var setup = CreateSetup();
        Assert.That(() => new SingleplayerLobby(setup, Substitute.For<IBattlegroundsServerAPI>(), null!), Throws.ArgumentNullException);
    }

    // ── GetLocalPlayerId / GetLocalPlayerSlot ─────────────────────────────────

    [Test]
    public void GetLocalPlayerId_ReturnsParticipantId() {
        using var lobby = CreateLobby();
        Assert.That(lobby.GetLocalPlayerId(), Is.EqualTo("local-player"));
    }

    [Test]
    public void GetLocalPlayerSlot_ReturnsCorrectTeamAndIndex() {
        using var lobby = CreateLobby();

        var (team, slotId) = lobby.GetLocalPlayerSlot();

        using (Assert.EnterMultipleScope()) {
            Assert.That(team, Is.SameAs(lobby.Team1));
            Assert.That(slotId, Is.EqualTo(0));
        }
    }

    // ── GetParticipant ────────────────────────────────────────────────────────

    [Test]
    public void GetParticipant_ReturnsParticipant_WhenFound() {
        using var lobby = CreateLobby();

        var p = lobby.GetParticipant("local-player");

        Assert.That(p, Is.Not.Null);
        Assert.That(p!.ParticipantName, Is.EqualTo("Player One"));
    }

    [Test]
    public void GetParticipant_ReturnsNull_WhenNotFound() {
        using var lobby = CreateLobby();
        Assert.That(lobby.GetParticipant("does-not-exist"), Is.Null);
    }

    // ── SetCompany ────────────────────────────────────────────────────────────

    [Test]
    public async Task SetCompany_UpdatesSlotCompanyIdAndFaction() {
        using var lobby = CreateLobby();

        await lobby.SetCompany(lobby.Team1, 0, "company-1", "british_africa");

        using (Assert.EnterMultipleScope()) {
            Assert.That(lobby.Team1.Slots[0].CompanyId, Is.EqualTo("company-1"));
            Assert.That(lobby.Team1.Slots[0].Faction, Is.EqualTo("british_africa"));
        }
    }

    [Test]
    public async Task SetCompany_EmitsTeamUpdatedEvent() {
        using var lobby = CreateLobby();

        await lobby.SetCompany(lobby.Team1, 0, "c1", "british_africa");

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev, Is.Not.Null);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.TeamUpdated));
        Assert.That(ev.Arg, Is.EqualTo(TeamType.Allies));
    }

    [Test]
    public async Task SetCompany_OnTeam2_EmitsAxisTeamType() {
        using var lobby = CreateLobby();

        await lobby.SetCompany(lobby.Team2, 0, "c2", "germans");

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.Arg, Is.EqualTo(TeamType.Axis));
    }

    // ── SetSlotAIDifficulty ──────────────────────────────────────────────────

    [Test]
    public async Task SetSlotAIDifficulty_CreatesAIParticipant() {
        using var lobby = CreateLobby();

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EASY);

        using (Assert.EnterMultipleScope()) {
            Assert.That(lobby.Team2.Slots[0].Difficulty, Is.EqualTo(AIDifficulty.EASY));
            Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Not.Null.And.Not.Empty);
            Assert.That(lobby.Participants, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task SetSlotAIDifficulty_EmitsTeamUpdatedEvent() {
        using var lobby = CreateLobby();

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.NORMAL);

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.TeamUpdated));
    }

    [Test]
    public async Task SetSlotAIDifficulty_SettingToHuman_RemovesAI() {
        using var lobby = CreateLobby();

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.HARD);
        await ConsumeEventAsync(lobby);

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.HUMAN);
        await ConsumeEventAsync(lobby);

        using (Assert.EnterMultipleScope()) {
            Assert.That(lobby.Team2.Slots[0].Difficulty, Is.EqualTo(AIDifficulty.HUMAN));
            Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Null);
            Assert.That(lobby.Participants, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task SetSlotAIDifficulty_ChangingDifficulty_ReusesExistingParticipant() {
        using var lobby = CreateLobby();

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EASY);
        await ConsumeEventAsync(lobby);
        var originalPid = lobby.Team2.Slots[0].ParticipantId;

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EXPERT);
        await ConsumeEventAsync(lobby);

        Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.EqualTo(originalPid));
        Assert.That(lobby.Participants, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task SetSlotAIDifficulty_MultipleSlots_CreatesDistinctParticipants() {
        using var lobby = CreateLobby();

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EASY);
        await ConsumeEventAsync(lobby);
        await lobby.SetSlotAIDifficulty(lobby.Team2, 1, AIDifficulty.HARD);
        await ConsumeEventAsync(lobby);

        Assert.That(lobby.Participants, Has.Count.EqualTo(3));
        Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Not.EqualTo(lobby.Team2.Slots[1].ParticipantId));
    }

    // ── RemoveAI ─────────────────────────────────────────────────────────────

    [Test]
    public async Task RemoveAI_ClearsSlotAndRemovesParticipant() {
        using var lobby = CreateLobby();

        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.HARD);
        await ConsumeEventAsync(lobby);

        await lobby.RemoveAI(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);

        using (Assert.EnterMultipleScope()) {
            Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Null);
            Assert.That(lobby.Team2.Slots[0].Difficulty, Is.EqualTo(AIDifficulty.HUMAN));
            Assert.That(lobby.Participants, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task RemoveAI_DoesNotRemoveLocalPlayer() {
        using var lobby = CreateLobby();

        await lobby.RemoveAI(lobby.Team1, 0);

        Assert.That(lobby.Team1.Slots[0].ParticipantId, Is.EqualTo("local-player"));
        Assert.That(lobby.Participants, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task RemoveAI_OnEmptySlot_DoesNothing() {
        using var lobby = CreateLobby();

        await lobby.RemoveAI(lobby.Team2, 0);

        Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Null);
    }

    // ── ToggleSlotLock ───────────────────────────────────────────────────────

    [Test]
    public async Task ToggleSlotLock_LocksUnlockedSlot() {
        using var lobby = CreateLobby();
        Assert.That(lobby.Team2.Slots[0].Locked, Is.False);

        await lobby.ToggleSlotLock(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);

        Assert.That(lobby.Team2.Slots[0].Locked, Is.True);
    }

    [Test]
    public async Task ToggleSlotLock_UnlocksLockedSlot() {
        using var lobby = CreateLobby();

        await lobby.ToggleSlotLock(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);
        await lobby.ToggleSlotLock(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);

        Assert.That(lobby.Team2.Slots[0].Locked, Is.False);
    }

    [Test]
    public async Task ToggleSlotLock_EmitsTeamUpdatedEvent() {
        using var lobby = CreateLobby();

        await lobby.ToggleSlotLock(lobby.Team1, 1);

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.TeamUpdated));
        Assert.That(ev.Arg, Is.EqualTo(TeamType.Allies));
    }

    // ── SetMap ────────────────────────────────────────────────────────────────

    [Test]
    public async Task SetMap_ReturnsTrueAndUpdatesMap() {
        using var lobby = CreateLobby();
        var newMap = new Map("NewMap", "Description", 4, "preview", "new_map_4p");

        var result = await lobby.SetMap(newMap);

        Assert.That(result, Is.True);
        Assert.That(lobby.Map, Is.EqualTo(newMap));
    }

    [Test]
    public async Task SetMap_SameMap_ReturnsTrueWithoutEvent() {
        using var lobby = CreateLobby();

        var result = await lobby.SetMap(DefaultMap);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SetMap_EmitsMapUpdatedEvent() {
        using var lobby = CreateLobby();
        var newMap = new Map("NewMap", "Description", 4, "preview", "new_map_4p");

        await lobby.SetMap(newMap);

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.MapUpdated));
        Assert.That(ev.Arg, Is.EqualTo(newMap));
    }

    [Test]
    public async Task SetMap_DifferentPlayerCount_HidesExcessSlots() {
        using var lobby = CreateLobby();

        var result = await lobby.SetMap(SmallMap);

        Assert.That(result, Is.True);
        Assert.That(lobby.Team1.Slots[0].Hidden, Is.False);
        Assert.That(lobby.Team1.Slots[1].Hidden, Is.True);
        Assert.That(lobby.Team2.Slots[0].Hidden, Is.False);
        Assert.That(lobby.Team2.Slots[1].Hidden, Is.True);
    }

    [Test]
    public async Task SetMap_DifferentPlayerCount_EmitsTeamUpdatedThenMapUpdated() {
        using var lobby = CreateLobby();

        await lobby.SetMap(LargeMap);

        var first = await ConsumeEventAsync(lobby);
        var second = await ConsumeEventAsync(lobby);

        Assert.That(first!.EventType, Is.EqualTo(LobbyEventType.TeamUpdated));
        Assert.That(second!.EventType, Is.EqualTo(LobbyEventType.MapUpdated));
    }

    [Test]
    public async Task SetMap_ReturnsFalse_WhenMaxPlayersLessThanParticipants() {
        using var lobby = CreateLobby();

        // Add 2 AIs so total participants = 3
        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EASY);
        await ConsumeEventAsync(lobby);
        await lobby.SetSlotAIDifficulty(lobby.Team2, 1, AIDifficulty.EASY);
        await ConsumeEventAsync(lobby);

        var result = await lobby.SetMap(SmallMap); // supports 2

        Assert.That(result, Is.False);
        Assert.That(lobby.Map, Is.EqualTo(DefaultMap)); // unchanged
    }

    // ── SetSetting ───────────────────────────────────────────────────────────

    [Test]
    public async Task SetSetting_UpdatesExistingSetting() {
        using var lobby = CreateLobby();

        var updated = new LobbySetting { Name = LobbySetting.SETTING_GAMEMODE, Type = LobbySettingType.Selection, Value = 1 };
        await lobby.SetSetting(updated);

        Assert.That(lobby.Settings[0].Value, Is.EqualTo(1));
    }

    [Test]
    public async Task SetSetting_AddsNewSetting() {
        using var lobby = CreateLobby();

        var newSetting = new LobbySetting { Name = "new_setting", Type = LobbySettingType.Boolean, Value = 1 };
        await lobby.SetSetting(newSetting);

        Assert.That(lobby.Settings, Has.Count.EqualTo(2));
        Assert.That(lobby.Settings.Any(s => s.Name == "new_setting"), Is.True);
    }

    [Test]
    public async Task SetSetting_EmitsSettingUpdatedEvent() {
        using var lobby = CreateLobby();

        await lobby.SetSetting(new LobbySetting { Name = "x", Type = LobbySettingType.Boolean });

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.SettingUpdated));
    }

    // ── SendMessage ──────────────────────────────────────────────────────────

    [Test]
    public async Task SendMessage_EmitsParticipantMessageEvent() {
        using var lobby = CreateLobby();

        await lobby.SendMessage(ChatChannel.All, "Hello!");

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.ParticipantMessage));

        var msg = ev.Arg as ChatMessage;
        using (Assert.EnterMultipleScope()) {
            Assert.That(msg, Is.Not.Null);
            Assert.That(msg!.Message, Is.EqualTo("Hello!"));
            Assert.That(msg.SenderId, Is.EqualTo("local-player"));
            Assert.That(msg.Sender, Is.EqualTo("Player One"));
            Assert.That(msg.Channel, Is.EqualTo(ChatChannel.All));
        }
    }

    [Test]
    public async Task SendMessage_TeamChannel_SetsCorrectChannel() {
        using var lobby = CreateLobby();

        await lobby.SendMessage(ChatChannel.Team, "Push right!");

        var ev = await ConsumeEventAsync(lobby);
        var msg = (ChatMessage)ev!.Arg!;
        Assert.That(msg.Channel, Is.EqualTo(ChatChannel.Team));
    }

    // ── MoveToSlot ───────────────────────────────────────────────────────────

    [Test]
    public async Task MoveToSlot_MovesPlayerToNewSlotOnSameTeam() {
        using var lobby = CreateLobby();

        await lobby.MoveToSlot(lobby.Team1, 1);
        await ConsumeEventAsync(lobby);

        using (Assert.EnterMultipleScope()) {
            Assert.That(lobby.Team1.Slots[0].ParticipantId, Is.Null);
            Assert.That(lobby.Team1.Slots[1].ParticipantId, Is.EqualTo("local-player"));
        }
    }

    [Test]
    public async Task MoveToSlot_MovesPlayerToDifferentTeam() {
        using var lobby = CreateLobby();

        await lobby.MoveToSlot(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);

        Assert.That(lobby.GetLocalPlayerSlot(), Is.EqualTo((lobby.Team2, 0)));
        Assert.That(lobby.Team1.Slots[0].ParticipantId, Is.Null);
    }

    [Test]
    public async Task MoveToSlot_SamePosition_DoesNothing() {
        using var lobby = CreateLobby();

        await lobby.MoveToSlot(lobby.Team1, 0);

        // No event should be written; verify slot unchanged
        Assert.That(lobby.Team1.Slots[0].ParticipantId, Is.EqualTo("local-player"));
    }

    // ── BeginMatch / EndMatch ─────────────────────────────────────────────────

    [Test]
    public async Task BeginMatch_CompletesWithoutError() {
        using var lobby = CreateLobby();
        await lobby.BeginMatch(); // NOP in singleplayer
    }

    [Test]
    public async Task EndMatch_WithSuccess_EmitsMatchOverEvent() {
        using var lobby = CreateLobby();

        await lobby.EndMatch(EndMatchReason.MatchEndedInSuccess);

        var ev = await ConsumeEventAsync(lobby);
        Assert.That(ev!.EventType, Is.EqualTo(LobbyEventType.MatchOver));
        Assert.That(ev.Arg, Is.EqualTo(EndMatchReason.MatchEndedInSuccess));
    }

    [Test]
    public async Task EndMatch_WithNonSuccess_DoesNotEmitEvent() {
        using var lobby = CreateLobby();

        await lobby.EndMatch(EndMatchReason.GameCancelled);

        var ev = await ConsumeEventAsync(lobby, timeoutMs: 200);
        Assert.That(ev, Is.Null);
    }

    // ── NOP operations in singleplayer ────────────────────────────────────────

    [Test]
    public async Task LaunchGame_ReturnsNonFailed() {
        using var lobby = CreateLobby();
        var result = await lobby.LaunchGame();
        Assert.That(result.Failed, Is.False);
    }

    [Test]
    public async Task UploadGamemode_ReturnsNonFailed() {
        using var lobby = CreateLobby();
        var result = await lobby.UploadGamemode("some/path");
        Assert.That(result.Failed, Is.False);
    }

    [Test]
    public async Task WaitForAllPlayersHaveGamemode_ReturnsTrue() {
        using var lobby = CreateLobby();
        Assert.That(await lobby.WaitForAllPlayersHaveGamemode(), Is.True);
    }

    [Test]
    public async Task MarkReady_IsNoOp() {
        using var lobby = CreateLobby();
        await lobby.MarkReady(true);
        Assert.That(lobby.IsReady, Is.True); // unchanged
    }

    [Test]
    public async Task KickPlayer_IsNoOp() {
        using var lobby = CreateLobby();
        await lobby.KickPlayer(lobby.Team1, 0);
        Assert.That(lobby.Team1.Slots[0].ParticipantId, Is.EqualTo("local-player"));
    }

    [Test]
    public async Task SetSlotFaction_IsNoOp() {
        using var lobby = CreateLobby();
        var originalFaction = lobby.Team1.Slots[0].Faction;

        await lobby.SetSlotFaction(lobby.Team1, 0, "germans");

        Assert.That(lobby.Team1.Slots[0].Faction, Is.EqualTo(originalFaction));
    }

    [Test]
    public async Task PublishSystemMessage_IsNoOp() {
        using var lobby = CreateLobby();
        await lobby.PublishSystemMessage("system msg"); // should not throw
    }

    // ── GetMatchResults ──────────────────────────────────────────────────────

    [Test]
    public async Task GetMatchResults_ReturnsNull_WhenNoMatchPlayed() {
        using var lobby = CreateLobby();
        Assert.That(await lobby.GetMatchResults(), Is.Null);
    }

    // ── Dispose ──────────────────────────────────────────────────────────────

    [Test]
    public void Dispose_SetsIsActiveToFalse() {
        var lobby = CreateLobby();
        lobby.Dispose();
        Assert.That(lobby.IsActive, Is.False);
    }

    [Test]
    public async Task Dispose_ClosesEventChannel_GetNextEventReturnsNull() {
        var lobby = CreateLobby();
        lobby.Dispose();

        var ev = await lobby.GetNextEvent();
        Assert.That(ev, Is.Null);
    }

    [Test]
    public void Dispose_CanBeCalledTwice() {
        var lobby = CreateLobby();
        lobby.Dispose();
        Assert.That(() => lobby.Dispose(), Throws.Nothing);
    }

    // ── Complete lobby flows ─────────────────────────────────────────────────

    [Test]
    public async Task FullFlow_SetupConfigureAndStartMatch() {
        using var lobby = CreateLobby();

        // 1. Verify initial state
        Assert.That(lobby.IsHost, Is.True);
        Assert.That(lobby.IsActive, Is.True);

        // 2. Add AI opponent
        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.HARD);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.Participants, Has.Count.EqualTo(2));

        // 3. Set companies
        await lobby.SetCompany(lobby.Team1, 0, "player-co", "british_africa");
        await ConsumeEventAsync(lobby);
        await lobby.SetCompany(lobby.Team2, 0, "ai-co", "germans");
        await ConsumeEventAsync(lobby);

        // 4. Verify both teams have occupied slots with companies
        Assert.That(lobby.Team1.Slots[0].CompanyId, Is.Not.Empty);
        Assert.That(lobby.Team2.Slots[0].CompanyId, Is.Not.Empty);

        // 5. Change map
        var newMap = new Map("BattleMap", "Great map", 4, "preview", "battle_map");
        Assert.That(await lobby.SetMap(newMap), Is.True);
        await ConsumeEventAsync(lobby); // MapUpdated event

        // 6. Adjust a setting
        await lobby.SetSetting(new LobbySetting { Name = LobbySetting.SETTING_GAMEMODE, Type = LobbySettingType.Selection, Value = 1 });
        await ConsumeEventAsync(lobby);

        // 7. Start match pipeline (all NOP in singleplayer but should succeed)
        await lobby.BeginMatch();
        var launch = await lobby.LaunchGame();
        Assert.That(launch.Failed, Is.False);
        var upload = await lobby.UploadGamemode("path");
        Assert.That(upload.Failed, Is.False);
        Assert.That(await lobby.WaitForAllPlayersHaveGamemode(), Is.True);

        // 8. End match
        await lobby.EndMatch(EndMatchReason.MatchEndedInSuccess);
        var matchOverEvent = await ConsumeEventAsync(lobby);
        Assert.That(matchOverEvent!.EventType, Is.EqualTo(LobbyEventType.MatchOver));
    }

    [Test]
    public async Task FullFlow_AddAndRemoveMultipleAI() {
        using var lobby = CreateLobby();

        // Add two AIs
        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EASY);
        await ConsumeEventAsync(lobby);
        await lobby.SetSlotAIDifficulty(lobby.Team2, 1, AIDifficulty.HARD);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.Participants, Has.Count.EqualTo(3));

        // Remove one
        await lobby.RemoveAI(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.Participants, Has.Count.EqualTo(2));
        Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Null);
        Assert.That(lobby.Team2.Slots[1].Difficulty, Is.EqualTo(AIDifficulty.HARD));

        // Re-add with different difficulty
        await lobby.SetSlotAIDifficulty(lobby.Team2, 0, AIDifficulty.EXPERT);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.Participants, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task FullFlow_MoveBetweenTeams() {
        using var lobby = CreateLobby();

        Assert.That(lobby.GetLocalPlayerSlot(), Is.EqualTo((lobby.Team1, 0)));

        // Move to Team2
        await lobby.MoveToSlot(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.GetLocalPlayerSlot(), Is.EqualTo((lobby.Team2, 0)));
        Assert.That(lobby.Team1.Slots[0].ParticipantId, Is.Null);

        // Move back to Team1 slot 1
        await lobby.MoveToSlot(lobby.Team1, 1);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.GetLocalPlayerSlot(), Is.EqualTo((lobby.Team1, 1)));
        Assert.That(lobby.Team2.Slots[0].ParticipantId, Is.Null);
    }

    [Test]
    public async Task FullFlow_LockAndUnlockSlots() {
        using var lobby = CreateLobby();

        await lobby.ToggleSlotLock(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.Team2.Slots[0].Locked, Is.True);

        await lobby.ToggleSlotLock(lobby.Team2, 0);
        await ConsumeEventAsync(lobby);
        Assert.That(lobby.Team2.Slots[0].Locked, Is.False);
    }

    [Test]
    public async Task FullFlow_ChatMessagesAccumulate() {
        using var lobby = CreateLobby();

        await lobby.SendMessage(ChatChannel.All, "Hello");
        var ev1 = await ConsumeEventAsync(lobby);

        await lobby.SendMessage(ChatChannel.Team, "Team msg");
        var ev2 = await ConsumeEventAsync(lobby);

        await lobby.SendMessage(ChatChannel.All, "GG");
        var ev3 = await ConsumeEventAsync(lobby);

        Assert.That(((ChatMessage)ev1!.Arg!).Message, Is.EqualTo("Hello"));
        Assert.That(((ChatMessage)ev2!.Arg!).Channel, Is.EqualTo(ChatChannel.Team));
        Assert.That(((ChatMessage)ev3!.Arg!).Message, Is.EqualTo("GG"));
    }

    [Test]
    public async Task FullFlow_MapChangeResizesSlots() {
        using var lobby = CreateLobby();

        // Default 4-player map (2 per team), slots 2-3 hidden
        Assert.That(lobby.Team1.Slots[0].Hidden, Is.False);
        Assert.That(lobby.Team1.Slots[1].Hidden, Is.False);
        Assert.That(lobby.Team1.Slots[2].Hidden, Is.True);

        // Switch to 8-player map
        await lobby.SetMap(LargeMap);
        await ConsumeEventAsync(lobby); // TeamUpdated
        await ConsumeEventAsync(lobby); // MapUpdated

        Assert.That(lobby.Team1.Slots[2].Hidden, Is.False);
        Assert.That(lobby.Team1.Slots[3].Hidden, Is.False);

        // Switch back to 4-player map
        await lobby.SetMap(DefaultMap);
        await ConsumeEventAsync(lobby); // TeamUpdated
        await ConsumeEventAsync(lobby); // MapUpdated

        Assert.That(lobby.Team1.Slots[2].Hidden, Is.True);
        Assert.That(lobby.Team1.Slots[3].Hidden, Is.True);
    }

}
