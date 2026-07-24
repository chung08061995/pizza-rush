Shader "PR3D/TerracottaTileFloor"
{
    Properties
    {
        _TileColor ("Tile Color", Color) = (0.38, 0.115, 0.045, 1)
        _AlternateColor ("Alternate Tile", Color) = (0.31, 0.075, 0.03, 1)
        _GroutColor ("Grout Color", Color) = (0.085, 0.018, 0.014, 1)
        _TileSize ("Tile Size", Float) = 0.72
        _GroutWidth ("Grout Width", Range(0.01, 0.16)) = 0.055
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

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TileColor;
                half4 _AlternateColor;
                half4 _GroutColor;
                float _TileSize;
                float _GroutWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 cell = input.positionWS.xz / max(_TileSize, 0.01);
                float2 grid = frac(cell);
                float edge = min(min(grid.x, 1.0 - grid.x), min(grid.y, 1.0 - grid.y));
                float groutMask = smoothstep(_GroutWidth * 0.55, _GroutWidth, edge);
                float checker = fmod(floor(cell.x) + floor(cell.y), 2.0);
                half4 tile = lerp(_TileColor, _AlternateColor, checker * 0.34);
                return lerp(_GroutColor, tile, groutMask);
            }
            ENDHLSL
        }
    }
}
