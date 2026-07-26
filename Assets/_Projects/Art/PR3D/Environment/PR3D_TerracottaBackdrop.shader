Shader "PR3D/TerracottaBackdrop"
{
    Properties
    {
        _TileColorA ("Tile Color A", Color) = (0.48, 0.12, 0.045, 1)
        _TileColorB ("Tile Color B", Color) = (0.62, 0.20, 0.07, 1)
        _GroutColor ("Grout Color", Color) = (0.12, 0.045, 0.035, 1)
        _WallTileA ("Wall Tile A", Color) = (0.025, 0.11, 0.34, 1)
        _WallTileB ("Wall Tile B", Color) = (0.035, 0.22, 0.58, 1)
        _WallStartZ ("Wall Start Z", Float) = 9.35
        _TileSize ("Tile Size", Float) = 0.65
        _TileAspectZ ("Tile Z Aspect", Float) = 1.65
        _GroutWidth ("Grout Width", Range(0.01, 0.15)) = 0.03
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry-5" }

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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 tileUV : TEXCOORD0;
                float objectZ : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TileColorA;
                half4 _TileColorB;
                half4 _GroutColor;
                half4 _WallTileA;
                half4 _WallTileB;
                float _WallStartZ;
                float _TileSize;
                float _TileAspectZ;
                float _GroutWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.tileUV = input.positionOS.xz / float2(
                    max(_TileSize, 0.01),
                    max(_TileSize * _TileAspectZ, 0.01));
                output.objectZ = input.positionOS.z;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 tileUV = input.tileUV;
                float row = floor(tileUV.y);
                tileUV.x += fmod(abs(row), 2.0) * 0.5;
                float2 cell = floor(tileUV);
                float2 local = frac(tileUV);
                float edge = min(min(local.x, 1.0 - local.x), min(local.y, 1.0 - local.y));
                float tileMask = smoothstep(_GroutWidth, _GroutWidth + 0.025, edge);
                float variation = fmod(abs(cell.x + cell.y), 2.0);
                half3 floorColor = lerp(_TileColorA.rgb, _TileColorB.rgb, variation * 0.42);
                half3 wallColor = lerp(_WallTileA.rgb, _WallTileB.rgb, variation * 0.46);
                half wallMask = step(_WallStartZ, input.objectZ);
                half3 tileColor = lerp(floorColor, wallColor, wallMask);
                float bevel = smoothstep(_GroutWidth, _GroutWidth + 0.10, edge);
                tileColor *= lerp(0.78, 1.05, bevel);
                half3 grout = lerp(_GroutColor.rgb, _WallTileA.rgb * 0.22, wallMask);
                return half4(lerp(grout, tileColor, tileMask), 1);
            }
            ENDHLSL
        }
    }
}
