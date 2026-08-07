using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Replays;
using Battlegrounds.Test.Models.Blueprints;

namespace Battlegrounds.Test.Models.Lobbies;

[TestFixture]
public sealed class MatchOverDataTests {

    private const string PlayerId = "player1";
    private const string CompanyId = "company1";

    private static MatchResult BuildValidResult(
        string matchId = "match1",
        string gameId = "game1",
        string scenario = "test_map",
        TimeSpan? duration = null,
        bool concluded = true,
        IReadOnlySet<string>? winners = null,
        IReadOnlySet<string>? losers = null,
        IReadOnlyDictionary<string, string>? playerCompanies = null,
        IReadOnlyDictionary<string, LinkedList<CompanyEventModifier>>? companyModifiers = null,
        IReadOnlyList<BadMatchEvent>? badEvents = null) => new MatchResult {
            MatchId = matchId,
            GameId = gameId,
            Scenario = scenario,
            MatchDuration = duration ?? TimeSpan.FromMinutes(30),
            Concluded = concluded,
            Winners = winners ?? new HashSet<string>(),
            Losers = losers ?? new HashSet<string>(),
            PlayerCompanies = playerCompanies ?? new Dictionary<string, string>(),
            CompanyModifiers = companyModifiers ?? new Dictionary<string, LinkedList<CompanyEventModifier>>(),
            BadEvents = badEvents ?? []
        };

    [Test]
    public void FromMatchResultForPlayer_ReturnsInvalid_WhenResultIsInvalid() {
        var data = MatchOverData.FromMatchResultForPlayer(MatchResult.Invalid, PlayerId);

        Assert.That(data.IsValid, Is.False);
    }

    [Test]
    public void FromMatchResultForPlayer_ReturnsInvalid_WhenPlayerIdIsEmpty() {
        var result = BuildValidResult();

        var data = MatchOverData.FromMatchResultForPlayer(result, string.Empty);

        Assert.That(data.IsValid, Is.False);
    }

    [Test]
    public void FromMatchResultForPlayer_MapsMatchMetadataCorrectly() {
        var expectedDuration = TimeSpan.FromMinutes(45);
        var result = BuildValidResult(
            matchId: "m123",
            gameId: "g456",
            scenario: "la_gleize",
            duration: expectedDuration,
            concluded: true);

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        using (Assert.EnterMultipleScope()) {
            Assert.That(data.MatchId, Is.EqualTo("m123"));
            Assert.That(data.GameId, Is.EqualTo("g456"));
            Assert.That(data.Scenario, Is.EqualTo("la_gleize"));
            Assert.That(data.MatchDuration, Is.EqualTo(expectedDuration));
            Assert.That(data.Concluded, Is.True);
            Assert.That(data.IsValid, Is.True);
        }
    }

