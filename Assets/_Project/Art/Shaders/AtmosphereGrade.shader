// 쉘터 분위기 컬러 그레이딩 — URP Full Screen Pass Renderer Feature 의 머티리얼로 쓴다(VR 싱글패스/멀티뷰 대응).
// 한 번의 풀스크린 패스에서: 노출 → 채도 → 스플릿 토닝(차가운 그림자 / 누런 하이라이트) → 대비·토(검정 누르기)
// → 어두운 곳일수록 강해지는 비네트 → 8비트 밴딩을 없애는 고정 디더.
// VR 컴포트: 시간에 따라 움직이는 효과(필름 그레인·색수차·흔들림)는 넣지 않는다 — 양안에 다르게 보이면 눈이 피로하다.
// 성능: Quest 에서 풀스크린 1패스(텍스처 1회 샘플 + ALU). 끄려면 렌더러의 Atmosphere 피처를 비활성화.
Shader "Shelter/AtmosphereGrade"
{
    Properties
    {
        [Header(Exposure and Tone)]
        _Exposure ("Exposure", Range(0.1, 2)) = 0.75
        _Contrast ("Contrast", Range(0.5, 2)) = 1.18
        _Pivot ("Contrast Pivot", Range(0.1, 0.9)) = 0.42
        _BlackCrush ("Black Crush", Range(0, 0.25)) = 0.035
        _HighlightRolloff ("Highlight Rolloff", Range(0, 1)) = 0.35

        [Header(Color)]
        _Saturation ("Saturation", Range(0, 1.5)) = 0.68
        _ShadowTint ("Shadow Tint", Color) = (0.62, 0.78, 0.92, 1)
        _HighlightTint ("Highlight Tint", Color) = (1.0, 0.9, 0.7, 1)
        _SplitBalance ("Split Balance", Range(-1, 1)) = -0.15
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.7

        [Header(Vignette)]
        _VignetteColor ("Vignette Color", Color) = (0.02, 0.025, 0.03, 1)
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.55
        _VignetteRadius ("Vignette Radius", Range(0, 1)) = 0.35
        _VignetteSoftness ("Vignette Softness", Range(0.05, 1)) = 0.55
        _VignetteDarkBoost ("Vignette Dark Boost", Range(0, 1)) = 0.5

        [Header(Banding)]
        _Dither ("Dither (8-bit steps)", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "AtmosphereGrade"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Exposure;
                half _Contrast;
                half _Pivot;
                half _BlackCrush;
                half _HighlightRolloff;
                half _Saturation;
                half4 _ShadowTint;
                half4 _HighlightTint;
                half _SplitBalance;
                half _TintStrength;
                half4 _VignetteColor;
                half _VignetteIntensity;
                half _VignetteRadius;
                half _VignetteSoftness;
                half _VignetteDarkBoost;
                half _Dither;
            CBUFFER_END

            static const half3 LumaWeights = half3(0.2126, 0.7152, 0.0722);

            // 화면 좌표 기반 고정 노이즈(Jimenez interleaved gradient noise) — 시간에 따라 변하지 않는다.
            float InterleavedGradientNoise(float2 pixel)
            {
                return frac(52.9829189 * frac(dot(pixel, float2(0.06711056, 0.00583715))));
            }

            // 색조는 바꾸되 밝기는 유지하도록 정규화한 틴트
            half3 NormalizeTint(half3 tint)
            {
                return tint / max(dot(tint, LumaWeights), 1e-3);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half3 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0).rgb;

                // 1) 노출 + 밝은 부분 부드럽게 눌러주기(LDR 버퍼에서도 형광등이 하얗게 타지 않게)
                color *= _Exposure;
                color = lerp(color, color / (1.0 + color), _HighlightRolloff);

                // 지각 공간(≈감마 2)에서 그레이딩 — 선형에서 하면 어두운 영역이 뭉개진다
                half3 p = sqrt(max(color, 0.0));
                half luma = dot(p, LumaWeights);

                // 2) 채도
                p = max(lerp(luma.xxx, p, _Saturation), 0.0);

                // 3) 스플릿 토닝: 어두운 곳은 차갑게, 밝은 곳은 누렇게(오래된 형광등·나트륨등)
                half split = smoothstep(0.0, 1.0, saturate(luma + _SplitBalance * 0.5));
                half3 tint = lerp(NormalizeTint(_ShadowTint.rgb), NormalizeTint(_HighlightTint.rgb), split);
                p *= lerp(1.0.xxx, tint, _TintStrength);

                // 4) 대비 + 검정 누르기(토)
                p = (p - _Pivot) * _Contrast + _Pivot;
                p = max(p - _BlackCrush, 0.0) / (1.0 - _BlackCrush);

                // 5) 비네트 — 어두운 장면일수록 가장자리가 더 깊게 가라앉는다
                float2 centered = uv - 0.5;
                half radial = length(centered * float2(1.0, 0.9)) * 1.41421356;
                half vignette = smoothstep(_VignetteRadius, _VignetteRadius + _VignetteSoftness, radial);
                half darkness = 1.0 - saturate(luma * 2.0);
                vignette *= _VignetteIntensity * (1.0 + _VignetteDarkBoost * darkness);
                p = lerp(p, sqrt(_VignetteColor.rgb), saturate(vignette));

                // 6) 디더(출력 8비트 한 단계 크기) — 어두운 그라데이션의 계단 현상 제거
                half noise = InterleavedGradientNoise(input.positionCS.xy) - 0.5;
                p += noise * (_Dither / 255.0);

                color = p * p;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
