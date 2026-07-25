Shader "Custom/ProjectileTrails"
{
    Properties
    {
        [MainColor] _BaseColor("Trail Color", Color) = (1, 0.85, 0.1, 1)
        [MainTexture] _BaseMap("Base Map (soft streak, white on black)", 2D) = "white" {}
        _EmissionIntensity("Glow Intensity", Range(0, 20)) = 4
        _FadePower("Tail Fade Power", Range(0.1, 8)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend One One // additive - makes the trail glow instead of muddying the background

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Trail Renderer / Particle System color-over-lifetime gradient
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half _EmissionIntensity;
                half _FadePower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // Trail Renderer UVs run u=0 at the newest (head) end to u=1 at the
                // oldest (tail) end, so this fades brightness out toward the tail.
                half tailFade = pow(saturate(1.0h - IN.uv.x), _FadePower);

                half3 glow = tex.rgb * _BaseColor.rgb * IN.color.rgb * _EmissionIntensity;
                half alpha = tex.a * _BaseColor.a * IN.color.a * tailFade;

                return half4(glow * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
