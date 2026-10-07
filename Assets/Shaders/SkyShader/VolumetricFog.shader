Shader "Hidden/Llanos/VolumetricFog"
{
    Properties
    {
        _VolFogMaxDistance ("Max Distance", Float) = 60
        _VolFogSteps ("Steps", Float) = 24
        _VolFogBaseHeight ("Base Height", Float) = 0
        _VolFogHeightFalloff ("Height Falloff", Float) = 0.15
        _VolFogNoiseScale ("Noise Scale", Float) = 0.08
        _VolFogNoiseAmount ("Noise Amount", Float) = 0.6
        _VolFogWind ("Wind Speed", Float) = 0.3
        _VolFogAnisotropy ("Anisotropy", Float) = 0.45
        _VolFogAmbient ("Ambient Intensity", Float) = 1
        _VolFogMoonScatter ("Moon Scatter", Float) = 0.6
        _VolFogLanternScatter ("Lantern Scatter", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "VolumetricFog"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // Ajustes (los escribe VolumetricFogFeature en el material)
            float _VolFogMaxDistance;
            float _VolFogSteps;
            float _VolFogBaseHeight;
            float _VolFogHeightFalloff;
            float _VolFogNoiseScale;
            float _VolFogNoiseAmount;
            float _VolFogWind;
            float _VolFogAnisotropy;
            float _VolFogAmbient;
            float _VolFogMoonScatter;
            float _VolFogLanternScatter;

            // Luces de niebla (linternas): xyz = posición, w = rango / rgb = color * intensidad
            float4 _VolFogLightPosRange[4];
            float4 _VolFogLightColor[4];
            float _VolFogLightCount;

            // Globales: los escribe FogDriver
            float _VolFogDensity;
            float4 _SkyFogColor;

            // ---------- Ruido ----------
            float vfHash(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float vfNoise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = vfHash(i);
                float n100 = vfHash(i + float3(1, 0, 0));
                float n010 = vfHash(i + float3(0, 1, 0));
                float n110 = vfHash(i + float3(1, 1, 0));
                float n001 = vfHash(i + float3(0, 0, 1));
                float n101 = vfHash(i + float3(1, 0, 1));
                float n011 = vfHash(i + float3(0, 1, 1));
                float n111 = vfHash(i + float3(1, 1, 1));
                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            // Fase Henyey-Greenstein (normalizada para que g = 0 valga 1)
            float henyeyGreenstein(float cosTheta, float g)
            {
                float g2 = g * g;
                return (1.0 - g2) / pow(max(1.0 + g2 - 2.0 * g * cosTheta, 1e-3), 1.5);
            }

            // ---------- Densidad de la niebla en un punto del mundo ----------
            float fogDensityAt(float3 p)
            {
                float h = max(p.y - _VolFogBaseHeight, 0.0);
                float heightFactor = exp(-h * _VolFogHeightFalloff);

                float3 np = p * _VolFogNoiseScale
                            + float3(_Time.y * _VolFogWind, 0.0, _Time.y * _VolFogWind * 0.6);
                float n = vfNoise(np);
                float patch = lerp(1.0, saturate((n - 0.25) * 2.2), _VolFogNoiseAmount);

                return _VolFogDensity * heightFactor * patch;
            }

            // ---------- Luz que recibe la niebla en un punto ----------
            float3 fogLightAt(float3 p, float3 rayDir)
            {
                // Ambiente: el mismo color que usa el cielo, así todo queda coherente
                float3 L = _SkyFogColor.rgb * _VolFogAmbient;

                // Luna (luz direccional principal). Sin sombras por ahora.
                float3 toMoon = normalize(_MainLightPosition.xyz);
                float moonPhase = henyeyGreenstein(dot(rayDir, toMoon), _VolFogAnisotropy);
                L += _MainLightColor.rgb * moonPhase * _VolFogMoonScatter;

                // Linternas
                int count = (int)_VolFogLightCount;
                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    if (i < count)
                    {
                        float3 toLight = _VolFogLightPosRange[i].xyz - p;
                        float d = length(toLight);
                        float att = saturate(1.0 - d / max(_VolFogLightPosRange[i].w, 0.001));
                        att *= att;
                        float ph = henyeyGreenstein(dot(rayDir, toLight / max(d, 1e-3)), _VolFogAnisotropy * 0.4);
                        L += _VolFogLightColor[i].rgb * att * ph * _VolFogLanternScatter;
                    }
                }
                return L;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                if (_VolFogDensity <= 0.00001) return sceneColor;

                // Posición en el mundo del píxel (el cielo queda en el plano lejano)
                float rawDepth = SampleSceneDepth(uv);
            #if !UNITY_REVERSED_Z
                rawDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, rawDepth);
            #endif
                float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);

                float3 camPos = _WorldSpaceCameraPos;
                float3 toPos = worldPos - camPos;
                float fullDist = length(toPos);
                float3 rayDir = toPos / max(fullDist, 1e-4);

                float marchDist = min(fullDist, _VolFogMaxDistance);
                int steps = max((int)_VolFogSteps, 4);
                float stepLen = marchDist / steps;

                // Jitter por píxel (ruido de gradiente entrelazado) para ocultar el banding
                float2 pix = input.positionCS.xy + 5.588238 * fmod(floor(_Time.y * 60.0), 64.0);
                float jitter = frac(52.9829189 * frac(dot(pix, float2(0.06711056, 0.00583715))));

                float3 scatter = 0;
                float trans = 1.0;
                float3 lastLight = 0;
                float lastDensity = 0;

                [loop]
                for (int i = 0; i < steps; i++)
                {
                    float t = (i + jitter) * stepLen;
                    float3 p = camPos + rayDir * t;

                    float dens = fogDensityAt(p);
                    float3 L = fogLightAt(p, rayDir);

                    float stepTrans = exp(-dens * stepLen);
                    scatter += trans * (1.0 - stepTrans) * L;
                    trans *= stepTrans;

                    lastLight = L;
                    lastDensity = dens;
                }

                // Cola: lo que queda más allá de la distancia máxima (horizonte y cielo)
                float extra = min(max(fullDist - marchDist, 0.0), _VolFogMaxDistance * 3.0);
                if (extra > 0.0)
                {
                    float tailTrans = exp(-lastDensity * extra * 0.5);
                    scatter += trans * (1.0 - tailTrans) * lastLight;
                    trans *= tailTrans;
                }

                return half4(sceneColor.rgb * trans + scatter, sceneColor.a);
            }
            ENDHLSL
        }
    }
}
