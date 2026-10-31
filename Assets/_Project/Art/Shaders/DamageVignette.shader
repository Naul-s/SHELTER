// 피격·저체력·사망 시야 비네트. 카메라 앞 원판 메시(PlayerDamageFeedback 이 생성)에 쓰인다.
// uv0.x = 시야 중심에서의 각도 비율(1 = 시야 가장자리), uv1 = 그 정점의 방향(카메라 기준 xy, 중심은 0).
// 카메라를 움직이지 않고 색으로만 피드백한다(vr-performance-comfort). 화면 전체를 덮으므로 쓰지 않을 땐 렌더러를 끈다.
Shader "Shelter/DamageVignette"
{
    Properties
    {
        [HideInInspector] _FlashColor ("Flash Color", Color) = (0.55, 0, 0, 1)
        [HideInInspector] _LowColor ("Low Health Color", Color) = (0.35, 0, 0, 1)
        [HideInInspector] _FadeColor ("Fade Color", Color) = (0.06, 0, 0, 1)
        [HideInInspector] _HitDir ("Hit Direction (xy, z = directional amount)", Vector) = (0, 0, 0, 0)
        [HideInInspector] _Flash ("Flash", Range(0, 1)) = 0
        [HideInInspector] _FlashCoverage ("Flash Coverage", Range(0.01, 1)) = 0.5
        [HideInInspector] _Low ("Low Health", Range(0, 1)) = 0
        [HideInInspector] _LowCoverage ("Low Coverage", Range(0.01, 1)) = 0.45
        [HideInInspector] _Fade ("Fade", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay+100" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "DamageVignette"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FlashColor;
                half4 _LowColor;
                half4 _FadeColor;
                float4 _HitDir;
                half _Flash;
                half _FlashCoverage;
                half _Low;
                half _LowCoverage;
                half _Fade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 dir : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float edge : TEXCOORD0;
                float2 dir : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.edge = input.uv.x;
                output.dir = input.dir;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 맞은 방향 쪽 가장자리를 더 진하게(정면·정후방에서 맞으면 고르게)
                float len = length(input.dir);
                float toward = len > 1e-3 ? dot(input.dir / len, _HitDir.xy) * 0.5 + 0.5 : 0.5;
                float dirMask = lerp(1.0, saturate(toward * 1.6 - 0.1), _HitDir.z);

                half flash = smoothstep(1.0 - _FlashCoverage, 1.0, input.edge) * _Flash * dirMask;
                half low = smoothstep(1.0 - _LowCoverage, 1.0, input.edge) * _Low;

                half alpha = saturate(flash + low * (1.0 - flash));
                half3 color = (_FlashColor.rgb * flash + _LowColor.rgb * low * (1.0 - flash)) / max(alpha, 1e-4);

                color = lerp(color, _FadeColor.rgb, _Fade);
                alpha = lerp(alpha, 1.0, _Fade);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
