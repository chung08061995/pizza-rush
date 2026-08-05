Shader "PizzaRush/Pizza Quarter Vertex Color"
{
    Properties
    {
        _GameplayColor ("Gameplay Color", Color) = (1, 1, 1, 1)
        _CompletionFlash ("Completion Flash", Range(0, 2)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _GameplayColor;
                half _CompletionFlash;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half linerMask = saturate(input.color.a);
                half3 baseColor = lerp(input.color.rgb, input.color.rgb * _GameplayColor.rgb, linerMask);

                // Stable mobile-friendly shaping: bright tops, gently shaded sides.
                half topLight = saturate(dot(normalize(input.normalWS), normalize(half3(-0.35h, 0.85h, -0.25h))));
                half lighting = 0.72h + topLight * 0.28h;
                half3 flashColor = half3(1.0h, 0.72h, 0.24h) * _CompletionFlash;
                return half4(baseColor * lighting + flashColor, 1.0h);
            }
            ENDHLSL
        }
    }
}
