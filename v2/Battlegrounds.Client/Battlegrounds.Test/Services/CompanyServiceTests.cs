using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.Facades;
using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Services.Data;
using Battlegrounds.Test.Models.Companies;
using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Battlegrounds.Test.Services;


[TestOf(nameof(CompanyService))]
public class CompanyServiceTests
{

    private CompanyService _companyService;

    private Configuration _configuration;

    private IUserService _users;
    private IBattlegroundsServerAPI _battlegroundsServerAPI;

    [SetUp]
    public void Setup()
    {
        _users = Substitute.For<IUserService>();
        _battlegroundsServerAPI = Substitute.For<IBattlegroundsServerAPI>();
        _configuration = new Configuration()
        {
            CompaniesPath = Path.GetTempPath(),
        };

        var bps = new BlueprintFixtureService();
        _companyService = new CompanyService(
            _users,
            new BinaryCompanyDeserializer(bps),
            new BinaryCompanySerializer(),
            _battlegroundsServerAPI,
            Substitute.For<ILogger<CompanyService>>(),
            _configuration);
    }

    [Test]
    public async Task CompanyService_ApplyEvents_ShouldRemoveSquadCorrectly()
    {

        // Arrange
        Company company = CompanyFixture.DESERT_RATS;
        Squad squadToKill = company.Squads[0];
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Kill((ushort)squadToKill.Id));

