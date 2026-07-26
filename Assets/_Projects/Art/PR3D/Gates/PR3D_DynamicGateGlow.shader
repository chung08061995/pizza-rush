Shader "PR3D/DynamicGateGlow"
{
    Properties
    {
        _Color ("Gate Color", Color) = (1, 0.08, 0.55, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry+10" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half rim : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(TransformObjectToWorld(input.positionOS.xyz));
                output.rim = 1.0h - saturate(dot(normalWS, viewWS));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 shadowColor = _Color.rgb * 0.58h;
                half3 highlightColor = saturate(_Color.rgb * 1.08h + 0.055h);
                half3 housingColor = lerp(shadowColor, highlightColor, 0.28h + input.rim * 0.72h);
                return half4(housingColor, _Color.a);
            }
            ENDHLSL
        }
    }
}
