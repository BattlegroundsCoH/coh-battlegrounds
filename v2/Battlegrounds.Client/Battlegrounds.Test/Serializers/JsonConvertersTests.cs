using System.Text;
using System.Text.Json;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Replays;
using Battlegrounds.Serializers;

namespace Battlegrounds.Test.Serializers;

[TestOf(typeof(ReadOnlySetConverterFactory))]
[TestOf(typeof(LinkedListConverterFactory))]
public class JsonConvertersTests {

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) {
        Converters = { new ReadOnlySetConverterFactory(), new LinkedListConverterFactory() }
    };

    // --- ReadOnlySetConverterFactory ---

    [Test]
    public void ReadOnlySetConverterFactory_CanConvert_IReadOnlySetOfString() {
        var factory = new ReadOnlySetConverterFactory();
        Assert.That(factory.CanConvert(typeof(IReadOnlySet<string>)), Is.True);
    }

    [Test]
    public void ReadOnlySetConverterFactory_DoesNotConvert_HashSet() {
        var factory = new ReadOnlySetConverterFactory();
        Assert.That(factory.CanConvert(typeof(HashSet<string>)), Is.False);
    }

    [Test]
    public void IReadOnlySet_Deserializes_WithExpectedElements() {
        const string json = """["player-1","player-2","player-3"]""";
        var result = JsonSerializer.Deserialize<IReadOnlySet<string>>(json, Options);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(new[] { "player-1", "player-2", "player-3" }));
    }

    [Test]
    public void IReadOnlySet_EmptyArray_DeserializesAsEmptySet() {
        var result = JsonSerializer.Deserialize<IReadOnlySet<string>>("[]", Options);
        Assert.That(result, Is.Not.Null.And.Empty);
    }

    [Test]
    public void IReadOnlySet_Serializes_ToJsonArray() {
        IReadOnlySet<string> set = new HashSet<string> { "a", "b" };
        string json = JsonSerializer.Serialize(set, Options);
        Assert.That(json, Does.StartWith("[").And.EndWith("]"));
        Assert.That(json, Does.Contain("\"a\"").And.Contain("\"b\""));
    }

    // --- LinkedListConverterFactory ---

    [Test]
    public void LinkedListConverterFactory_CanConvert_LinkedListOfT() {
        var factory = new LinkedListConverterFactory();
        Assert.That(factory.CanConvert(typeof(LinkedList<string>)), Is.True);
    }

    [Test]
    public void LinkedListConverterFactory_DoesNotConvert_List() {
        var factory = new LinkedListConverterFactory();
        Assert.That(factory.CanConvert(typeof(List<string>)), Is.False);
    }

    [Test]
    public void LinkedList_Deserializes_WithCorrectOrderAndValues() {
        const string json = """
            [
                { "squadId": 1, "eventType": "kill_squad" },
                { "squadId": 2, "eventType": "experience_gain", "floatValue": 250.0 }
            ]
            """;

        var result = JsonSerializer.Deserialize<LinkedList<CompanyEventModifier>>(json, Options);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.Multiple(() => {
            Assert.That(result.First!.Value.SquadId, Is.EqualTo(1));
            Assert.That(result.First!.Value.EventType, Is.EqualTo(CompanyEventModifier.EVENT_TYPE_KILL_SQUAD));
            Assert.That(result.Last!.Value.SquadId, Is.EqualTo(2));
            Assert.That(result.Last!.Value.EventType, Is.EqualTo(CompanyEventModifier.EVENT_TYPE_EXPERIENCE_GAIN));
            Assert.That(result.Last!.Value.FloatValue, Is.EqualTo(250.0f));
        });
    }

    [Test]
    public void LinkedList_EmptyArray_DeserializesAsEmptyList() {
        var result = JsonSerializer.Deserialize<LinkedList<CompanyEventModifier>>("[]", Options);
        Assert.That(result, Is.Not.Null.And.Empty);
    }

    [Test]
    public void LinkedList_Serializes_ToJsonArray() {
        var list = new LinkedList<CompanyEventModifier>();
        list.AddLast(CompanyEventModifier.Kill(1));
        list.AddLast(CompanyEventModifier.ExperienceGain(2, 50.0f));
        string json = JsonSerializer.Serialize(list, Options);
        Assert.That(json, Does.StartWith("[").And.EndWith("]"));
    }

    // --- MatchResult full deserialization ---

    [Test]
    public async Task MatchResult_Deserializes_WithIReadOnlySetAndLinkedListProperties() {
        const string json = """
            {
                "isValid": true,
                "lobbyId": "lobby-123",
                "gameId": "game-456",
                "matchId": "match-789",
                "modVersion": "2.0.0",
                "scenario": "winter_crossing",
                "matchDuration": "00:35:20",
                "companyModifiers": {
                    "player-1": [
                        { "squadId": 10, "eventType": "kill_squad" },
                        { "squadId": 11, "eventType": "experience_gain", "floatValue": 250.0 }
                    ],
                    "player-2": [
                        { "squadId": 20, "eventType": "statistics", "intValue1": 5, "intValue2": 1, "intValue3": 2 }
                    ]
                },
                "playerCompanies": {
                    "player-1": "company-aaa",
                    "player-2": "company-bbb"
                },
                "winners": ["player-1"],
                "losers": ["player-2"],
                "players": ["player-1", "player-2"],
                "concluded": true
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = await JsonSerializer.DeserializeAsync<MatchResult>(stream, Options);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() => {
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.LobbyId, Is.EqualTo("lobby-123"));
            Assert.That(result.MatchId, Is.EqualTo("match-789"));
            Assert.That(result.ModVersion, Is.EqualTo("2.0.0"));
            Assert.That(result.Scenario, Is.EqualTo("winter_crossing"));
            Assert.That(result.MatchDuration, Is.EqualTo(new TimeSpan(0, 35, 20)));
            Assert.That(result.Concluded, Is.True);

            // IReadOnlySet<string>
            Assert.That(result.Winners, Is.EquivalentTo(new[] { "player-1" }));
            Assert.That(result.Losers, Is.EquivalentTo(new[] { "player-2" }));
            Assert.That(result.Players, Is.EquivalentTo(new[] { "player-1", "player-2" }));

            // IReadOnlyDictionary<string, string>
            Assert.That(result.PlayerCompanies["player-1"], Is.EqualTo("company-aaa"));
            Assert.That(result.PlayerCompanies["player-2"], Is.EqualTo("company-bbb"));

            // IReadOnlyDictionary<string, LinkedList<CompanyEventModifier>>
            Assert.That(result.CompanyModifiers, Has.Count.EqualTo(2));
            Assert.That(result.CompanyModifiers["player-1"], Has.Count.EqualTo(2));
            Assert.That(result.CompanyModifiers["player-1"].First!.Value.SquadId, Is.EqualTo(10));
            Assert.That(result.CompanyModifiers["player-1"].First!.Value.EventType, Is.EqualTo(CompanyEventModifier.EVENT_TYPE_KILL_SQUAD));
            Assert.That(result.CompanyModifiers["player-1"].Last!.Value.FloatValue, Is.EqualTo(250.0f));
            Assert.That(result.CompanyModifiers["player-2"].First!.Value.EventType, Is.EqualTo(CompanyEventModifier.EVENT_TYPE_STATISTICS));
            Assert.That(result.CompanyModifiers["player-2"].First!.Value.IntValue1, Is.EqualTo(5));
            Assert.That(result.CompanyModifiers["player-2"].First!.Value.IntValue2, Is.EqualTo(1));
            Assert.That(result.CompanyModifiers["player-2"].First!.Value.IntValue3, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task MatchResult_WithEmptySets_DeserializesWithoutError() {
        const string json = """
            {
                "isValid": true,
                "lobbyId": "",
                "gameId": "",
                "matchId": "",
                "modVersion": "",
                "scenario": "",
                "matchDuration": "00:00:00",
                "companyModifiers": {},
                "playerCompanies": {},
                "winners": [],
                "losers": [],
                "players": [],
                "concluded": false
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = await JsonSerializer.DeserializeAsync<MatchResult>(stream, Options);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() => {
            Assert.That(result.Winners, Is.Empty);
            Assert.That(result.Losers, Is.Empty);
            Assert.That(result.Players, Is.Empty);
            Assert.That(result.CompanyModifiers, Is.Empty);
        });
    }

}