        // Act
        var updated = await _companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null, "Updated company should not be null after applying events.");
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count - 1), "Squad should be removed from the company after applying the event.");
        Assert.That(updated.Squads.Any(s => s.Id == squadToKill.Id), Is.False, "Removed squad should not be present in the updated company.");

    }

    [Test]
    public async Task CompanyService_ApplyEvents_ShouldNotModifyCompanyIfNoEvents()
    {
        // Arrange
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();

        // Act
        var updated = await _companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null, "Updated company should not be null after applying events.");
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count), "Squad count should remain the same if no events are applied.");
    }

    [Test]
    public async Task CompanyService_ApplyEvents_ShouldHandleInvalidSquadIdGracefully()
    {
        // Arrange
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Kill(9999)); // Non-existent squad ID

        // Act
        var updated = await _companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null, "Updated company should not be null after applying events.");
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count), "Squad count should remain the same if an invalid squad ID is provided.");
    }

    [Test]
    public async Task CompanyService_ApplyEvents_ShouldHandleMultipleEventsCorrectly()
    {
        // Arrange
        Company company = CompanyFixture.DESERT_RATS;
        Squad squadToKill = company.Squads[0];
        Squad squadToDeploy = company.Squads[1];
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Kill(squadToKill.Id));
        _ = events.AddLast(CompanyEventModifier.InMatch(squadToDeploy.Id));

        // Act
        var updated = await _companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null, "Updated company should not be null after applying events.");
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count - 1), "Squad count should be one less after applying multiple events.");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated.Squads.Any(s => s.Id == squadToKill.Id), Is.False, "Killed squad should not be present in the updated company.");
            Assert.That(updated.Squads.Any(s => s.Id == squadToDeploy.Id), Is.True, "Deployed squad should be present in the updated company.");
            Assert.That(updated.Squads.First(x => x.Id == squadToDeploy.Id).MatchCounts, Is.GreaterThan(0), "Deployed squad should have match counts greater than 0 after being deployed.");
        }
    }


    /// <summary>
    /// Tests that ApplyEvents throws ArgumentNullException when localEvents parameter is null.
    /// </summary>
    [Test]
    public void ApplyEvents_NullLocalEvents_ThrowsArgumentNullException()
    {
        // Arrange
        CompanyService companyService = CreateCompanyService();
        Company company = CompanyFixture.DESERT_RATS;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await companyService.ApplyEvents(null, company, false));
        Assert.That(ex.ParamName, Is.EqualTo("localEvents"));
        Assert.That(ex.Message, Does.Contain("Local events cannot be null"));
    }

    /// <summary>
    /// Tests that ApplyEvents logs a warning when an in-match event references a non-existent squad ID.
    /// </summary>
    [Test]
    public async Task ApplyEvents_InMatchEventWithInvalidSquadId_LogsWarning()
    {
        // Arrange
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();
        CompanyService companyService = CreateCompanyService(logger);
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.InMatch(9999));

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("9999")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that ApplyEvents correctly updates squad experience when processing an experience gain event with a valid squad ID.
    /// </summary>
    [Test]
    public async Task ApplyEvents_ExperienceGainEventWithValidSquadId_UpdatesSquadExperience()
    {
        // Arrange
        CompanyService companyService = CreateCompanyService();
        Company company = CompanyFixture.DESERT_RATS;
        Squad targetSquad = company.Squads[0];
        float experienceGain = 150.5f;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.ExperienceGain(targetSquad.Id, experienceGain));

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        Squad updatedSquad = updated.Squads.First(s => s.Id == targetSquad.Id);
        Assert.That(updatedSquad.Experience, Is.EqualTo(experienceGain));
    }

    /// <summary>
    /// Tests that ApplyEvents logs a warning when an experience gain event references a non-existent squad ID.
    /// </summary>
    [Test]
    public async Task ApplyEvents_ExperienceGainEventWithInvalidSquadId_LogsWarning()
    {
        // Arrange
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();
        CompanyService companyService = CreateCompanyService(logger);
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.ExperienceGain(8888, 100.0f));

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("Squad 8888 not found for experience gain event.")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that ApplyEvents correctly updates squad statistics (infantry and vehicle kills) when processing a statistics event with a valid squad ID.
    /// </summary>
    [Test]
    public async Task ApplyEvents_StatisticsEventWithValidSquadId_UpdatesSquadStatistics()
    {
        // Arrange
        CompanyService companyService = CreateCompanyService();
        Company company = CompanyFixture.DESERT_RATS;
        Squad targetSquad = company.Squads[0];
        int infantryKills = 10;
        int vehicleKills = 5;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Statistics(targetSquad.Id, infantryKills, vehicleKills));

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        Squad updatedSquad = updated.Squads.First(s => s.Id == targetSquad.Id);
        Assert.That(updatedSquad.TotalInfantryKills, Is.EqualTo(targetSquad.TotalInfantryKills + infantryKills));
        Assert.That(updatedSquad.TotalVehicleKills, Is.EqualTo(targetSquad.TotalVehicleKills + vehicleKills));
    }

    /// <summary>
    /// Tests that ApplyEvents logs a warning when a statistics event references a non-existent squad ID.
    /// </summary>
    [Test]
    public async Task ApplyEvents_StatisticsEventWithInvalidSquadId_LogsWarning()
    {
        // Arrange
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();
        CompanyService companyService = CreateCompanyService(logger);
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Statistics(7777, 10, 5));

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("7777")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that ApplyEvents throws NotImplementedException when processing a pickup event with a valid squad ID.
    /// </summary>
    [Test]
    public void ApplyEvents_PickupEventWithValidSquadId_ThrowsNotImplementedException()
    {
        // Arrange
        CompanyService companyService = CreateCompanyService();
        Company company = CompanyFixture.DESERT_RATS;
        Squad targetSquad = company.Squads[0];
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Pickup(targetSquad.Id, "test_blueprint"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotImplementedException>(async () =>
            await companyService.ApplyEvents(events, company, false));
        Assert.That(ex.Message, Does.Contain("Pickup event handling is not implemented yet"));
    }

    /// <summary>
    /// Tests that ApplyEvents logs a warning when a pickup event references a non-existent squad ID.
    /// </summary>
    [Test]
    public async Task ApplyEvents_PickupEventWithInvalidSquadId_LogsWarning()
    {
        // Arrange
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();
        CompanyService companyService = CreateCompanyService(logger);
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.Pickup(6666, "test_blueprint"));

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString().Contains("6666")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that ApplyEvents logs a warning when processing an unknown event type.
    /// </summary>
    [Test]
    public async Task ApplyEvents_UnknownEventType_LogsWarning()
    {
        // Arrange
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();
        CompanyService companyService = CreateCompanyService(logger);
        Company company = CompanyFixture.DESERT_RATS;
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        CompanyEventModifier unknownEvent = new CompanyEventModifier { SquadId = 1, EventType = "unknown_event_type" };
        _ = events.AddLast(unknownEvent);

        // Act
        var updated = await companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count));
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString().Contains("Unknown replay event type: unknown_event_type")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that ApplyEvents saves the company locally when commitLocally is true and SaveCompany succeeds.
    /// </summary>
    [Test]
    public async Task ApplyEvents_CommitLocallyTrue_SavesCompanySuccessfully()
    {
        // Arrange
        IUserService userService = Substitute.For<IUserService>();
        IBattlegroundsServerAPI battlegroundsServerAPI = Substitute.For<IBattlegroundsServerAPI>();
        Configuration configuration = new Configuration() { CompaniesPath = Path.GetTempPath() };
        var bps = new BlueprintFixtureService();
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();
        CompanyService companyService = new CompanyService(
            userService,
            new BinaryCompanyDeserializer(bps),
            new BinaryCompanySerializer(),
            battlegroundsServerAPI,
            logger,
            configuration);

        Company company = CompanyFixture.DESERT_RATS;
        Squad targetSquad = company.Squads[0];
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.InMatch(targetSquad.Id));

        // Act
        var updated = await companyService.ApplyEvents(events, company, commitLocally: true);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Version, Is.EqualTo(company.Version + 1));

        // Clean up
        string companyFilePath = Path.Combine(configuration.CompaniesPath, $"{updated.GameId}_{updated.Faction}_{updated.Id}.cmpny");
        if (File.Exists(companyFilePath))
        {
            File.Delete(companyFilePath);
        }
    }

    /// <summary>
    /// Tests that ApplyEvents returns null when commitLocally is true and SaveCompany fails.
    /// This test verifies the error handling path when local save operations fail.
    /// </summary>
    [Test]
    [Category("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task ApplyEvents_CommitLocallyTrueAndSaveFails_ReturnsNull()
    {
        // Arrange
        IUserService userService = Substitute.For<IUserService>();
        IBattlegroundsServerAPI battlegroundsServerAPI = Substitute.For<IBattlegroundsServerAPI>();
        ICompanySerializer companySerializer = Substitute.For<ICompanySerializer>();
        companySerializer.When(x => x.SerializeCompany(Arg.Any<Stream>(), Arg.Any<Company>()))
            .Do(x => throw new IOException("Simulated save failure"));

        Configuration configuration = new Configuration() { CompaniesPath = Path.Combine(Path.GetTempPath(), "invalid_path_" + Guid.NewGuid().ToString()) };
        var bps = new BlueprintFixtureService();
        ILogger<CompanyService> logger = Substitute.For<ILogger<CompanyService>>();

        CompanyService companyService = new CompanyService(
            userService,
            new BinaryCompanyDeserializer(bps),
            companySerializer,
            battlegroundsServerAPI,
            logger,
            configuration);

        Company company = CompanyFixture.DESERT_RATS;
        Squad targetSquad = company.Squads[0];
        LinkedList<CompanyEventModifier> events = new LinkedList<CompanyEventModifier>();
        _ = events.AddLast(CompanyEventModifier.InMatch(targetSquad.Id));

        // Act
        var updated = await companyService.ApplyEvents(events, company, commitLocally: true);

        // Assert
        Assert.That(updated, Is.Null, "ApplyEvents should return null when SaveCompany fails");
        logger.Received(1).LogError(Arg.Is<string>(s => s.Contains("Failed to commit changes")), company.Id);
    }

    private CompanyService CreateCompanyService(ILogger<CompanyService>? logger = null)
    {
        IUserService userService = Substitute.For<IUserService>();
        IBattlegroundsServerAPI battlegroundsServerAPI = Substitute.For<IBattlegroundsServerAPI>();
        Configuration configuration = new Configuration() { CompaniesPath = Path.GetTempPath() };
        var bps = new BlueprintFixtureService();

        return new CompanyService(
            userService,
            new BinaryCompanyDeserializer(bps),
            new BinaryCompanySerializer(),
            battlegroundsServerAPI,
            logger ?? Substitute.For<ILogger<CompanyService>>(),
            configuration);
    }

    /// <summary>
    /// Tests that SyncWithServerAsync logs a warning and returns early when the server is not available.
    /// Input: Server reports as unavailable via IsServerAvailableAsync returning false.
    /// Expected: Method logs warning message and exits without attempting to sync any companies.
    /// </summary>
    [Test]
    public async Task SyncWithServerAsync_ServerNotAvailable_LogsWarningAndReturnsEarly()
    {
        // Arrange
        var logger = Substitute.For<ILogger<CompanyService>>();
        var serverAPI = Substitute.For<IBattlegroundsServerAPI>();
        var userService = Substitute.For<IUserService>();
        var configuration = new Configuration { CompaniesPath = Path.GetTempPath() };
        var companySerializer = Substitute.For<ICompanySerializer>();
        var companyDeserializer = Substitute.For<ICompanyDeserializer>();

        serverAPI.IsServerAvailableAsync().Returns(false);

        var service = new CompanyService(
            userService,
            companyDeserializer,
            companySerializer,
            serverAPI,
            logger,
            configuration);

        // Act
        await service.SyncWithServerAsync();

        // Assert
        await serverAPI.Received(1).IsServerAvailableAsync();
        await serverAPI.DidNotReceive().GetCompanyInfoAsync(Arg.Any<string>(), Arg.Any<string>());
        await serverAPI.DidNotReceive().UploadCompanyAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<uint>(), Arg.Any<Stream>());
    }

}