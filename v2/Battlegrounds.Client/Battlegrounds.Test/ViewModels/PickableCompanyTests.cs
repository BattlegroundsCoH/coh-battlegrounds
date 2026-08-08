using Battlegrounds.Models.Companies;
using Battlegrounds.ViewModels.LobbyHelpers;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.Test.ViewModels;

/// <summary>
/// Unit tests for <see cref="PickableCompany"/>.
/// <para>
/// These all exist for one reason: a WPF <c>Selector</c> resolves its <c>SelectedItem</c> against
/// <c>ItemsSource</c> by equality. If a <see cref="PickableCompany"/> naming a company is not equal
/// to the item in the list naming the same company, the lobby's company picker renders blank.
/// </para>
/// </summary>
[TestOf(typeof(PickableCompany))]
public sealed class PickableCompanyTests {

    private static Company CreateCompany(string id) => new() {
        Id = id,
        Name = $"Company {id}",
        Faction = "british_africa",
        GameId = "CoH3"
    };

    private static IRelayCommand CreateCommand() => new RelayCommand(() => { });

    [Test]
    public void Equals_SameCompanyDifferentPreviewCommands_IsTrue() {
        var company = CreateCompany("company-1");
        var a = new PickableCompany(false, false, company, CreateCommand());
        var b = new PickableCompany(false, false, company, CreateCommand());

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_SameCompanyIdDifferentCompanyInstances_IsTrue() {
        // The download path hands the slot a Company deserialised from the server while the picker
        // still lists the locally loaded instance of the same company.
        var a = new PickableCompany(false, false, CreateCompany("company-1"), CreateCommand());
        var b = new PickableCompany(false, false, CreateCompany("company-1"), CreateCommand());

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_DifferentCompanyIds_IsFalse() {
        var a = new PickableCompany(false, false, CreateCompany("company-1"), CreateCommand());
        var b = new PickableCompany(false, false, CreateCompany("company-2"), CreateCommand());

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_NonePlaceholders_IsTrue() {
        var a = new PickableCompany(true, false, null, CreateCommand());
        var b = new PickableCompany(true, false, null, CreateCommand());

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_NoneAndRandomPlaceholders_IsFalse() {
        var none = new PickableCompany(true, false, null, CreateCommand());
        var random = new PickableCompany(false, true, null, CreateCommand());

        Assert.That(none, Is.Not.EqualTo(random));
    }

    [Test]
    public void Equals_Null_IsFalse() {
        var company = new PickableCompany(false, false, CreateCompany("company-1"), CreateCommand());

        Assert.That(company.Equals(null), Is.False);
    }

    [Test]
    public void Contains_FindsAnEquivalentOptionInAList() {
        // The exact lookup Selector performs when it is given a SelectedItem.
        List<PickableCompany> options = [
            new(false, false, CreateCompany("company-1"), CreateCommand()),
            new(false, false, CreateCompany("company-2"), CreateCommand())
        ];
        var selected = new PickableCompany(false, false, CreateCompany("company-2"), CreateCommand());

        Assert.That(options, Does.Contain(selected));
        Assert.That(options.IndexOf(selected), Is.EqualTo(1));
    }

}
