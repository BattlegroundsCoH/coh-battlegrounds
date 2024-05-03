using Battlegrounds.Core.Companies.Serializing;

namespace Battlegrounds.Core.Test.Companies.Serializing;

[TestFixture]
public class BinaryCompanySerializerTest {

    private BinaryCompanySerializer _serializer;

    [SetUp]
    public void SetUp() {
        _serializer = new BinaryCompanySerializer(CoreTest.Services.GetService<ILogger<BinaryCompanySerializer>>()!);
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

}
