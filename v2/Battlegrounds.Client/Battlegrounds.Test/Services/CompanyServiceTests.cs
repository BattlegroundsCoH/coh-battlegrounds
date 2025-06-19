using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Test.Models.Companies;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace Battlegrounds.Test.Services;

[TestOf(nameof(CompanyService))]
public class CompanyServiceTests {

    private CompanyService _companyService;

    private Configuration _configuration;

    private IUserService _users;
    private IBattlegroundsServerAPI _battlegroundsServerAPI;

    [SetUp]
    public void Setup() {
        _users = Substitute.For<IUserService>();
        _battlegroundsServerAPI = Substitute.For<IBattlegroundsServerAPI>();
        _configuration = new Configuration() {
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
    public async Task CompanyService_ApplyEvents_ShouldRemoveSquadCorrectly() {

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
    public async Task CompanyService_ApplyEvents_ShouldNotModifyCompanyIfNoEvents() {
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
    public async Task CompanyService_ApplyEvents_ShouldHandleInvalidSquadIdGracefully() {
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
    public async Task CompanyService_ApplyEvents_ShouldHandleMultipleEventsCorrectly() {
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
        Assert.That(updated.Squads, Has.Count.EqualTo(company.Squads.Count-1), "Squad count should be one less after applying multiple events.");
        using (Assert.EnterMultipleScope()) {
            Assert.That(updated.Squads.Any(s => s.Id == squadToKill.Id), Is.False, "Killed squad should not be present in the updated company.");
            Assert.That(updated.Squads.Any(s => s.Id == squadToDeploy.Id), Is.True, "Deployed squad should be present in the updated company.");
            Assert.That(updated.Squads.First(x => x.Id == squadToDeploy.Id).MatchCounts, Is.GreaterThan(0), "Deployed squad should have match counts greater than 0 after being deployed.");
        }
    }

}
