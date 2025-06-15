using Battlegrounds.Models;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

namespace Battlegrounds.Test.Services;

[TestOf(nameof(CoH3ArchiverService))]
public class CoH3ArchiverServiceTests {

    private Configuration _cfg = null!;
    private CoH3ArchiverService _archiverService = null!;

    [SetUp]
    public void SetUp() {
        _cfg = new Configuration {
            CoH3 = new Configuration.CoH3Configuration {
                InstallPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3",
                ModProjectPath = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\bg_wincondition.coh3mod",
                MatchDataPath = "E:\\coh3-dev\\coh3-bg-wincondition\\bg_wincondition\\assets\\scar\\winconditions\\match_data.scar",
                ModBuildPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "CoHBattlegrounds", "build", "coh3")
            }
        };
        _archiverService = new CoH3ArchiverService(new CoH3(_cfg), _cfg, new TestLogger<CoH3ArchiverService>(), new TestLogger<CoH3ArchiverService.EssenceEditor>());
    }

    [Explicit, Test, Category("Integration")]
    public async Task TestCoH3Archiver() {
        bool built = await _archiverService.CreateModArchiveAsync(_cfg.CoH3.ModProjectPath);
        Assert.That(built, Is.True, "Mod archive should be built successfully.");
    }

}
