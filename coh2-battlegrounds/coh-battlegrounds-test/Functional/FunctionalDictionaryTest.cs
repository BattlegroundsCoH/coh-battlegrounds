using Battlegrounds.Functional;

namespace Battlegrounds.Testing.Functional;

public class FunctionalDictionaryTest {

    [Test]
    public void CanUnionDistinctDictionaries() {

        // Define first
        Dictionary<string, int> first = new() {
            ["one"] = 1,
            ["two"] = 2,
            ["three"] = 3
        };

        // Define second
        Dictionary<string, int> second = new() {
            ["four"] = 4,
            ["five"] = 5,
            ["six"] = 6
        };

        // Union
        IDictionary<string, int> union = first.Union(second);

        // Assert
        Assert.Multiple(() => {
            Assert.That(first, Is.SubsetOf(union));
            Assert.That(second, Is.SubsetOf(union));
        });

    }

    [Test]
    public void WillDiscardSourceEntries() {

        // Define first
        Dictionary<string, int> first = new() {
            ["a"] = 1,
            ["b"] = 2,
            ["c"] = 3
        };

        // Define second
        Dictionary<string, int> second = new() {
            ["c"] = 11,
            ["d"] = 12,
            ["e"] = 13
        };

        // Union
        IDictionary<string, int> union = first.Union(second);

        // Assert
        Assert.Multiple(() => {
            Assert.That(union, Has.Count.EqualTo(5));
            Assert.That(union.ContainsKey("c"), Is.True);
            Assert.That(union["c"], Is.EqualTo(11));
            Assert.That(union.Values, Has.No.Member(3));
        });

    }

}
