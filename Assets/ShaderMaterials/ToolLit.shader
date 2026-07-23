Shader "Custom/ToolLit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        // アウトライン用、追加.
        [HDR] _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.01

        // 追加5: 影色. 真っ黒ではなく青紫系に転がすとアニメ塗りらしくなる.
        _ShadowTint("Shadow Tint", Color) = (0.35, 0.3, 0.5, 1)

        // 追加5: トゥーンの境界位置と滑らかさも外部化(元は0.45〜0.55固定だった).
        _ToonThreshold("Toon Threshold", Range(0, 1)) = 0.5
        _ToonSmoothness("Toon Smoothness", Range(0.001, 0.5)) = 0.05

        // 追加6: リムライトの外部設定化. HDRにするとBloom併用で発光する.
        [HDR] _RimColor("Rim Color", Color) = (0.4, 0.6, 1.0, 1)
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.6
        _RimSmoothness("Rim Smoothness", Range(0.001, 0.5)) = 0.3

        // 追加7: トゥーンスペキュラ. Powerが大きいほどハイライトが小さく締まる.
        [HDR] _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularPower("Specular Power", Range(8, 128)) = 48

        // 追加9: ランプテクスチャ. ONにするとsmoothstep段階化の代わりに使う.
        // 横長グラデ画像(例: 256x1)を用意し, Wrap ModeをClampにしておくこと.
        [Toggle(_USE_RAMP)] _UseRamp("Use Ramp Texture", Float) = 0
        [NoScaleOffset] _RampMap("Ramp Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            // これが無いと暗黙SRPDefaultUnlit扱いになり, 2パス目と衝突してアウトラインが消える.
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // 追加8: シャドウマップを受けるためのキーワード.
            // URP Asset側の設定(カスケード有無・ソフトシャドウ)に応じて適切な亜種が選ばれる.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // 追加9: ランプ使用のON/OFFで別バリアントをコンパイルする.
            #pragma shader_feature_local_fragment _USE_RAMP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" // Core から Lighting に変更(改行しない).

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS: NORMAL; // 追加.
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD2; // 追加4.
                float3 normalWS : TEXCOORD1; // 追加.
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // 追加9: ランプテクスチャ. テクスチャはCBUFFERの外に宣言する.
            TEXTURE2D(_RampMap);
            SAMPLER(sampler_RampMap);

            // SRP Batcher対応のため, 2パス目とレイアウトを完全一致させること.
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _OutlineColor;
                float _OutlineWidth;
                half4 _ShadowTint;      // 追加5.
                float _ToonThreshold;   // 追加5.
                float _ToonSmoothness;  // 追加5.
                half4 _RimColor;        // 追加6.
                float _RimThreshold;    // 追加6.
                float _RimSmoothness;   // 追加6.
                half4 _SpecularColor;   // 追加7.
                float _SpecularPower;   // 追加7.
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS  = TransformObjectToWorldNormal(IN.normalOS); // 追加.
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz); // 追加4.
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 補間で法線の長さが1からズレるので, 最初に一回だけ正規化して使い回す.
                half3 N = normalize(IN.normalWS);

// 追加8.
                // ワールド座標からシャドウマップ参照用の座標を作り, ライトに渡す.
                // これでlight.shadowAttenuation(1=日向, 0=他オブジェクトの影の中)が取れる.
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light light = GetMainLight(shadowCoord); // 主ライトを影の減衰値付きで取得する.
// ここまで

                // Lambert: 法線とライト方向の内積 = 明るさ. 影の減衰も掛け込む.
                half ndl = saturate(dot(N, light.direction)) * light.shadowAttenuation;

// 追加2, 追加5, 追加9: 段階化(トゥーンの本体).
            #if defined(_USE_RAMP)
                // 追加9: ランプテクスチャ方式.
                // 明るさndl(0〜1)をそのまま横方向のUVにしてグラデ画像を参照する.
                // 影の色・段数・境界のボケ具合を全部テクスチャ側で自由に描ける最終形態.
                half3 lit = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, half2(ndl, 0.5)).rgb * light.color;
            #else
                // smoothstepで2段階に切り, 影側を_ShadowTint, 明側をライト色にする.
                half toon = smoothstep(_ToonThreshold - _ToonSmoothness,
                                       _ToonThreshold + _ToonSmoothness, ndl);
                half3 lit = lerp(_ShadowTint.rgb, light.color, toon); // 追加5: 影を黒以外にする.
            #endif
// ここまで

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

// 追加4, 追加6: リムライト(外部設定化).
                // 視線と法線が直交する = 輪郭に近いほどrimDotが1に近づく.
                half3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                half rimDot = 1.0 - saturate(dot(viewDir, N));
                half rim = smoothstep(_RimThreshold, _RimThreshold + _RimSmoothness, rimDot);
// ここまで

// 追加7: トゥーンスペキュラ(髪のアレ).
                // Blinn-Phong: ライト方向と視線方向の中間ベクトルと法線が揃うほど強く光る.
                half3 halfVec = normalize(light.direction + viewDir);
                half spec = pow(saturate(dot(N, halfVec)), _SpecularPower);
                // stepだと縁がジャギるので, ごく狭いsmoothstepでパキッと2値化する.
                spec = smoothstep(0.45, 0.55, spec);
// ここまで

                // 合成: ベース色*陰影 + ハイライト + リム.
                // スペキュラには影の減衰を掛けて, 影の中でハイライトが浮かないようにする.
                // リムは輪郭光なので影側でも見えるよう, 段階化の後に加算する.
                half3 rgb = color.rgb * lit
                          + spec * _SpecularColor.rgb * light.color * light.shadowAttenuation
                          + rim * _RimColor.rgb;

                return half4(rgb, color.a);
            }
            ENDHLSL
        }

//  アウトライン用2パス目.
        Pass
        {
            Name "Outline"
            Tags{ "LightMode" = "SRPDefaultUnlit" }
            Cull Front // 背面だけ描画する.

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // Coreに変更(改行しない).

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS: NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // 1パス目とレイアウトを完全一致させる(SRP Batcher対応).
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _OutlineColor;
                float _OutlineWidth;
                half4 _ShadowTint;
                float _ToonThreshold;
                float _ToonSmoothness;
                half4 _RimColor;
                float _RimThreshold;
                float _RimSmoothness;
                half4 _SpecularColor;
                float _SpecularPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 pos = IN.positionOS.xyz + normalize(IN.normalOS) * _OutlineWidth;
                OUT.positionHCS = TransformObjectToHClip(pos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
