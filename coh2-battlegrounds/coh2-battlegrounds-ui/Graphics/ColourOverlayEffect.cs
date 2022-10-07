using System;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;

namespace Battlegrounds.UI.Graphics;

/// <summary>
/// Shader effect adding a colour overlay on top of the underlying element.
/// </summary>
public class ColourOverlayEffect : ShaderEffect {

    private static readonly PixelShader __shader = new PixelShader() {
        UriSource = new Uri("pack://application:,,,/coh2-battlegrounds-ui;component/Graphics/Shaders/ColourOverlayEffect.ps")
    };

    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ColourOverlayEffect), 0);

    public Brush Input {
        get => (Brush)GetValue(InputProperty);
        set => SetCurrentValue(InputProperty, value);
    }

    public static readonly DependencyProperty OverlayProperty =
        DependencyProperty.Register(nameof(Overlay), typeof(Color), typeof(ColourOverlayEffect),
                new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

    /// <summary>
    /// Get or set the overlay colour
    /// </summary>
    public Color Overlay {
        get => (Color)GetValue(OverlayProperty);
        set => SetCurrentValue(OverlayProperty, value);
    }

    /// <summary>
    /// Initialise a new <see cref="ColourOverlayEffect"/> instance with with <see cref="Colors.White"/> as overlay colour.
    /// </summary>
    public ColourOverlayEffect() : this(Colors.White) { }

    /// <summary>
    /// Initialise 
    /// </summary>
    /// <param name="colour"></param>
    public ColourOverlayEffect(Color colour) {

        // Set colour
        this.Overlay = colour;

        // Set pixelshader
        this.PixelShader = __shader;

        // Update input
        this.UpdateShaderValue(InputProperty);
        this.UpdateShaderValue(OverlayProperty);

    }

}
