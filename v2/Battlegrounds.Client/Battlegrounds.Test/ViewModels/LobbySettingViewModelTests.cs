using Battlegrounds.Models.Lobbies;
using Battlegrounds.ViewModels.LobbyHelpers;

namespace Battlegrounds.Test.ViewModels;

[TestOf(typeof(LobbySettingViewModel))]
public sealed class LobbySettingViewModelTests {

    [Test]
    public async Task BooleanDraft_DoesNotMutateConfirmedSettingUntilApply() {
        LobbySetting? requested = null;
        var confirmed = new LobbySetting {
            Name = "enabled",
            Type = LobbySettingType.Boolean,
            Value = 0
        };
        var vm = new LobbySettingViewModel(confirmed, setting => {
            requested = setting;
            return Task.CompletedTask;
        });

        vm.DraftBoolValue = true;

        Assert.Multiple(() => {
            Assert.That(vm.BoolValue, Is.False);
            Assert.That(confirmed.Value, Is.Zero);
            Assert.That(requested, Is.Null);
        });

        await vm.ApplyCommand.ExecuteAsync(null);

        Assert.Multiple(() => {
            Assert.That(requested, Is.Not.SameAs(confirmed));
            Assert.That(requested!.Value, Is.EqualTo(1));
            Assert.That(confirmed.Value, Is.Zero);
        });
    }

    [Test]
    public async Task IntegerDraft_DoesNotInvokeApplyUntilCommandExecutes() {
        LobbySetting? requested = null;
        var confirmed = new LobbySetting {
            Name = "tickets",
            Type = LobbySettingType.Integer,
            Value = 500,
            MinValue = 100,
            MaxValue = 1000,
            Step = 100
        };
        var vm = new LobbySettingViewModel(confirmed, setting => {
            requested = setting;
            return Task.CompletedTask;
        });

        vm.DraftIntValue = 700;

        Assert.Multiple(() => {
            Assert.That(vm.IntValue, Is.EqualTo(500));
            Assert.That(requested, Is.Null);
        });

        await vm.ApplyCommand.ExecuteAsync(null);

        Assert.That(requested!.Value, Is.EqualTo(700));
    }

    [Test]
    public async Task SelectionDraft_DoesNotInvokeApplyUntilCommandExecutes() {
        LobbySetting? requested = null;
        var confirmed = new LobbySetting {
            Name = "mode",
            Type = LobbySettingType.Selection,
            Value = 0,
            Options = [
                new LobbySettingOption("Annihilation", "annihilation"),
                new LobbySettingOption("Victory Points", "victory_points")
            ]
        };
        var vm = new LobbySettingViewModel(confirmed, setting => {
            requested = setting;
            return Task.CompletedTask;
        });

        vm.DraftSelectedOptionIndex = 1;

        Assert.Multiple(() => {
            Assert.That(vm.SelectedOptionIndex, Is.Zero);
            Assert.That(vm.SelectedOption?.Name, Is.EqualTo("Annihilation"));
            Assert.That(requested, Is.Null);
        });

        await vm.ApplyCommand.ExecuteAsync(null);

        Assert.That(requested!.Value, Is.EqualTo(1));
    }

    [Test]
    public void ApplyServerValue_RefreshesConfirmedAndDraftPropertiesWithoutApplying() {
        var applyCalls = 0;
        var vm = new LobbySettingViewModel(
            new LobbySetting {
                Name = "tickets",
                Type = LobbySettingType.Integer,
                Value = 500,
                MinValue = 100,
                MaxValue = 1000
            },
            _ => {
                applyCalls++;
                return Task.CompletedTask;
            });
        var changed = new List<string>();
        vm.PropertyChanged += (_, args) => changed.Add(args.PropertyName!);

        vm.ApplyServerValue(700);

        Assert.Multiple(() => {
            Assert.That(vm.IntValue, Is.EqualTo(700));
            Assert.That(vm.DraftIntValue, Is.EqualTo(700));
            Assert.That(vm.IsDirty, Is.False);
            Assert.That(changed, Contains.Item(nameof(vm.IntValue)));
            Assert.That(changed, Contains.Item(nameof(vm.DraftIntValue)));
            Assert.That(applyCalls, Is.Zero);
        });
    }

}
