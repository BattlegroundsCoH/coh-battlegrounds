using Battlegrounds.Util;

namespace Battlegrounds.Testing.Core.Util;

[TestFixture]
public class MergedSearchTreeTests {

    [Test]
    public void Lookup_ReturnsValueFromFirstTree_IfPathExistsInFirstTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 }
        };
        var tree2 = new SearchTree<int> {
            { "b", 2 }
        };
        var mergedTree = new MergedSearchTree<int>(tree1, tree2);

        // Act
        var result = mergedTree.Lookup("a");

        // Assert
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void Lookup_ReturnsValueFromSecondTree_IfPathDoesNotExistInFirstTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 }
        };
        var tree2 = new SearchTree<int> {
            { "b", 2 }
        };
        var mergedTree = new MergedSearchTree<int>(tree1, tree2);

        // Act
        var result = mergedTree.Lookup("b");

        // Assert
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Lookup_ReturnsDefault_IfPathDoesNotExistInAnyTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 }
        };
        var tree2 = new SearchTree<int> {
            { "b", 2 }
        };
        var mergedTree = new MergedSearchTree<int>(tree1, tree2);

        // Act
        var result = mergedTree.Lookup("c");

        // Assert
        Assert.That(result, Is.EqualTo(default(int)));
    }

    [Test]
    public void Lookup_OutParameterReturnsTrue_IfPathExistsInFirstTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 }
        };
        var tree2 = new SearchTree<int> {
            { "b", 2 }
        };
        var mergedTree = new MergedSearchTree<int>(tree1, tree2);

        // Act
        var success = mergedTree.Lookup("a", out int result);

        // Assert
        Assert.Multiple(() => {
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(1));
        });
    }

    [Test]
    public void Lookup_OutParameterReturnsTrue_IfPathExistsInSecondTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 }
        };
        var tree2 = new SearchTree<int> {
            { "b", 2 }
        };
        var mergedTree = new MergedSearchTree<int>(tree1, tree2);

        // Act
        var success = mergedTree.Lookup("b", out int result);

        // Assert
        Assert.Multiple(() => {

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(2));
        });
    }

    [Test]
    public void Lookup_OutParameterReturnsFalse_IfPathDoesNotExistInAnyTree() {
        // Arrange
        var tree1 = new SearchTree<int> {
            { "a", 1 }
        };
        var tree2 = new SearchTree<int> {
            { "b", 2 }
        };
        var mergedTree = new MergedSearchTree<int>(tree1, tree2);

        // Act
        var success = mergedTree.Lookup("c", out int result);

        // Assert
        Assert.Multiple(() => {
            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(int)));
        });
    }

    [Test]
    public void Count_ReturnsCorrectNumberOfTreesMerged() {
        // Arrange
        var tree1 = new SearchTree<int>();
        var tree2 = new SearchTree<int>();
        var tree3 = new SearchTree<int>();
        var mergedTree = new MergedSearchTree<int>(tree1, tree2, tree3);

        // Act
        var result = mergedTree.Count;

        // Assert
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void Add_ThrowsNotSupportedException() {
        // Arrange
        var mergedTree = new MergedSearchTree<int>();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => mergedTree.Add("foo", 1));
    }

}
