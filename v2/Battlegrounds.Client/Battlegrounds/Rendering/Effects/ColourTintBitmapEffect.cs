using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Battlegrounds.Rendering.Effects;

public sealed class ColourTintBitmapEffect : ShaderEffect {

    private static readonly PixelShader _shader = new PixelShader {
        UriSource = new Uri("Battlegrounds;component/Rendering/Effects/ColourTintShader.ps", UriKind.Relative)
    };

    public static readonly DependencyProperty InputProperty =
        ShaderEffect.RegisterPixelShaderSamplerProperty(nameof(Input), typeof(ColourTintBitmapEffect), 0);

    public static readonly DependencyProperty TintColourProperty =
        DependencyProperty.Register(nameof(TintColour), typeof(Color), typeof(ColourTintBitmapEffect),
            new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

    public Brush Input {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public Color TintColour {
        get => (Color)GetValue(TintColourProperty);
        set => SetValue(TintColourProperty, value);
    }

    public ColourTintBitmapEffect() {
        PixelShader = _shader;
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(TintColourProperty);
    }

}
