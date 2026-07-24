Shader "PR3D/ContainerDynamicBody"
{
    Properties
    {
        _Color ("Container Color", Color) = (0.55, 0.75, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; half rim : TEXCOORD0; };

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
                half3 pastel = lerp(_Color.rgb, half3(1.0h, 1.0h, 1.0h), 0.28h);
                half shade = 0.82h + input.rim * 0.18h;
                return half4(saturate(pastel * shade), _Color.a);
            }
            ENDHLSL
        }
    }
}
