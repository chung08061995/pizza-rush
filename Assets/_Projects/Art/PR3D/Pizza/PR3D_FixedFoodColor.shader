Shader "PR3D/FixedFoodColor"
{
    Properties
    {
        _FixedFoodTint ("Fixed Food Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

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
                half3 normalWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _FixedFoodTint;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half upFacing = saturate(normalWS.y * 0.5h + 0.5h);
                half keyLight = saturate(dot(
                    normalWS,
                    normalize(half3(-0.38h, 0.84h, -0.38h))));
                half shade = 0.70h + upFacing * 0.30h + keyLight * 0.08h;
                return half4(
                    saturate(_FixedFoodTint.rgb * shade),
                    _FixedFoodTint.a);
            }
            ENDHLSL
        }
    }
}
