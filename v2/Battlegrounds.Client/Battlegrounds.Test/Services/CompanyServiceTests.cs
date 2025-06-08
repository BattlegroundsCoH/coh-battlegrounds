using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Replays;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Test.Models.Companies;

using Microsoft.Extensions.Logging;

using NSubstitute;

using NUnit.Framework.Internal;

namespace Battlegrounds.Test.Services;

[TestFixture]
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
        
        _companyService = new CompanyService(
            _users, 
            new BinaryCompanyDeserializer(new BlueprintFixtureService()), 
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
        LinkedList<ReplayEvent> events = new LinkedList<ReplayEvent>();
        _ = events.AddLast(new SquadKilledEvent(TimeSpan.FromSeconds(15), new ReplayPlayer(0, 0, string.Empty, 0, 0, company.Faction, "ai_default_personality"), (ushort)squadToKill.Id));

        // Act
        var updated = await _companyService.ApplyEvents(events, company, false);

        // Assert
        Assert.That(updated, Is.Not.Null, "Updated company should not be null after applying events.");
        Assert.That(updated.Squads.Count, Is.EqualTo(company.Squads.Count - 1), "Squad should be removed from the company after applying the event.");
        Assert.That(updated.Squads.Any(s => s.Id == squadToKill.Id), Is.False, "Removed squad should not be present in the updated company.");

    }

}
