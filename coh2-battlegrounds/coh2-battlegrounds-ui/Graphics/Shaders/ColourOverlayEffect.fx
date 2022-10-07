sampler2D implicitInput : register(S0);

float4 overlay : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    return tex2D(implicitInput, uv) * overlay;
}
