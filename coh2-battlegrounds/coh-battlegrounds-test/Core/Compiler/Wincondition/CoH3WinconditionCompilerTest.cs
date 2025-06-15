using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Locale.CoH3;
using Battlegrounds.Compiler.Source.CoH3;
using Battlegrounds.Compiler.Wincondition.CoH3;
using Battlegrounds.Game;
using Battlegrounds.Game.Match;

using Moq;

namespace Battlegrounds.Testing.Core.Compiler.Wincondition;

[TestFixture]
public class CoH3WinconditionCompilerTest {

    private static readonly string workDir = "compiler_test_coh3";
    private static readonly string sgaPath = 
        $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 3\\mods\\extension\\subscriptions\\battlegrounds\\coh3_battlegrounds_wincondition.sga";

    private CoH3WinconditionCompiler compiler;
    private CoH3LocalSourceProvider localSourceProvider;
    private ISession session;
    private Mock<IArchiver> archiver;

    [SetUp]
    public void SetUp() {
        
        if (Directory.Exists(workDir)) {
            Directory.Delete(workDir, true);
        }
        
        Directory.CreateDirectory(workDir);
        
        archiver = new Mock<IArchiver>();
        localSourceProvider = new CoH3LocalSourceProvider("bg_common\\bg_wc\\coh3");
        compiler = new CoH3WinconditionCompiler(workDir, new CoH3LocaleCompiler(), archiver.Object);

        session = Session.CreateSession(new SessionInfo() {
            Entities = Array.Empty<SessionPlanEntityInfo>(),
            Squads = Array.Empty<SessionPlanSquadInfo>(),
            Goals = Array.Empty<SessionPlanGoalInfo>(),
            Allies = new SessionParticipant[] {
                new SessionParticipant("Alfredo", 700000000000, null, ParticipantTeam.TEAM_ALLIES, 0, 0),
            },
            Axis = new SessionParticipant[] {
                new SessionParticipant("Alfredo's Friend", 700000000001, null, ParticipantTeam.TEAM_AXIS, 0, 1),
            },
            AdditionalOptions = new()
        });

        if (File.Exists(sgaPath)) {
            File.Delete(sgaPath);
        }

    }

    [Test]
    public void CanGetArchivePath() {
        Assert.That(compiler.GetArchivePath(), Is.EqualTo(sgaPath));
    }

    [Test]
    public void WillCompile() {
        archiver.Setup(x => x.Archive("compiler_test_coh3\\ArchiveDefinition.json")).Returns(true);

        var result = compiler.CompileToSga("session.scar", session, localSourceProvider);

        Assert.Multiple(() => {

            // Assert actually working
            Assert.That(result, Is.True);

            // Assert *core* files
            Assert.That(File.Exists("compiler_test_coh3\\ArchiveDefinition.json"), Is.True);
            Assert.That(File.Exists("compiler_test_coh3\\data\\info\\mod.bin"), Is.True);
            Assert.That(File.Exists("compiler_test_coh3\\data\\scar\\winconditions\\battlegrounds.bin"), Is.True);
            Assert.That(File.Exists("compiler_test_coh3\\data\\scar\\winconditions\\battlegrounds.scar"), Is.True);

        });

    }

    [Test]
    public void WillCompileFully() {
        Precondition.RequiresNotGithubActions("Requires Essence Editor installed");
        string coh3 = Precondition.RequiresEssenceEditor();

        BattlegroundsArchiver battlegroundsArchiver = new BattlegroundsArchiver(Path.GetDirectoryName(coh3)!, GameCase.CompanyOfHeroes3);

        bool success = false;
        archiver.Setup(x => x.Archive("compiler_test_coh3\\ArchiveDefinition.json")).Callback(() => {
            success = battlegroundsArchiver.Archive("compiler_test_coh3\\ArchiveDefinition.json");
        }).Returns(() => success);

        var result = compiler.CompileToSga("session.scar", session, localSourceProvider);

        Assert.Multiple(() => {

            // Assert archived
            Assert.That(result, Is.True);

            // Assert file is there
            Assert.That(File.Exists(compiler.GetArchivePath()), Is.True);

        });

    }

}
