namespace Battlegrounds.Services;

/// <summary>
/// Applies the user's UI scale by swapping the design system's size tokens.
/// </summary>
/// <remarks>Scaling is done by overriding <c>Space.*</c>, <c>Size.*</c> and <c>Font.Size.*</c> with a
/// hand-authored set of whole-pixel values, not by transforming the visual tree. A
/// <see cref="System.Windows.Media.ScaleTransform"/> would put glyph origins back on fractional device
/// pixels and undo the crispness work described in CLAUDE.md.
///
/// Only tokens referenced with <c>{DynamicResource}</c> respond to a change. That is already true
/// throughout <c>Themes/</c>, so every control style and named text style scales for free; literal
/// sizes still sitting in <c>Views/</c> do not.</remarks>
public interface IUiScaleService {

    /// <summary>
    /// The scale currently applied to the application resources.
    /// </summary>
    string CurrentScale { get; }

    /// <summary>
    /// The multiplier the current scale represents, e.g. 1.25 for "125%". Useful for sizing
    /// things WPF cannot express as a resource, such as a window minimum.
    /// </summary>
    double CurrentFactor { get; }

    /// <summary>
    /// Raised after a new scale has been applied to the application resources.
    /// </summary>
    event EventHandler? ScaleChanged;

    /// <summary>
    /// Applies the given scale, replacing any previously applied overlay. An unrecognised value
    /// falls back to 100% rather than throwing — a hand-edited config should not stop the app.
    /// </summary>
    /// <param name="scale">One of <c>AvailableScales</c>, e.g. "125%".</param>
    void Apply(string? scale);

}
