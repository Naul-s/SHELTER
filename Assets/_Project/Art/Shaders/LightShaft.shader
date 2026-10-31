// 가짜 볼류메트릭 광선(달빛 기둥·전등 빛 원뿔). LightShaft 컴포넌트가 만드는 원뿔/사각뿔대 메시에 쓴다.
// 실시간 광원·깊이 텍스처 없이(Quest 성능) 빛 속 먼지 낀 공기를 표현한다:
// - 면을 비스듬히 볼수록 흐려지는 가장자리(프레넬) → 부피감
// - 광원에서 멀어질수록 감쇠, 시작부는 부드럽게
// - 월드 공간 3D 값 노이즈가 천천히 흘러가는 먼지(양안에 같은 위치라 VR 에서도 자연스럽다)
// - 카메라가 광선 안/가까이 들어오면 사라짐(눈앞이 하얗게 덮이지 않게), 포그로 멀리선 어둠에 묻힘
// 가산 혼합. 밝기(_Intensity)는 LightFlicker 가 MaterialPropertyBlock 으로 흔든다.
Shader "Shelter/LightShaft"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.55, 0.68, 1.0, 1)
        _Intensity ("Intensity", Range(0, 4)) = 0.2
        _LengthFalloff ("Length Falloff", Range(0.2, 6)) = 1.6
        _StartFade ("Start Fade", Range(0.001, 0.5)) = 0.08
        _EdgePower ("Edge Softness", Range(0.2, 8)) = 2
        _NoiseScale ("Dust Scale", Range(0.1, 10)) = 2.2
        _NoiseSpeed ("Dust Drift (xyz)", Vector) = (0.05, -0.08, 0.03, 0)
        _NoiseStrength ("Dust Strength", Range(0, 1)) = 0.55
        _CameraFadeNear ("Camera Fade Near", Range(0, 3)) = 0.3
        _CameraFadeRange ("Camera Fade Range", Range(0.01, 3)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+50" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "LightShaft"
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _LengthFalloff;
                half _StartFade;
                half _EdgePower;
                half _NoiseScale;
                float4 _NoiseSpeed;
                half _NoiseStrength;
                half _CameraFadeNear;
                half _CameraFadeRange;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float along : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            // 삼선형 보간 값 노이즈(텍스처 없음)
            float ValueNoise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(Hash(i + float3(0, 0, 0)), Hash(i + float3(1, 0, 0)), f.x),
                                 lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x),
                                 lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.along = input.uv.y;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 toCamera = _WorldSpaceCameraPos - input.positionWS;
                float cameraDistance = length(toCamera);
                float3 viewDir = toCamera / max(cameraDistance, 1e-4);

                // 면을 정면으로 볼수록 진하고, 옆으로 비껴 볼수록(실루엣) 사라진다 → 둥근 부피처럼 보인다
                half facing = abs(dot(normalize(input.normalWS), viewDir));
                half edge = pow(facing, _EdgePower);

                half along = saturate(input.along);
                half lengthFade = pow(1.0 - along, _LengthFalloff) * smoothstep(0.0, _StartFade, along);

                float3 dustPos = input.positionWS * _NoiseScale + _Time.y * _NoiseSpeed.xyz;
                half dust = ValueNoise(dustPos) * 0.65 + ValueNoise(dustPos * 2.7 + 13.1) * 0.35;
                dust = lerp(1.0, dust * 1.6, _NoiseStrength);

                half cameraFade = saturate((cameraDistance - _CameraFadeNear) / _CameraFadeRange);

                half amount = edge * lengthFade * dust * cameraFade * _Intensity;
                half3 color = _Color.rgb * amount;
                color = MixFogColor(color, half3(0, 0, 0), input.fogFactor);
                return half4(color, 0);
            }
            ENDHLSL
        }
    }
}
