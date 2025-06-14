using Battlegrounds.Models;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Playing;
using Battlegrounds.Parsers;
using Battlegrounds.Services;

using NSubstitute;

namespace Battlegrounds.Test.Parsers;

[TestFixture, TestOf(typeof(BlueprintParser<>))]
public class BlueprintParserTests {

    private IGameLocaleService _gameLocaleService = null!;
    private BlueprintParser<CoH3> _parser = null!;

    [SetUp]
    public void SetUp() {
        _gameLocaleService = Substitute.For<IGameLocaleService>();
        _parser = new BlueprintParser<CoH3>(_gameLocaleService);
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenLocaleServiceIsNull() {
        Assert.Throws<ArgumentNullException>(() => new BlueprintParser<CoH3>(null!));
    }

    [Test]
    public void ParseSquadBlueprints_ThrowsArgumentNullException_WhenSourceIsNull() {
        Assert.ThrowsAsync<ArgumentNullException>(() => _parser.ParseSquadBlueprints(null!));
    }

    [Test]
    public void ParseSquadBlueprints_ThrowsArgumentException_WhenSourceIsNotReadable() {
        using MemoryStream stream = new();
        stream.Close(); // Close the stream to make it unreadable
        Assert.ThrowsAsync<ArgumentException>(() => _parser.ParseSquadBlueprints(stream));
    }

    [Test]
    public async Task ParseSquadBlueprints_ReturnsEmptyDictionary_WhenNoBlueprintsFound() {
        using MemoryStream stream = new();
        var result = await _parser.ParseSquadBlueprints(stream);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ParseSquadBlueprints_ReturnsCorrectBlueprints_WhenValidDataProvided() {

        // Prepare
        _gameLocaleService.FromGame<CoH3>(Arg.Any<uint>()).Returns(x => new LocaleString(x.ArgAt<uint>(0), (_, _) => ""));

        // Act
        using FileStream stream = new("TestData/blueprints/sbps.yaml", FileMode.Open, FileAccess.Read);
        var result = await _parser.ParseSquadBlueprints(stream);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2), "Expected two squad blueprints to be parsed.");

        // Grab tommy_uk
        SquadBlueprint? tommyUK = result.FirstOrDefault(bp => bp.Id == "tommy_uk");
        Assert.That(tommyUK, Is.Not.Null, "Expected tommy_uk squad blueprint to be parsed.");
        using (Assert.EnterMultipleScope()) {
            Assert.That(tommyUK.Category, Is.EqualTo(SquadCategory.Infantry), "Expected tommy_uk squad blueprint to be in the Infantry category.");
            Assert.That(tommyUK.FactionAssociation, Is.EqualTo("british_africa"), "Expected tommy_uk squad blueprint to be associated with the British Africa faction.");
            Assert.That(tommyUK.IsInfantry, Is.True, "Expected tommy_uk squad blueprint to be marked as infantry.");
            Assert.That(tommyUK.HasExtension<HoldExtension>(), Is.False, "Expected tommy_uk squad blueprint to not have a HoldExtension.");
            Assert.That(tommyUK.HasExtension<UIExtension>(), Is.True, "Expected tommy_uk squad blueprint to have a UIExtension.");
            Assert.That(tommyUK.UI.IconName, Is.EqualTo("tommy_uk"), "Expected tommy_uk squad blueprint to have the correct icon name in UIExtension.");
        }

        // Grab CWT15 Truck
        SquadBlueprint? cwt15Truck = result.FirstOrDefault(bp => bp.Id == "cwt_15_truck_uk");
        Assert.That(cwt15Truck, Is.Not.Null, "Expected cwt_15_truck_uk squad blueprint to be parsed.");
        using (Assert.EnterMultipleScope()) {
            Assert.That(cwt15Truck.Category, Is.EqualTo(SquadCategory.Support), "Expected cwt_15_truck_uk squad blueprint to be in the Support category.");
            Assert.That(cwt15Truck.HasExtension<HoldExtension>(), Is.True, "Expected cwt_15_truck_uk squad blueprint to have a HoldExtension.");
        }

    }

}
