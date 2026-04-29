using Battlegrounds.Models.Lobbies;
using Battlegrounds.Test;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.IntegrationTest;

/// <summary>
/// Abstract base class for integration tests that require a running Battlegrounds server (gRPC + HTTP API).
/// Uses the dedicated integration-test Docker image which has authentication disabled and supports
/// <c>x-test-user-id</c> / <c>x-test-user-name</c> metadata for injecting caller identity.
/// </summary>
/// <remarks>
/// Keep integration tests in this project (<c>Battlegrounds.IntegrationTest</c>) separate from unit tests so
/// they can be scheduled independently in CI (Docker required).
/// <para>
/// Run with: <c>dotnet test Battlegrounds.IntegrationTest --filter "Category=Integration"</c>
/// </para>
/// </remarks>
[Category("Integration")]
public abstract class LobbyServerIntegrationTests {

    private const string IntegrationTestImage =
        "ghcr.io/battlegroundscoh/battlegrounds-backend-server/battlegrounds-server-integration-test:latest";

#pragma warning disable NUnit1032
    private readonly TestLogger<LobbyServerIntegrationTests> _containerLogger = new();
    protected IContainer _container = null!;
#pragma warning restore NUnit1032

    /// <summary>Mapped host port for the HTTP REST API (container port 8080).</summary>
    protected ushort HttpApiPort => _container.GetMappedPublicPort(8080);

    /// <summary>Mapped host port for the gRPC LobbyService (container port 8082).</summary>
    protected ushort GrpcPort => _container.GetMappedPublicPort(8082);

    /// <summary>Mapped host port for the admin/health endpoint (container port 8081).</summary>
    protected ushort AdminPort => _container.GetMappedPublicPort(8081);

    protected string ContainerHost => _container.Hostname;

    protected string GrpcAddress => $"http://{ContainerHost}:{GrpcPort}";
    protected string HttpApiBaseUrl => $"http://{ContainerHost}:{HttpApiPort}";

    [OneTimeSetUp]
    public async Task OneTimeSetUp() {
        ServerIssueReporter.Reset();

        _container = new ContainerBuilder()
            .WithImage(IntegrationTestImage)
            .WithPortBinding(8080, true)
            .WithPortBinding(8082, true)
            .WithPortBinding(8081, true)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r
                        .ForPort(8081)
                        .ForPath("/api/v1/health")))
            .WithCleanUp(true)
            .WithOutputConsumer(_containerLogger)
            .Build();

        await _container.StartAsync().ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() {
        try {
            string outputDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults");
            var (jsonPath, markdownPath) = ServerIssueReporter.WriteSummary(outputDirectory, GetType().Name);
            _containerLogger.LogInformation(
                "Server issue summary for {Fixture}: {Count} issue(s). JSON: {JsonPath}. Markdown: {MarkdownPath}.",
                GetType().Name,
                ServerIssueReporter.IssueCount,
                jsonPath,
                markdownPath);
            TestContext.AddTestAttachment(jsonPath, "Server issue summary (JSON)");
            TestContext.AddTestAttachment(markdownPath, "Server issue summary (Markdown)");
        } catch (Exception ex) {
            _containerLogger.LogError(ex, "Failed to write server issue summary for fixture {Fixture}.", GetType().Name);
        }

        _containerLogger.LogInformation("Stopping integration test container.");
        _containerLogger.Dispose();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    protected void ReportServerIssue(string scenario, string expected, string actual, string? details = null) {
        ServerIssueReporter.Report(GetType().Name, scenario, expected, actual, details);
    }

    protected async Task<LobbyEvent?> TryWaitForEventOrReportAsync(
        MultiplayerLobby lobby,
        LobbyEventType eventType,
        string scenario,
        int timeoutMs = 5000,
        string? details = null) {

        var evt = await LobbyIntegrationHarness.TryWaitForEventAsync(lobby, eventType, timeoutMs, scenario);
        if (evt is null) {
            ReportServerIssue(
                scenario,
                $"Lobby should publish {eventType} within {timeoutMs} ms.",
                $"Timed out waiting for {eventType}.",
                details);
        }
        return evt;
    }
}
