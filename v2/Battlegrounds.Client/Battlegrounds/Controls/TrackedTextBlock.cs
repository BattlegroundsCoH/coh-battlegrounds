using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Battlegrounds.Controls;

/// <summary>
/// A single-line text element that can apply letter-spacing (tracking).
/// </summary>
/// <remarks>
/// <para>
/// WPF has no letter-spacing property, and the Battlegrounds design system leans on it
/// heavily — eyebrow labels are tracked at 0.34em, the PLAY NOW call to action at 0.24em.
/// The usual workaround is to interleave hair spaces (U+200A) into the string, which does
/// not work here: neither Oswald nor JetBrains Mono contains that codepoint, so every gap
/// would render as a missing-glyph box.
/// </para>
/// <para>
/// Instead this draws a single <see cref="GlyphRun"/> and adds the tracking to each glyph's
/// own advance width. That keeps the font's real metrics — it is not an approximation — and
/// costs one draw call.
/// </para>
/// <para>
/// The trade-off is that a <see cref="GlyphRun"/> is raw glyph placement: no wrapping, no
/// trimming, no text selection, and no GPOS shaping (kerning, ligatures). That is why this
/// is deliberately not a general <c>TextBlock</c> replacement. Use it for short, static,
/// usually uppercase display strings — headings, eyebrows, field labels, button captions —
/// where tracking is part of the design. Use a plain <c>TextBlock</c> for body copy, for
/// anything that wraps, and for anything showing user-supplied text.
/// </para>
/// <para>
/// With <see cref="Tracking"/> left at 0 this still renders correctly, so it is safe to bind
/// the property to a token and let a theme decide.
/// </para>
/// <para>
/// Everything is snapped to whole device pixels — see <see cref="BuildLayout"/>. That is not
/// a polish detail: the public <see cref="GlyphRun"/> constructor always renders in
/// <c>TextFormattingMode.Ideal</c>, so unlike a <c>TextBlock</c> this element cannot be put
/// into Display mode from a theme, and a baseline landing mid-pixel has no ClearType to hide
/// it — it simply blurs.
/// </para>
/// </remarks>
public sealed class TrackedTextBlock : FrameworkElement {

    public TrackedTextBlock() {
        // The glyph origins BuildLayout produces are only on the pixel grid if the element
        // itself is. Nothing else puts it there: SnapsToDevicePixels does not affect text,
        // and text drawn through DrawGlyphRun never goes near the layout-rounding the text
        // stack applies to a TextBlock.
        UseLayoutRounding = true;
    }

