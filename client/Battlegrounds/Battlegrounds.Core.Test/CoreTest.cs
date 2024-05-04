global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using NSubstitute;

namespace Battlegrounds.Core.Test;

[SetUpFixture]
public static class CoreTest {

#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
    public static IServiceProvider Services { get; private set; }
#pragma warning restore NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

    [OneTimeSetUp]
    public static void Setup() {
        Services = new ServiceCollection().AddLogging()
            .BuildServiceProvider();
    }

}
