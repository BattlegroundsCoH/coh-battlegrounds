using System.Diagnostics;

using Battlegrounds.Game;
using Battlegrounds.Networking;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.Factory;
using Battlegrounds.Networking.Server;

using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Battlegrounds.Testing.Core.Networking;

public abstract class TestWithServer : IDisposable {

    private readonly IContainer container;

    private readonly bool isGithub;

    protected readonly NetworkEndpoint endpoint;

    protected readonly ServerAPI serverAPI;
    
    public TestWithServer() {
        
        if (Environment.GetEnvironmentVariable("TEST_LOCATION") is "github") {
            isGithub = true;
            return;
        }

        container = new ContainerBuilder()
            .WithImage("ghcr.io/battlegroundscoh/coh-battlegrounds-networking:latest")
            .WithPortBinding(80, true)
            .WithPortBinding(11000, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Started the CoH:Battlegrounds server with TCP and HTTP servers active"))
            .Build();

        TestcontainersSettings.Logger = ConsoleLogger.Instance;

        container.StartAsync().GetAwaiter().GetResult();
        endpoint = new NetworkEndpoint(container.Hostname, container.GetMappedPublicPort(80), container.GetMappedPublicPort(11000));
        serverAPI = new ServerAPI(endpoint.RemoteIPAddress, endpoint.Http, true);

    }

    [SetUp] 
    public void SetUpGithubCheck() { 
        if (isGithub) {
            Assert.Inconclusive("Cannot run server integration tests in githb");
        }
    }

    public void Dispose() {
        container?.StopAsync().GetAwaiter().GetResult();
    }

    protected ILobbyHandle HostTestLobby(string name, string? password, GameCase game, string mod, ILobbyFactory factory) {
        bool waiting = true;
        ILobbyHandle? handle = null;
        factory.HostLobby(name, password, game, mod).Then(x => { handle = x; waiting = false; }).Else(() => { Assert.Fail("Failed to establish connection."); waiting = false; });
        while (waiting) {
            Thread.Sleep(50);
        }
        return handle ?? throw new Exception();
    }

    protected ILobbyHandle JoinTestLobby(ILobbyFactory factory) {
        return null;
    }

    protected void WaitFor(TimeSpan duration) => Thread.Sleep(duration);

    protected void WaitUntil(Func<bool> condition, TimeSpan timeout) { 
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.ElapsedMilliseconds < timeout.TotalMilliseconds) {
            Thread.Sleep(100);
        }
        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds >= timeout.TotalMilliseconds) {
            Assert.Fail("Wait for event timed out");
        }
    }

}
