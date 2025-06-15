using System.Text;

using Battlegrounds.Parsers;

namespace Battlegrounds.Test.Parsers;

[TestFixture, TestOf(typeof(LocaleParser))]
public sealed class LocaleParserTests {

    private LocaleParser _parser = null!;

    [SetUp]
    public void SetUp() {
        _parser = new LocaleParser();
    }

    [Test]
    public void ParseLocalesAsync_NullStream_ThrowsArgumentNullException() {
        Assert.ThrowsAsync<ArgumentNullException>(() => _parser.ParseLocalesAsync(null!));
    }

    [Test]
    public void ParseLocalesAsync_EmptyStream_ThrowsArgumentException() {
        using var stream = new MemoryStream();
        Assert.ThrowsAsync<ArgumentException>(() => _parser.ParseLocalesAsync(stream));
    }

    [Test]
    public void ParseLocalesAsync_InvalidStream_ThrowsYamlException() {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("invalid: yaml: content"));
        stream.Position = 0; // Reset stream position to the beginning
        var result = Assert.ThrowsAsync<InvalidDataException>(() => _parser.ParseLocalesAsync(stream));
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Message, Does.Contain("The provided stream does not contain valid YAML content."));
    }

    [Test]
    public async Task ParseLocalesAsync_ValidStream_ReturnsCorrectDictionary() {
        using var stream = File.OpenRead("TestData/Locales/sample-english-only.yaml");
        var result = await _parser.ParseLocalesAsync(stream);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result, Contains.Key(Consts.UCS_LANG_ENGLISH));
        Assert.That(result[Consts.UCS_LANG_ENGLISH], Has.Count.EqualTo(11));
        Assert.That(result[Consts.UCS_LANG_ENGLISH][11153223], Is.EqualTo("Infantry Section"));
    }

}