    [Test]
    public void FromMatchResultForPlayer_SetsIsVictory_WhenPlayerIsWinner() {
        var result = BuildValidResult(winners: new HashSet<string> { PlayerId });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.IsVictory, Is.True);
    }

    [Test]
    public void FromMatchResultForPlayer_DoesNotSetIsVictory_WhenPlayerIsLoser() {
        var result = BuildValidResult(losers: new HashSet<string> { PlayerId });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.IsVictory, Is.False);
    }

    [Test]
    public void FromMatchResultForPlayer_SetsCompanyId_WhenPlayerHasCompany() {
        var result = BuildValidResult(
            playerCompanies: new Dictionary<string, string> { [PlayerId] = CompanyId });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.CompanyId, Is.EqualTo(CompanyId));
    }

    [Test]
    public void FromMatchResultForPlayer_SetsEmptyCompanyId_WhenPlayerHasNoCompany() {
        var result = BuildValidResult(playerCompanies: new Dictionary<string, string>());

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.CompanyId, Is.Empty);
    }

    [Test]
    public void FromMatchResultForPlayer_SetsHasBadEvents_WhenBadEventsExist() {
        var replayPlayer = new ReplayPlayer(1000, 0, 0, "Player", 0, 0, "british_africa", "default_ai");
        var badEvent = new BadMatchEvent(new SquadDeployedEvent(TimeSpan.Zero, replayPlayer, 1), "Test bad event");
        var result = BuildValidResult(badEvents: [badEvent]);

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.HasBadEvents, Is.True);
    }

    [Test]
    public void FromMatchResultForPlayer_DoesNotSetHasBadEvents_WhenNoBadEvents() {
        var result = BuildValidResult(badEvents: []);

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.HasBadEvents, Is.False);
    }

    [Test]
    public void FromMatchResultForPlayer_ReturnsEmptySquadSummaries_WhenPlayerHasNoModifiers() {
        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>>());

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        Assert.That(data.SquadSummaries, Is.Empty);
    }

    [Test]
    public void FromMatchResultForPlayer_AggregatesStatisticsModifier() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 5, vehiclesDestroyed: 2, losses: 1));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);
        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);

        using (Assert.EnterMultipleScope()) {
            Assert.That(squad.InfantryKilled, Is.EqualTo(5));
            Assert.That(squad.VehiclesDestroyed, Is.EqualTo(2));
            Assert.That(squad.Losses, Is.EqualTo(1));
        }
    }

    [Test]
    public void FromMatchResultForPlayer_SumsMultipleStatisticsModifiersForSameSquad() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 3, vehiclesDestroyed: 1, losses: 0));
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 2, vehiclesDestroyed: 0, losses: 1));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);
        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);

        using (Assert.EnterMultipleScope()) {
            Assert.That(squad.InfantryKilled, Is.EqualTo(5));
            Assert.That(squad.VehiclesDestroyed, Is.EqualTo(1));
            Assert.That(squad.Losses, Is.EqualTo(1));
        }
    }

    [Test]
    public void FromMatchResultForPlayer_AccumulatesExperienceGainAcrossMultipleModifiers() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.ExperienceGain(squadId: 1, experience: 100f));
        modifiers.AddLast(CompanyEventModifier.ExperienceGain(squadId: 1, experience: 50f));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);
        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);

        Assert.That(squad.ExperienceGained, Is.EqualTo(150f));
    }

    [Test]
    public void FromMatchResultForPlayer_SetsWasKilled_WhenKillSquadModifierPresent() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Kill(squadId: 1));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);
        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);

        Assert.That(squad.WasKilled, Is.True);
    }

    [Test]
    public void FromMatchResultForPlayer_DoesNotSetWasKilled_WhenNoKillModifierPresent() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 1, vehiclesDestroyed: 0, losses: 0));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);
        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);

        Assert.That(squad.WasKilled, Is.False);
    }

    [Test]
    public void FromMatchResultForPlayer_SetsPickedUpBlueprint_WhenPickupModifierPresent() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Pickup(squadId: 1, blueprintArg: "sbp_stg44"));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);
        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);

        Assert.That(squad.PickedUpBlueprint, Is.EqualTo("sbp_stg44"));
    }

    [Test]
    public void FromMatchResultForPlayer_OnlyIncludesCurrentPlayerSquads() {
        var playerModifiers = new LinkedList<CompanyEventModifier>();
        playerModifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 2, vehiclesDestroyed: 0, losses: 0));

        var otherModifiers = new LinkedList<CompanyEventModifier>();
        otherModifiers.AddLast(CompanyEventModifier.Statistics(squadId: 99, infantryKilled: 5, vehiclesDestroyed: 3, losses: 2));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = playerModifiers,
                ["other_player"] = otherModifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        using (Assert.EnterMultipleScope()) {
            Assert.That(data.SquadSummaries, Has.Count.EqualTo(1));
            Assert.That(data.SquadSummaries[0].SquadId, Is.EqualTo(1));
        }
    }

    [Test]
    public void FromMatchResultForPlayer_CreatesOneSummaryPerSquad() {
        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 1, vehiclesDestroyed: 0, losses: 0));
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 2, infantryKilled: 0, vehiclesDestroyed: 1, losses: 0));
        modifiers.AddLast(CompanyEventModifier.Kill(squadId: 3));

        var result = BuildValidResult(
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(result, PlayerId);

        using (Assert.EnterMultipleScope()) {
            Assert.That(data.SquadSummaries, Has.Count.EqualTo(3));
            Assert.That(data.SquadSummaries.Select(s => s.SquadId), Is.EquivalentTo(new[] { 1, 2, 3 }));
        }
    }

    [Test]
    public void FromMatchResultForPlayer_UsesCompanyContextForBlueprintAndNetExperience() {
        var company = new Company {
            Id = CompanyId,
            Squads = [
                new Squad {
                    Id = 1,
                    Experience = 90f,
                    Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK
                }
            ]
        };

        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.ExperienceGain(squadId: 1, experience: 150f));

        var result = BuildValidResult(
            playerCompanies: new Dictionary<string, string> { [PlayerId] = CompanyId },
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(
            result,
            PlayerId,
            new Dictionary<string, Company> { [CompanyId] = company });

        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);
        using (Assert.EnterMultipleScope()) {
            Assert.That(squad.Blueprint, Is.EqualTo(SquadBlueprintFixture.SBP_TOMMY_UK));
            Assert.That(squad.ExperienceGained, Is.EqualTo(60f));
            Assert.That(squad.WasKilled, Is.False);
        }
    }

    [Test]
    public void FromMatchResultForPlayer_WhenSquadWasKilled_ResetsPickupAndExperienceGain() {
        var company = new Company {
            Id = CompanyId,
            Squads = [
                new Squad {
                    Id = 1,
                    Experience = 50f,
                    Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK
                }
            ]
        };

        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.ExperienceGain(squadId: 1, experience: 120f));
        modifiers.AddLast(CompanyEventModifier.Pickup(squadId: 1, blueprintArg: "weapon_sten"));
        modifiers.AddLast(CompanyEventModifier.Kill(squadId: 1));

        var result = BuildValidResult(
            playerCompanies: new Dictionary<string, string> { [PlayerId] = CompanyId },
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        var data = MatchOverData.FromMatchResultForPlayer(
            result,
            PlayerId,
            new Dictionary<string, Company> { [CompanyId] = company });

        var squad = data.SquadSummaries.Single(s => s.SquadId == 1);
        using (Assert.EnterMultipleScope()) {
            Assert.That(squad.WasKilled, Is.True);
            Assert.That(squad.ExperienceGained, Is.EqualTo(0f));
            Assert.That(squad.PickedUpBlueprint, Is.EqualTo(string.Empty));
        }
    }

    [Test]
    public void FromMatchResultForPlayer_WhenCompanyDoesNotContainSquad_ThrowsInvalidOperationException() {
        var company = new Company {
            Id = CompanyId,
            Squads = [
                new Squad {
                    Id = 99,
                    Experience = 0f,
                    Blueprint = SquadBlueprintFixture.SBP_TOMMY_UK
                }
            ]
        };

        var modifiers = new LinkedList<CompanyEventModifier>();
        modifiers.AddLast(CompanyEventModifier.Statistics(squadId: 1, infantryKilled: 1, vehiclesDestroyed: 0, losses: 0));

        var result = BuildValidResult(
            playerCompanies: new Dictionary<string, string> { [PlayerId] = CompanyId },
            companyModifiers: new Dictionary<string, LinkedList<CompanyEventModifier>> {
                [PlayerId] = modifiers
            });

        Assert.That(
            () => MatchOverData.FromMatchResultForPlayer(
                result,
                PlayerId,
                new Dictionary<string, Company> { [CompanyId] = company }),
            Throws.TypeOf<InvalidOperationException>());
    }

}