    /// <summary>The text to render. Line breaks are not honoured; this is a single line.</summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Extra space between glyphs, in ems, matching how CSS <c>letter-spacing</c> is
    /// expressed in the design system. 0.34 here is the same as <c>letter-spacing: .34em</c>.
    /// </summary>
    /// <remarks>
    /// Registered as an inheriting attached property rather than a plain one so that a
    /// container can set the tracking for the text inside it. That is what lets a button
    /// style say <c>TrackedTextBlock.Tracking="0.08"</c> on the button itself and have the
    /// label its template generates pick the value up — the label is created by a
    /// ContentPresenter, so the style cannot reach it directly.
    /// </remarks>
    public static readonly DependencyProperty TrackingProperty =
        DependencyProperty.RegisterAttached(
            "Tracking", typeof(double), typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.Inherits
                | FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsRender));

    public static double GetTracking(DependencyObject element)
        => (double)element.GetValue(TrackingProperty);

    public static void SetTracking(DependencyObject element, double value)
        => element.SetValue(TrackingProperty, value);

    // Font and brush properties are added as owners of the TextElement attached properties
    // rather than declared fresh, so they inherit down the visual tree exactly like a
    // TextBlock's do. Setting FontFamily on a window root reaches this element too.
    public static readonly DependencyProperty FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner(typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner(typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(SystemFonts.MessageFontSize,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner(typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(FontWeights.Normal,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner(typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(FontStyles.Normal,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner(typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(FontStretches.Normal,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner(typeof(TrackedTextBlock),
            new FrameworkPropertyMetadata(Brushes.Black,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double Tracking {
        get => (double)GetValue(TrackingProperty);
        set => SetValue(TrackingProperty, value);
    }

    public FontFamily FontFamily {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontWeight FontWeight {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStyle FontStyle {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontStretch FontStretch {
        get => (FontStretch)GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public Brush Foreground {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// The laid-out run, cached between measure and render so the glyph mapping is done
    /// once per layout pass rather than twice.
    /// </summary>
    private Layout? _layout;

    protected override Size MeasureOverride(Size availableSize) {
        _layout = BuildLayout();
        return _layout is null ? new Size(0, 0) : new Size(_layout.Width, _layout.Height);
    }

    /// <summary>
    /// The layout is measured in device pixels, so it stops being valid when the window is
    /// dragged to a monitor at a different scale.
    /// </summary>
    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi) {
        base.OnDpiChanged(oldDpi, newDpi);
        _layout = null;
        InvalidateMeasure();
    }

    protected override void OnRender(DrawingContext drawingContext) {

        // MeasureOverride always runs before render, but a property change that only
        // affects render (Foreground) will not have rebuilt the layout.
        var layout = _layout ??= BuildLayout();
        if (layout is null) {
            return;
        }

        var run = new GlyphRun(
            layout.GlyphTypeface,
            bidiLevel: 0,
            isSideways: false,
            renderingEmSize: layout.FontSize,
            pixelsPerDip: (float)VisualTreeHelper.GetDpi(this).PixelsPerDip,
            glyphIndices: layout.Indices,
            baselineOrigin: new Point(0, layout.Baseline),
            advanceWidths: layout.Advances,
            glyphOffsets: null,
            characters: null,
            deviceFontName: null,
            clusterMap: null,
            caretStops: null,
            language: null);

        drawingContext.DrawGlyphRun(Foreground, run);
    }

    private Layout? BuildLayout() {

        var text = Text;
        if (string.IsNullOrEmpty(text)) {
            return null;
        }

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        if (!typeface.TryGetGlyphTypeface(out var glyphTypeface)) {
            // No glyph typeface means the family failed to resolve — a missing embedded
            // font, say. Rendering nothing would hide the problem in a way that is hard to
            // spot, so fall back to the default UI font instead.
            typeface = new Typeface(SystemFonts.MessageFontFamily, FontStyle, FontWeight, FontStretch);
            if (!typeface.TryGetGlyphTypeface(out glyphTypeface)) {
                return null;
            }
        }

        var fontSize = FontSize;
        var extra = Tracking * fontSize;

        var dpi = VisualTreeHelper.GetDpi(this);

        var indices = new ushort[text.Length];
        var advances = new double[text.Length];

        // Two running positions: the typographically exact one, and the one actually drawn.
        // Each glyph is placed at the exact position rounded to a whole device pixel, and
        // its advance is the gap between consecutive rounded positions. Rounding the
        // positions rather than the advances matters — rounding each advance on its own
        // accumulates, and at Track.Tight on a 24px title the whole tracking value is
        // under half a pixel and would quantise away to nothing. This way every glyph
        // starts on the pixel grid while the run as a whole stays within half a pixel of
        // the width the design asks for.
        var exact = 0.0;
        var placed = 0.0;

        for (var i = 0; i < text.Length; i++) {

            if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(text[i], out var glyph)) {
                // Glyph 0 is .notdef, which draws the font's missing-glyph box. Showing it
                // is intentional: a silently dropped character is harder to notice than a
                // box, and this element is only ever fed strings the design controls.
                glyph = 0;
            }

            indices[i] = glyph;

            // Tracking is added after every glyph including the last, matching how CSS
            // letter-spacing behaves. The trailing gap is then trimmed off the measured
            // width so the element does not carry dead space on its right edge.
            exact += glyphTypeface.AdvanceWidths[glyph] * fontSize + extra;

            var next = Snap(exact, dpi.DpiScaleX);
            advances[i] = next - placed;
            placed = next;
        }

        return new Layout(
            glyphTypeface,
            fontSize,
            indices,
            advances,
            Math.Max(0, Snap(exact - extra, dpi.DpiScaleX)),
            // Ceiling, not round: half a pixel shaved off the bottom clips descenders.
            Ceil(glyphTypeface.Height * fontSize, dpi.DpiScaleY),
            Snap(glyphTypeface.Baseline * fontSize, dpi.DpiScaleY));
    }

    /// <summary>Rounds a device-independent length to the nearest whole device pixel.</summary>
    private static double Snap(double length, double scale)
        => Math.Round(length * scale, MidpointRounding.AwayFromZero) / scale;

    /// <summary>Rounds a device-independent length up to a whole device pixel.</summary>
    private static double Ceil(double length, double scale)
        => Math.Ceiling(length * scale) / scale;

    private sealed record Layout(
        GlyphTypeface GlyphTypeface,
        double FontSize,
        ushort[] Indices,
        double[] Advances,
        double Width,
        double Height,
        double Baseline);

}
