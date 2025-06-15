using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Test.Models.Blueprints.Extensions;

[TestFixture]
public sealed class VeterancyExtensionTests {

    [Test]
    public void VeterancyExtension_Constructor_ShouldInitializeWithDefaultValues() {
        
        // Arrange & Act
        var extension = new VeterancyExtension([]);
        // Assert
        Assert.That(extension.MaxRank, Is.EqualTo(0), "Default VeterancyLevel should be 0.");

    }

    [Test]
    public void VeterancyExtension_MaxRank_ShouldReturnCorrectValue() {
        
        // Arrange
        var extension = new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(100.0f, ""), 
            new VeterancyExtension.VeterancyRank(200.0f, ""), 
            new VeterancyExtension.VeterancyRank(300.0f, ""),
        ]);

        // Act
        int maxRank = extension.MaxRank;
        
        // Assert
        Assert.That(maxRank, Is.EqualTo(3), "MaxRank should return the value set in the constructor.");

    }

    [Test]
    public void VeterancyExtension_GetRank_ShouldReturnCorrectRank() {
        
        // Arrange
        var extension = new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(100.0f, ""), 
            new VeterancyExtension.VeterancyRank(200.0f, ""), 
            new VeterancyExtension.VeterancyRank(300.0f, ""),
        ]);

        // Act & Assert
        Assert.Multiple(() => {
            Assert.That(extension.GetRank(50.0f), Is.EqualTo(0), "Experience below first rank should return 0.");
            Assert.That(extension.GetRank(150.0f), Is.EqualTo(1), "Experience between first and second rank should return 1.");
            Assert.That(extension.GetRank(250.0f), Is.EqualTo(2), "Experience between second and third rank should return 2.");
            Assert.That(extension.GetRank(350.0f), Is.EqualTo(3), "Experience above max rank should return max rank");
        });
    }

    [Test]
    public void VeterancyExtension_GetRankProgress_ShouldReturnCorrectProgress() {
        
        // Arrange
        var extension = new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(100.0f, ""), 
            new VeterancyExtension.VeterancyRank(200.0f, ""), 
            new VeterancyExtension.VeterancyRank(300.0f, ""),
        ]);

        // Act & Assert
        Assert.Multiple(() => {
            Assert.That(extension.GetRankProgress(50.0f), Is.EqualTo(0.5f), "Progress below first rank should be at 0.5");
            Assert.That(extension.GetRankProgress(150.0f), Is.EqualTo(0.5f), "Progress between first and second rank should be 0.5.");
            Assert.That(extension.GetRankProgress(250.0f), Is.EqualTo(0.5f), "Progress between second and third rank should be 0.5.");
            Assert.That(extension.GetRankProgress(350.0f), Is.EqualTo(1.0f), "Progress above max rank should be 1.");
        });
    }

    [Test]
    public void VeterancyExtension_GetNextRankExperience_ShouldReturnCorrectExperience() {
        
        // Arrange
        var extension = new VeterancyExtension([
            new VeterancyExtension.VeterancyRank(100.0f, ""), 
            new VeterancyExtension.VeterancyRank(200.0f, ""), 
            new VeterancyExtension.VeterancyRank(300.0f, ""),
        ]);

        // Act & Assert
        Assert.Multiple(() => {
            Assert.That(extension.GetNextRankExperience(50.0f), Is.EqualTo(100.0f), "Next rank experience below first rank should be 100.");
            Assert.That(extension.GetNextRankExperience(150.0f), Is.EqualTo(200.0f), "Next rank experience between first and second rank should be 200.");
            Assert.That(extension.GetNextRankExperience(250.0f), Is.EqualTo(300.0f), "Next rank experience between second and third rank should be 300.");
            Assert.That(extension.GetNextRankExperience(350.0f), Is.EqualTo(300.0f), "Next rank experience above max rank should be 300.");
        });
    }

}
