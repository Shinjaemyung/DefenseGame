Shader "TowerDefense/BurningEffect"
{
    // 임의의 오브젝트(예: 사람 모델)를 감싸는 별도 메쉬(파이어 셸)에 씌워서
    // "불타는 오브젝트" 효과를 내기 위한 URP 언릿 셰이더.
    //
    // 사용 방법:
    // 1) 대상 오브젝트를 살짝 감쌀 수 있는 별도의 메쉬(대상 실루엣과 비슷한 형태거나,
    //    단순한 구/캡슐/화염 모양 메쉬)를 자식으로 붙인다.
    // 2) 이 셰이더로 머티리얼을 만들고 _NoiseTex에 노이즈 텍스처를 지정한다.
    //    (예: Assets/ImportAssets/HSFiles/Textures/Noise41.png, Noise35.png 등)
    // 3) UV의 V=0이 메쉬 아래쪽, V=1이 위쪽이 되도록 되어 있으면
    //    _BottomBias로 "위로 갈수록 옅어지는" 불꽃 형태를 낼 수 있다 (0으로 끄면 비활성).
    Properties
    {
        [Header(Noise)]
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _Tiling1("Noise Tiling Layer 1", Vector) = (2, 2, 0, 0)
        _Tiling2("Noise Tiling Layer 2", Vector) = (3, 3, 0, 0)
        _Scroll1("Scroll Speed Layer 1 XY", Vector) = (0.15, 0.4, 0, 0)
        _Scroll2("Scroll Speed Layer 2 XY", Vector) = (-0.1, 0.6, 0, 0)

        [Header(Fire Color Ramp)]
        _ColorCold("Color Base Cold", Color) = (0.35, 0.02, 0.0, 1)
        _ColorMid("Color Mid Flame", Color) = (1.0, 0.35, 0.02, 1)
        _ColorHot("Color Hot Tip", Color) = (1.0, 0.85, 0.3, 1)
        _GradientPower("Gradient Sharpness", Range(0.1, 5)) = 1.5

        [Header(Shape Alpha)]
        _AlphaThreshold("Alpha Cutoff", Range(0, 1)) = 0.35
        _AlphaSoftness("Alpha Edge Softness", Range(0.001, 0.5)) = 0.15
        _BottomBias("Bottom Density Bias 0 is off", Range(0, 2)) = 0.6

        [Header(Rim And Brightness)]
        _FresnelPower("Fresnel Power", Range(0.1, 8)) = 2.5
        _FresnelIntensity("Fresnel Intensity", Range(0, 5)) = 1.2
        _Intensity("Overall Brightness", Range(0, 5)) = 1.5

        [Header(Motion)]
        _DistortionStrength("Vertex Flicker Strength", Range(0, 0.2)) = 0.03
        _DistortionSpeed("Vertex Flicker Speed", Float) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            // 가산 블렌딩: 화염이 겹칠수록 밝아지고, 배경/피부와 자연스럽게 섞인다
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float2 _Tiling1;
                float2 _Tiling2;
                float2 _Scroll1;
                float2 _Scroll2;
                float4 _ColorCold;
                float4 _ColorMid;
                float4 _ColorHot;
                float _GradientPower;
                float _AlphaThreshold;
                float _AlphaSoftness;
                float _BottomBias;
                float _FresnelPower;
                float _FresnelIntensity;
                float _Intensity;
                float _DistortionStrength;
                float _DistortionSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // 오브젝트마다 흔들림 위상이 겹치지 않도록 월드 위치 기반 오프셋 사용
                float phase = dot(positionWS, float3(12.9898, 78.233, 37.719));
                float flicker = sin(_Time.y * _DistortionSpeed + phase) * 0.5
                               + sin(_Time.y * _DistortionSpeed * 1.7 + phase * 1.3) * 0.5;

                positionWS += normalWS * flicker * _DistortionStrength;

                OUT.positionWS = positionWS;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                OUT.normalWS = normalWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uvBase = IN.uv;

                float2 uv1 = uvBase * _Tiling1 + _Time.y * _Scroll1;
                float2 uv2 = uvBase * _Tiling2 + _Time.y * _Scroll2;

                float n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv1).r;
                float n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv2).r;

                // 두 겹의 노이즈를 곱해서 불규칙한 난류(turbulence)를 만든다
                float fireMask = saturate(n1 * n2 * 2.0);

                // 아래쪽이 진하고 위로 갈수록 옅어지는 불꽃 형태 (uv.y가 위로 갈수록 커진다고 가정)
                fireMask = saturate(fireMask - uvBase.y * _BottomBias);

                // 컬러 램프: cold -> mid -> hot
                half3 colorLow = lerp(_ColorCold.rgb, _ColorMid.rgb, pow(saturate(fireMask * 2.0), _GradientPower));
                half3 fireColor = lerp(colorLow, _ColorHot.rgb, pow(saturate(fireMask * 2.0 - 1.0), _GradientPower));

                // 프레넬 림 글로우 (가장자리를 더 밝게)
                float3 viewDirWS = normalize(IN.viewDirWS);
                float3 normalWS = normalize(IN.normalWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower) * _FresnelIntensity;

                half3 finalColor = (fireColor + fresnel * _ColorHot.rgb) * _Intensity;

                // 알파: 노이즈 기반 컷오프로 들쭉날쭉한 불꽃 가장자리를 표현 (디졸브 느낌)
                float alpha = smoothstep(_AlphaThreshold - _AlphaSoftness, _AlphaThreshold + _AlphaSoftness, fireMask);
                alpha = saturate(alpha + fresnel * 0.3);

                return half4(finalColor * alpha, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
