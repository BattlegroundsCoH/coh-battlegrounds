using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Test;

/// <summary>
/// Provides a base class for integration tests involving the Battlegrounds server.
/// </summary>
/// <remarks>This class is designed to facilitate integration testing by managing the lifecycle of a containerized
/// instance of the Battlegrounds server. It includes setup logic to initialize the container and prepare shared
/// resources required for the tests. Derived classes should implement specific test cases and can rely on the
/// initialized container for server interactions.</remarks>
public abstract class ServerIntegrationTests {

    private readonly TestLogger<ServerIntegrationTests> _containerlogger = new();

    protected IContainer _container;

    protected ushort IntegrationServerPort => _container.GetMappedPublicPort(8080);

    protected string IntegrationServerHost => _container.Hostname;

    [OneTimeSetUp]
    public async Task OneTimeSetUp() {

        // Create container for the Battlegrounds server
        _container = new ContainerBuilder()
            .WithImage("ghcr.io/battlegroundscoh/battlegrounds-backend-server:main")
            .WithPortBinding(8080, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/api/v1/isalive").ForPort(8080)))
            .WithCleanUp(true)
            .WithOutputConsumer(_containerlogger)
            .Build();

        await _container.StartAsync().ConfigureAwait(false);

    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() {
        _containerlogger.LogInformation("Stopping and disposing of the Battlegrounds server container.");
        _containerlogger.Dispose();
        await _container.StopAsync();
    }

}
