using Battlegrounds.Game;
using Battlegrounds.Networking;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.Factory;
using Battlegrounds.Networking.Server;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Battlegrounds.Testing.Core.Networking;

public abstract class TestWithServer : IDisposable {

    private readonly IContainer container = new ContainerBuilder()
            .WithImage("ghcr.io/battlegroundscoh/coh-battlegrounds-networking:v0.0.3")
            .WithPortBinding(80, true)
            .WithPortBinding(11000, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Started the CoH:Battlegrounds server with TCP and HTTP servers active"))
            .Build();

    protected readonly NetworkEndpoint endpoint;

    protected readonly ServerAPI serverAPI;
    
    public TestWithServer() {
        container.StartAsync().GetAwaiter().GetResult();
        endpoint = new NetworkEndpoint(container.Hostname, container.GetMappedPublicPort(80), container.GetMappedPublicPort(11000));
        serverAPI = new ServerAPI(endpoint.RemoteIPAddress, endpoint.Http, true);
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

}
