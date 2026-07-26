Shader "PR3D/DynamicFoodColor"
{
    Properties
    {
        _BaseColor ("Production Color", Color) = (1, 0.55, 0.08, 1)
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
                half4 _BaseColor;
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
                half shade = 0.68h + upFacing * 0.32h + keyLight * 0.08h;
                half3 pastel = lerp(
                    _BaseColor.rgb,
                    half3(1.0h, 1.0h, 1.0h),
                    0.12h);
                return half4(saturate(pastel * shade), _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
