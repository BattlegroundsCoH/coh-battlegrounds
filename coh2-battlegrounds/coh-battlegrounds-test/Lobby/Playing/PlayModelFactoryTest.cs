using Battlegrounds.Game;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Playing;
using Battlegrounds.Networking.Communication.Connections;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Steam;
using Battlegrounds.Util.Threading;

using Moq;

namespace Battlegrounds.Testing.Lobby.Playing;

[TestFixture]
public class PlayModelFactoryTest {

    private LocalLobbyHandle _localLobbyHandle;
    private Mock<ILobbyHandle> _singleLobbyHandleMock;
    private Mock<ILobbyHandle> _onlineLobbyHandleMock;
    private Mock<IChatSpectator> _lobbyChatMock;
    private Mock<IDispatcher> _dispatcherMock;
    private UploadProgressCallbackHandler _callbackHandler;

    [SetUp]
    public void SetUp() {

        // Create basic lobby handle
        _localLobbyHandle = new LocalLobbyHandle(SteamUser.CreateTempUser(10000000, "Alfredo"));

        // Mock the ILobbyHandle for a local lobby
        _singleLobbyHandleMock = new Mock<ILobbyHandle>();
        _singleLobbyHandleMock.Setup(m => m.GetPlayerCount(It.IsAny<bool>())).Returns(1);

        // Mock the ILobbyHandle for an online lobby
        _onlineLobbyHandleMock = new Mock<ILobbyHandle>();
        _onlineLobbyHandleMock.Setup(m => m.GetPlayerCount(It.IsAny<bool>())).Returns(2);

        _dispatcherMock = new Mock<IDispatcher>();
        _lobbyChatMock = new Mock<IChatSpectator>();
        _callbackHandler = (_, _, _) => { };

        // Setup mock
        _singleLobbyHandleMock.Setup(x => x.Game).Returns(GameCase.CompanyOfHeroes3);
        _onlineLobbyHandleMock.Setup(x => x.Game).Returns(GameCase.CompanyOfHeroes3);

    }

    [Test]
    public void GetModel_GivenLocalLobbyHandle_ReturnsSingleplayerModel() {

        // Arrange
        ILobbyHandle handle = _localLobbyHandle;
        IChatSpectator lobbyChat = _lobbyChatMock.Object;
        uint cancelTime = 0;
        UploadProgressCallbackHandler? callbackHandler = null;

        // Act
        var model = PlayModelFactory.GetModel(handle, lobbyChat, _dispatcherMock.Object, cancelTime, callbackHandler);

        // Assert
        Assert.That(model, Is.InstanceOf<SingleplayerModel>());

    }

    [Test]
    public void GetModel_GivenOnePlayerLobbyHandle_ReturnsSingleplayerModel() {

        // Arrange
        ILobbyHandle handle = _singleLobbyHandleMock.Object;
        IChatSpectator lobbyChat = _lobbyChatMock.Object;
        uint cancelTime = 0;
        UploadProgressCallbackHandler? callbackHandler = null;

        // Act
        var model = PlayModelFactory.GetModel(handle, lobbyChat, _dispatcherMock.Object, cancelTime, callbackHandler);

        // Assert
        Assert.That(model, Is.InstanceOf<SingleplayerModel>());

    }

    [Test]
    public void GetModel_GivenMoreThanOnePlayerOnline_ReturnsOnlineModel() {

        // Arrange
        ILobbyHandle handle = _onlineLobbyHandleMock.Object;
        IChatSpectator lobbyChat = _lobbyChatMock.Object;
        uint cancelTime = 0;
        UploadProgressCallbackHandler? callbackHandler = _callbackHandler;

        // Act
        var model = PlayModelFactory.GetModel(handle, lobbyChat, _dispatcherMock.Object, cancelTime, callbackHandler);

        // Assert
        Assert.That(model, Is.InstanceOf<OnlineModel>());

    }

    [Test]
    public void GetModel_GivenNullCallbackHandler_ThrowsException() {

        // Arrange
        ILobbyHandle handle = _onlineLobbyHandleMock.Object;
        IChatSpectator lobbyChat = _lobbyChatMock.Object;
        uint cancelTime = 0;
        UploadProgressCallbackHandler? callbackHandler = null;

        // Act & Assert
        Assert.Throws<Exception>(() => PlayModelFactory.GetModel(handle, lobbyChat, _dispatcherMock.Object, cancelTime, callbackHandler));

    }

}
