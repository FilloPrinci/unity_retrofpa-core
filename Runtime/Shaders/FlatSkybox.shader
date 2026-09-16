// Minimal flat-color skybox: outputs a single tint everywhere, no atmospheric
// scattering. Skybox/Procedural (URP's bundled physically-based sky) can't
// produce this - low Atmosphere Thickness darkens the zenith toward black
// instead of flattening the gradient, since it's modeling real Rayleigh
// scattering. A retro flat sky needs a dedicated shader, not tweaked physics.
// Pair with VisualStyleProfile.skyboxColor set close to fogColor so geometry
// fades into the sky instead of cutting against a mismatched horizon.
Shader "Retro FPA/Flat Skybox"
{
    Properties
    {
        _Tint ("Tint", Color) = (0.5, 0.5, 0.5, 1)
        _Exposure ("Exposure", Range(0, 8)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _Tint;
            half _Exposure;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _Tint * _Exposure;
            }
            ENDHLSL
        }
    }
}
