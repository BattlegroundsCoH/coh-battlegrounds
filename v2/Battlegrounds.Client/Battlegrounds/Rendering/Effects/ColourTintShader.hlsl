// fxc /T ps_3_0 /E main /Fo ColourTintShader.ps ColourTintShader.hlsl

sampler2D input : register(s0);
float4 tintColor : register(c0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 gray = tex2D(input, uv);
    float luminance = gray.r; // Assumes grayscale input
    return float4(luminance * tintColor.rgb, gray.a);
}
