using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Converters;

/// <summary>
/// Represents a base class for types that depend on an application-wide <see cref="IServiceProvider"/>.
/// </summary>
/// <remarks>This class provides access to an <see cref="IServiceProvider"/> instance, which is either obtained 
/// from the current application instance or initialized with a default service provider if the application  instance is
/// not available. Derived classes can use this service provider to resolve dependencies.</remarks>
public abstract class AbstractAppDependable {

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance associated with the application.
    /// </summary>
    protected IServiceProvider ServiceProvider => _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractAppDependable"/> class.
    /// </summary>
    /// <remarks>This constructor attempts to retrieve the application's service provider from the <see
    /// cref="BattlegroundsApp.Instance"/>. If the instance is not initialized or its service provider is null, an <see
    /// cref="InvalidOperationException"/> is thrown. If no instance of <see cref="BattlegroundsApp"/> is available, a
    /// new service provider is created using a default <see cref="ServiceCollection"/>.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="BattlegroundsApp.Instance"/> is initialized but its <see
    /// cref="BattlegroundsApp.ServiceProvider"/> is null.</exception>
    public AbstractAppDependable() {
        if (BattlegroundsApp.Instance is BattlegroundsApp app)
            _serviceProvider = app.ServiceProvider ?? throw new InvalidOperationException("BattlegroundsApp instance is not initialized or ServiceProvider is null.");
        else
            _serviceProvider = new ServiceCollection().BuildServiceProvider();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractAppDependable"/> class with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance used to resolve application dependencies.  This parameter cannot be
    /// <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public AbstractAppDependable(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "ServiceProvider cannot be null.");
    }

}
