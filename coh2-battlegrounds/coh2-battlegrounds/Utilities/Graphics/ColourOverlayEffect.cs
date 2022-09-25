using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace BattlegroundsApp.Utilities.Graphics;

public class ColourOverlayEffect : ShaderEffect {

    private static readonly PixelShader __shader = new PixelShader() {
        UriSource = new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/shaders/ColourOverlayEffect.ps")
    };

    public static readonly DependencyProperty InputProperty =
        ShaderEffect.RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ColourOverlayEffect), 0);

    public Brush Input {
        get => (Brush)GetValue(InputProperty);
        set => SetCurrentValue(InputProperty, value);
    }

    public static readonly DependencyProperty OverlayProperty =
        DependencyProperty.Register(nameof(Overlay), typeof(Color), typeof(ColourOverlayEffect),
                new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

    public Color Overlay {
        get => (Color)GetValue(OverlayProperty);
        set => SetCurrentValue(OverlayProperty, value);
    }

    public ColourOverlayEffect() : this(Colors.White) { }

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
