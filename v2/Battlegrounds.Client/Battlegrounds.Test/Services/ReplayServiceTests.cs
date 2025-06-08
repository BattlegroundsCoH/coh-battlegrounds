using Battlegrounds.Parsers;
using Battlegrounds.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Test.Services;

[TestFixture]
public class ReplayServiceTests {

    private ServiceProvider _serviceProvider;
    private ReplayService _replayService;

    [SetUp]
    public void Setup() {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<CoH3ReplayParser>()
            .AddSingleton<ReplayService>()
            .BuildServiceProvider();
        _replayService = _serviceProvider.GetRequiredService<ReplayService>();
    }

    [TearDown]
    public void TearDown() {
        _serviceProvider.Dispose();
    }

    // TODO: Tests

}
