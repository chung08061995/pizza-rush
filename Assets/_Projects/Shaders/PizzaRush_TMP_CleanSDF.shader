Shader "PizzaRush/UI/TMP Clean SDF"
{
    Properties
    {
        [HDR] _FaceColor ("Face Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        [HDR] _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        [HDR] _UnderlayColor ("Underlay Color", Color) = (0,0,0,0.5)
        _UnderlayOffsetX ("Underlay Offset X", Range(-1,1)) = 0
        _UnderlayOffsetY ("Underlay Offset Y", Range(-1,1)) = 0
        _UnderlayDilate ("Underlay Dilate", Range(-1,1)) = 0
        _UnderlaySoftness ("Underlay Softness", Range(0,1)) = 0

        _MainTex ("Font Atlas", 2D) = "white" {}
        _GradientScale ("Gradient Scale", Float) = 5
        _TextureWidth ("Texture Width", Float) = 512
        _TextureHeight ("Texture Height", Float) = 512
        _WeightNormal ("Weight Normal", Float) = 0
        _WeightBold ("Weight Bold", Float) = 0.5
        _ScaleRatioA ("Scale Ratio A", Float) = 1
        _ScaleRatioB ("Scale Ratio B", Float) = 1
        _ScaleRatioC ("Scale Ratio C", Float) = 1
        _Sharpness ("Sharpness", Range(-1,1)) = 0
        _ScaleX ("Scale X", Float) = 1
        _ScaleY ("Scale Y", Float) = 1
        _PerspectiveFilter ("Perspective Filter", Range(0,1)) = 0.875
        _ShaderFlags ("Shader Flags", Float) = 0
        _VertexOffsetX ("Vertex Offset X", Float) = 0
        _VertexOffsetY ("Vertex Offset Y", Float) = 0

        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)
        _MaskSoftnessX ("Mask Softness X", Float) = 0
        _MaskSoftnessY ("Mask Softness Y", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _CullMode ("Cull Mode", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull [_CullMode]
        ZWrite Off
        Lighting Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature __ OUTLINE_ON
            #pragma shader_feature __ UNDERLAY_ON UNDERLAY_INNER
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _FaceColor;
            fixed4 _OutlineColor;
            fixed4 _UnderlayColor;
            float _FaceDilate;
            float _OutlineWidth;
            float _OutlineSoftness;
            float _UnderlayOffsetX;
            float _UnderlayOffsetY;
            float _UnderlayDilate;
            float _UnderlaySoftness;
            float _GradientScale;
            float _TextureWidth;
            float _TextureHeight;
            float _WeightNormal;
            float _WeightBold;
            float _ScaleRatioA;
            float _ScaleRatioC;
            float _Sharpness;
            float _VertexOffsetX;
            float _VertexOffsetY;
            float4 _ClipRect;
            float _MaskSoftnessX;
            float _MaskSoftnessY;

            struct AppData
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float4 texcoord0 : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                fixed4 faceColor : COLOR;
                fixed4 outlineColor : COLOR1;
                float2 uv : TEXCOORD0;
                half4 param : TEXCOORD1;
                half4 mask : TEXCOORD2;
                #if (UNDERLAY_ON || UNDERLAY_INNER)
                float4 underlayUv : TEXCOORD3;
                fixed4 underlayColor : COLOR2;
                #endif
            };

            Varyings Vert(AppData input)
            {
                Varyings output;
                UNITY_INITIALIZE_OUTPUT(Varyings, output);

                // Unity 6 TMP stores the signed bold/scale value in UV0.w.
                // Older TMP shaders read UV1.y, whose bottom vertices are zero,
                // producing a visible horizontal strip below every glyph.
                float bold = step(input.texcoord0.w, 0);
                float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
                weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

                float4 vertex = input.vertex;
                vertex.xy += float2(_VertexOffsetX, _VertexOffsetY);
                output.vertex = UnityObjectToClipPos(vertex);
                output.uv = input.texcoord0.xy;

                float opacity = input.color.a;
                #if (UNDERLAY_ON || UNDERLAY_INNER)
                opacity = 1.0;
                #endif

                output.faceColor = fixed4(input.color.rgb, opacity) * _FaceColor;
                output.faceColor.rgb *= output.faceColor.a;

                output.outlineColor = _OutlineColor;
                output.outlineColor.a *= opacity;
                output.outlineColor.rgb *= output.outlineColor.a;

                output.param = half4(
                    0.5 - weight,
                    1.3333 * _GradientScale * (_Sharpness + 1) / _TextureWidth,
                    _OutlineWidth * _ScaleRatioA * 0.5,
                    0);

                float4 rect = clamp(_ClipRect, -2e10, 2e10);
                output.mask = half4(
                    vertex.xy * 2 - rect.xy - rect.zw,
                    0,
                    0);

                #if (UNDERLAY_ON || UNDERLAY_INNER)
                float2 offset = float2(
                    -_UnderlayOffsetX * _ScaleRatioC * _GradientScale / _TextureWidth,
                    -_UnderlayOffsetY * _ScaleRatioC * _GradientScale / _TextureHeight);
                output.underlayUv = float4(input.texcoord0.xy + offset, input.color.a, 0);
                output.underlayColor = _UnderlayColor;
                output.underlayColor.rgb *= output.underlayColor.a;
                #endif

                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float distance = tex2D(_MainTex, input.uv).a;
                float2 uvDx = ddx(input.uv);
                float2 uvDy = ddy(input.uv);
                float scale = rsqrt(abs(uvDx.x * uvDy.y - uvDy.x * uvDx.y)) * input.param.y;
                scale /= 1 + (_OutlineSoftness * _ScaleRatioA * scale);

                fixed4 color = input.faceColor *
                    saturate((distance - input.param.x) * scale + 0.5);

                #if OUTLINE_ON
                fixed4 outlineColor = lerp(
                    input.faceColor,
                    input.outlineColor,
                    sqrt(min(1.0, input.param.z * scale * 2)));
                color = lerp(
                    outlineColor,
                    input.faceColor,
                    saturate((distance - input.param.x - input.param.z) * scale + 0.5));
                color *= saturate(
                    (distance - input.param.x + input.param.z) * scale + 0.5);
                #endif

                #if (UNDERLAY_ON || UNDERLAY_INNER)
                float layerScale = scale;
                layerScale /= 1 + (_UnderlaySoftness * _ScaleRatioC * layerScale);
                float layerBias =
                    input.param.x * layerScale - 0.5 -
                    (_UnderlayDilate * _ScaleRatioC * 0.5 * layerScale);
                float underlayDistance = tex2D(_MainTex, input.underlayUv.xy).a;

                #if UNDERLAY_ON
                color += input.underlayColor *
                    saturate(underlayDistance * layerScale - layerBias) *
                    (1 - color.a);
                #endif

                #if UNDERLAY_INNER
                float faceBias = input.param.x * scale - 0.5;
                float faceDistance = saturate(
                    distance * scale - faceBias - input.param.z);
                color += input.underlayColor *
                    (1 - saturate(underlayDistance * layerScale - layerBias)) *
                    faceDistance *
                    (1 - color.a);
                #endif

                color *= input.underlayUv.z;
                #endif

                #if UNITY_UI_CLIP_RECT
                float2 maskSoftness =
                    0.25 / (0.25 * float2(_MaskSoftnessX, _MaskSoftnessY) + (1 / scale));
                float2 clipMask = saturate(
                    (_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * maskSoftness);
                color *= clipMask.x * clipMask.y;
                #endif

                #if UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }

    CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
}
