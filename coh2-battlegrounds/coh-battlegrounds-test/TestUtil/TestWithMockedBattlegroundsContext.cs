using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

using Moq;

namespace Battlegrounds.Testing.TestUtil;

public abstract class TestWithMockedBattlegroundsContext {

    protected Mock<IModDbManager> mockDbManager;
    protected Mock<IModManager> mockModManager;
    protected Mock<IScenarioList> mockScenarioList;
    protected Mock<IGamemodeList> mockGamemodeList;

    [SetUp]
    public void Awake() {

        // Initialise mocks
        this.mockDbManager = new Mock<IModDbManager>();
        this.mockModManager = new Mock<IModManager>();
        this.mockScenarioList = new Mock<IScenarioList>();
        this.mockGamemodeList = new Mock<IGamemodeList>();

        // Inject into context
        BattlegroundsContext.ChangeDataSource(this.mockDbManager.Object); // <-- and this means we cannot run tests in parallel
        BattlegroundsContext.ChangeModManager(this.mockModManager.Object); // <-- and this means we cannot run tests in parallel

    }

}
