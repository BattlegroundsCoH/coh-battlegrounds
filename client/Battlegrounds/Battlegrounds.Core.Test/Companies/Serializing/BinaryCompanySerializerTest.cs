using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Companies.Serializing;
using Battlegrounds.Core.Games.Blueprints;
using Battlegrounds.Core.Services;
using Battlegrounds.Core.Test.Companies.Templates;
using Battlegrounds.Core.Test.Games.Blueprints;

namespace Battlegrounds.Core.Test.Companies.Serializing;

[TestFixture]
public class BinaryCompanySerializerTest {

    private BinaryCompanySerializer _serializer;
    private ICompanyTemplateService _companyTemplateService;
    private IBlueprintService _blueprintService;

    [SetUp]
    public void SetUp() {
        _companyTemplateService = Substitute.For<ICompanyTemplateService>();
        _blueprintService = Substitute.For<IBlueprintService>();
        _serializer = new BinaryCompanySerializer(CoreTest.Services.GetService<ILogger<BinaryCompanySerializer>>()!, _companyTemplateService, _blueprintService);
    }

    [Test]
    public void WillSerialise() {

        var testCompany = CompanyFixture.DesertRats;

        using var outputStream = new MemoryStream();
        var result = _serializer.Serialize(testCompany, outputStream);

        Assert.Multiple(() => {
            Assert.That(result, Is.True);
        });

    }

    [Test]
    public void WillDeserialise() {

        _companyTemplateService.GetCompanyTemplate(CompanyTemplateFixture.DESERT_RATS.Id).Returns(CompanyTemplateFixture.DESERT_RATS);
        _blueprintService.GetBlueprintById<SquadBlueprint>("company_of_heroes_3", BlueprintFixture.SBP_COH3_BRITISH_TOMMY.Pbgid).Returns(BlueprintFixture.SBP_COH3_BRITISH_TOMMY);

        var testCompany = CompanyFixture.DesertRats;

        using var outputStream = new MemoryStream();
        Assert.That(_serializer.Serialize(testCompany, outputStream), Is.True);

        outputStream.Position = 0;
        var result = _serializer.Deserialise(outputStream);

        Assert.Multiple(() => {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Company>());
            Assert.That(result!.Name, Is.EqualTo(testCompany.Name));
            Assert.That(result!.Id, Is.EqualTo(testCompany.Id));
            // TODO: Verify squads etc. are deserialized properly
        });

    }

}
