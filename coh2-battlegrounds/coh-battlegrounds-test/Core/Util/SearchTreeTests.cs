using Battlegrounds.Util;

namespace Battlegrounds.Testing.Core.Util;

[TestFixture]
public class SearchTreeTests {

    [Test]
    public void AddAndGetNode_WithValidPath_ShouldAddNodeAndGetNode() {

        // Arrange
        var tree = new SearchTree<int>();
        const string path = "foo.bar.baz";
        const int expectedValue = 42;

        // Act
        tree.Add(path, expectedValue);
        var node = tree.Lookup(path);

        // Assert
        Assert.That(node, Is.EqualTo(expectedValue));

    }

    [Test]
    public void AddAndGetNode_WithInvalidPath_ShouldReturnNull() {

        // Arrange
        var tree = new SearchTree<int>();
        const string path = "foo.bar.baz";
        const string invalidPath = "foo.qux.baz";
        const int expectedValue = 42;

        // Act
        tree.Add(path, expectedValue);
        var node = tree.Lookup(invalidPath, out int _);

        // Assert
        Assert.That(node, Is.False);

    }

    [Test]
    public void Lookup_WithValidPath_ShouldReturnTrueAndValue() {

        // Arrange
        var tree = new SearchTree<int>();
        const string path = "foo.bar.baz";
        const int expectedValue = 42;

        // Act
        tree.Add(path, expectedValue);
        var result = tree.Lookup(path, out var value);

        // Assert
        Assert.Multiple(() => {
            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(expectedValue));
        });

    }

    [Test]
    public void Lookup_WithInvalidPath_ShouldReturnFalseAndNullValue() {

        // Arrange
        var tree = new SearchTree<int>();
        const string path = "foo.bar.baz";
        const string invalidPath = "foo.qux.baz";
        const int expectedValue = 42;

        // Act
        tree.Add(path, expectedValue);
        var result = tree.Lookup(invalidPath, out var value);

        // Assert
        Assert.That(result, Is.False);

    }

    [Test]
    public void Remove_WithValidPath_ShouldRemoveNode() {

        // Arrange
        var tree = new SearchTree<int>();
        const string path = "foo.bar.baz";
        const int expectedValue = 42;

        // Act
        tree.Add(path, expectedValue);
        tree.Remove(path);

        // Assert
        Assert.That(tree.Contains(path), Is.False);

    }

    [Test]
    public void Clear_ShouldRemoveAllNodes() {

        // Arrange
        var tree = new SearchTree<int>();
        const string path1 = "foo.bar.baz";
        const string path2 = "qux.quux.corge";
        const int expectedValue1 = 42;
        const int expectedValue2 = 99;

        // Act
        tree.Add(path1, expectedValue1);
        tree.Add(path2, expectedValue2);
        tree.Clear();

        // Assert
        Assert.Multiple(() => {
            Assert.That(tree.Contains(path1), Is.False);
            Assert.That(tree.Contains(path2), Is.False);
        });

    }

    [Test]
    public void Lookup_ReturnsCorrectValue_ForExistingPath() {

        // Arrange
        var tree = new SearchTree<int> {
            { "foo.bar", 42 }
        };

        // Act
        var result = tree.Lookup("foo.bar");

        // Assert
        Assert.That(result, Is.EqualTo(42));

    }

    [Test]
    public void Merge_EmptyTrees_ReturnsEmptyTree() {
        // Arrange
        var tree1 = new SearchTree<int>();
        var tree2 = new SearchTree<int>();

        // Act
        var mergedTree = tree1.Merge(tree2);

        // Assert
        Assert.That(mergedTree, Has.Count.EqualTo(2));
    }

    [Test]
    public void Merge_TwoNonEmptyTrees_ReturnsMergedTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 },
            { "b", 2 }
        };

        var tree2 = new SearchTree<int> {
            { "c", 3 },
            { "d", 4 }
        };

        // Act
        var mergedTree = tree1.Merge(tree2);

        // Assert
        Assert.That(mergedTree, Has.Count.EqualTo(2));
        Assert.Multiple(() => {
            Assert.That(mergedTree.Lookup("a"), Is.EqualTo(1));
            Assert.That(mergedTree.Lookup("b"), Is.EqualTo(2));
            Assert.That(mergedTree.Lookup("c"), Is.EqualTo(3));
            Assert.That(mergedTree.Lookup("d"), Is.EqualTo(4));
        });
    }

    [Test]
    public void Merge_ThreeNonEmptyTrees_ReturnsMergedTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 },
            { "b", 2 }
        };

        var tree2 = new SearchTree<int> {
            { "c", 3 },
            { "d", 4 }
        };

        var tree3 = new SearchTree<int> {
            { "e", 5 },
            { "f", 6 }
        };

        // Act
        var mergedTree = tree1.Merge(tree2, tree3);

        // Assert
        Assert.That(mergedTree, Has.Count.EqualTo(3));
        Assert.Multiple(() => {
            Assert.That(mergedTree.Lookup("a"), Is.EqualTo(1));
            Assert.That(mergedTree.Lookup("b"), Is.EqualTo(2));
            Assert.That(mergedTree.Lookup("c"), Is.EqualTo(3));
            Assert.That(mergedTree.Lookup("d"), Is.EqualTo(4));
            Assert.That(mergedTree.Lookup("e"), Is.EqualTo(5));
            Assert.That(mergedTree.Lookup("f"), Is.EqualTo(6));
        });
    }
}
