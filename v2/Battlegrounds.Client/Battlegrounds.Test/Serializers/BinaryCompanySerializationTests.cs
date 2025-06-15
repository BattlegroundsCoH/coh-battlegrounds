using Battlegrounds.Serializers;
using Battlegrounds.Test.Models.Companies;
using Battlegrounds.Test.Services;

namespace Battlegrounds.Test.Serializers;

[TestFixture]
public sealed class BinaryCompanySerializationTests {

    private BinaryCompanySerializer _serializer;
    private BinaryCompanyDeserializer _deserializer;

    [SetUp]
    public void Setup() {
        _serializer = new BinaryCompanySerializer();
        _deserializer = new BinaryCompanyDeserializer(new BlueprintFixtureService());
    }

    [Test]
    public void CanSerializeAndDeserializeDesertRats() {

        using var stream = new MemoryStream();
        _serializer.SerializeCompany(stream, CompanyFixture.DESERT_RATS);

        // Reset the stream position to the beginning for reading
        stream.Seek(0, SeekOrigin.Begin);

        // Deserialize the company to verify serialization
        var deserializedCompany = _deserializer.DeserializeCompany(stream);
        Assert.That(deserializedCompany, Is.Not.Null, "Deserialized company should not be null.");
        Assert.Multiple(() => {
            Assert.That(deserializedCompany.Id, Is.EqualTo(CompanyFixture.DESERT_RATS.Id), "Deserialized company ID should match.");
            Assert.That(deserializedCompany.Name, Is.EqualTo(CompanyFixture.DESERT_RATS.Name), "Deserialized company name should match.");
            Assert.That(deserializedCompany.Faction, Is.EqualTo(CompanyFixture.DESERT_RATS.Faction), "Deserialized company faction should match.");
            Assert.That(deserializedCompany.GameId, Is.EqualTo(CompanyFixture.DESERT_RATS.GameId), "Deserialized company game ID should match.");
            Assert.That(deserializedCompany.Squads, Has.Count.EqualTo(CompanyFixture.DESERT_RATS.Squads.Count), "Deserialized company should have the same number of squads.");
        });

        for (int i = 0; i < deserializedCompany.Squads.Count; i++) {
            var originalSquad = CompanyFixture.DESERT_RATS.Squads[i];
            var deserializedSquad = deserializedCompany.Squads[i];
            Assert.Multiple(() => {
                Assert.That(deserializedSquad.Id, Is.EqualTo(originalSquad.Id), $"Deserialized squad ID at index {i} should match.");
                Assert.That(deserializedSquad.Blueprint.Id, Is.EqualTo(originalSquad.Blueprint.Id), $"Deserialized squad blueprint ID at index {i} should match.");
                Assert.That(deserializedSquad.Experience, Is.EqualTo(originalSquad.Experience).Within(0.01f), $"Deserialized squad experience at index {i} should match.");
                Assert.That(deserializedSquad.Phase, Is.EqualTo(originalSquad.Phase), $"Deserialized squad phase at index {i} should match.");
            });
        }

    }

}
