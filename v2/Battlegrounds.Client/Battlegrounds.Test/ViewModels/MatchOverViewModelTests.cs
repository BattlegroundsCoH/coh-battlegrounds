using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.ViewModels;

using NSubstitute;

namespace Battlegrounds.Test.ViewModels;

[TestFixture]
public sealed class MatchOverViewModelTests {

    private static MatchOverData CreateMatchOverData(IReadOnlyList<SquadMatchSummary>? summaries = null) {
        return new MatchOverData {
            MatchId = "match-1",
            GameId = "coh3",
            Scenario = "test_map",
            MatchDuration = TimeSpan.FromMinutes(27),
            Concluded = true,
            IsVictory = true,
            CompanyId = "company-1",
            IsValid = true,
            HasBadEvents = false,
            SquadSummaries = summaries ?? []
        };
    }

    [Test]
    public void Constructor_MapsDataPropertiesAndGame() {
        var game = Substitute.For<Game>();
        var summaries = new List<SquadMatchSummary> {
            new(
                SquadId: 7,
                Blueprint: null,
                InfantryKilled: 3,
                VehiclesDestroyed: 1,
                Losses: 0,
                ExperienceGained: 42f,
                WasKilled: false,
                PickedUpBlueprint: "weapon_sten")
        };
        var data = CreateMatchOverData(summaries);

        var vm = new MatchOverViewModel(data, game, () => { });

        using (Assert.EnterMultipleScope()) {
            Assert.That(vm.IsVictory, Is.True);
            Assert.That(vm.Scenario, Is.EqualTo("test_map"));
            Assert.That(vm.MatchDuration, Is.EqualTo(TimeSpan.FromMinutes(27)));
            Assert.That(vm.Concluded, Is.True);
            Assert.That(vm.HasBadEvents, Is.False);
            Assert.That(vm.Game, Is.SameAs(game));
            Assert.That(vm.SquadSummaries, Is.EqualTo(summaries));
        }
    }

    [Test]
    public void CloseCommand_InvokesCallback() {
        var game = Substitute.For<Game>();
        var data = CreateMatchOverData();
        bool closed = false;

        var vm = new MatchOverViewModel(data, game, () => closed = true);

        vm.CloseCommand.Execute(null);

        Assert.That(closed, Is.True);
    }

}
