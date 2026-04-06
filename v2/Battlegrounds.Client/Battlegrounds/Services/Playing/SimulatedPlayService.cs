using Battlegrounds.Models.Playing;
using Battlegrounds.Services.Infrastructure;
using Battlegrounds.Services.Playing.Common;

namespace Battlegrounds.Services.Playing;

public sealed class SimulationParameters {
    public bool LaunchSuccessful { get; init; } = true;
    public required SimulatedAppRunParameters RunParameters { get; init; }
}

/// <summary>
/// Provides a simulated implementation of the IPlayService interface for testing and development purposes.
/// </summary>
/// <remarks>This class enables developers to test game mode building and launching workflows without interacting
/// with real game applications or servers. It is intended for use in test environments where actual game execution is
/// not required.</remarks>
/// <param name="coh3Archiver">The archiver service used to manage game archives during simulation.</param>
/// <param name="logger">The logger used to record diagnostic information during simulation.</param>
public sealed class SimulatedPlayService(CoH3ArchiverService coh3Archiver, SimulationParameters simulationParameters) : AbstractPlayService(coh3Archiver) {

    public override Task EnsureModSourceIsAvailable() => Task.CompletedTask;

    public override Task<LaunchGameAppResult> LaunchGameApp(Game game) => Task.FromResult(new LaunchGameAppResult {
        Failed = !simulationParameters.LaunchSuccessful,
        ErrorMessage = string.Empty,
        GameInstance = new SimulatedAppInstance(game, simulationParameters.LaunchSuccessful, simulationParameters.RunParameters, _activePlayLobby ?? throw new InvalidOperationException("No active lobby available"))
    });

}
