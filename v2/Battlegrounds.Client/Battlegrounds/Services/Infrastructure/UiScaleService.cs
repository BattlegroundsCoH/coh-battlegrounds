using System.Windows;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Infrastructure;

/// <inheritdoc cref="IUiScaleService"/>
public sealed class UiScaleService(ILogger<UiScaleService> logger) : IUiScaleService {

    public const string DefaultScale = "100%";

    /// <summary>
    /// Scale name to overlay dictionary. 100% maps to null: it is the base, expressed by
    /// Metrics.xaml and Typography.xaml themselves, so it applies by removing the overlay.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, (string? Source, double Factor)> Scales =
        new Dictionary<string, (string?, double)>(StringComparer.OrdinalIgnoreCase) {
            [DefaultScale] = (null, 1.00),
            ["110%"] = ("pack://application:,,,/Battlegrounds;component/Themes/Scale/Metrics.Scale110.xaml", 1.10),
            ["125%"] = ("pack://application:,,,/Battlegrounds;component/Themes/Scale/Metrics.Scale125.xaml", 1.25),
            ["150%"] = ("pack://application:,,,/Battlegrounds;component/Themes/Scale/Metrics.Scale150.xaml", 1.50),
        };

    /// <summary>
    /// The selectable scales, in ascending order. Keep in step with the Options on
    /// <see cref="Models.Configuration.UiScale"/>.
    /// </summary>
    public static IReadOnlyList<string> AvailableScales { get; } = [DefaultScale, "110%", "125%", "150%"];

    private readonly ILogger<UiScaleService> _logger = logger;

    /// <summary>The overlay currently merged into Application.Resources, if any.</summary>
    private ResourceDictionary? _applied;

    public string CurrentScale { get; private set; } = DefaultScale;

    public double CurrentFactor { get; private set; } = 1.0;

    public event EventHandler? ScaleChanged;

    public void Apply(string? scale) {

        string requested = scale ?? DefaultScale;
        if (!Scales.TryGetValue(requested, out var target)) {
            _logger.LogWarning("Unknown UI scale '{Scale}', falling back to {Default}.", requested, DefaultScale);
            requested = DefaultScale;
            target = Scales[DefaultScale];
        }

        // No Application in unit tests, or before OnStartup. Record the choice so CurrentScale
        // still reports correctly; there is simply nothing to merge into.
        var resources = Application.Current?.Resources;
        if (resources is null) {
            CurrentScale = requested;
            CurrentFactor = target.Factor;
            return;
        }

        if (_applied is not null) {
            resources.MergedDictionaries.Remove(_applied);
            _applied = null;
        }

        if (target.Source is string source) {
            // Appended last so it wins: WPF searches merged dictionaries in reverse order, and
            // adding to the collection invalidates every {DynamicResource} referencing these keys.
            var overlay = new ResourceDictionary { Source = new Uri(source, UriKind.Absolute) };
            resources.MergedDictionaries.Add(overlay);
            _applied = overlay;
        }

        CurrentScale = requested;
        CurrentFactor = target.Factor;

        _logger.LogInformation("UI scale set to {Scale}.", requested);
        ScaleChanged?.Invoke(this, EventArgs.Empty);
    }

}
