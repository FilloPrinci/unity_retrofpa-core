// Minimal 2-color vertical gradient skybox (horizon -> zenith), no
// atmospheric scattering. Skybox/Procedural (URP's bundled physically-based
// sky) can't produce a flat/matte retro look - low Atmosphere Thickness
// darkens the zenith toward black instead of flattening the gradient, since
// it's modeling real Rayleigh scattering, not a stylistic tint.
// Pair with VisualStyleProfile.skyboxHorizonColor set close to fogColor so
// geometry fades into the sky instead of cutting against a mismatched sky.
Shader "Retro FPA/Gradient Skybox"
{
    Properties
    {
        _HorizonColor ("Horizon Color", Color) = (0.5, 0.5, 0.5, 1)
        _ZenithColor ("Zenith Color", Color) = (0.5, 0.5, 0.5, 1)
        _Curve ("Blend Curve", Range(0.1, 8)) = 1
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

            half4 _HorizonColor;
            half4 _ZenithColor;
            half _Curve;
            half _Exposure;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 directionWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                // The skybox mesh is centered on the camera, so its object-space
                // position IS the view direction - no need for a camera position.
                output.directionWS = TransformObjectToWorldDir(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 dir = normalize(input.directionWS);
                half t = pow(saturate(dir.y), _Curve);
                half4 color = lerp(_HorizonColor, _ZenithColor, t);
                return color * _Exposure;
            }
            ENDHLSL
        }
    }
}
