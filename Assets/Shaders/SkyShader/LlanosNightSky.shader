Shader "Skybox/LlanosNightSky"
{
    Properties
    {
        [Header(Gradient)]
        _ZenithColor ("Zenith Color", Color) = (0.002, 0.003, 0.007, 1)
        _HorizonColor ("Horizon Color", Color) = (0.012, 0.014, 0.017, 1)
        _GroundColor ("Ground Color", Color) = (0.001, 0.001, 0.002, 1)
        _HorizonPower ("Horizon Power", Range(0.1, 2)) = 0.6

        [Header(Stars)]
        _StarIntensity ("Star Intensity", Range(0, 6)) = 1.8
        _StarDensity ("Star Density", Range(0, 2)) = 1
        _TwinkleSpeed ("Twinkle Speed", Range(0, 12)) = 5
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.12

        [Header(Milky Way)]
        _MilkyWayColor ("Milky Way Color", Color) = (0.75, 0.72, 0.65, 1)
        _MilkyWayIntensity ("Milky Way Intensity", Range(0, 1)) = 0.07
        _MilkyWayWidth ("Milky Way Width (higher = thinner)", Range(1, 30)) = 7
        _MilkyWayAxis ("Milky Way Axis (normal of the band)", Vector) = (0.3, 0.8, 0.5, 0)

        [Header(Moon)]
        _MoonPhase ("Moon Phase (0 new, 0.25 first qtr, 0.5 full, 0.75 last qtr)", Range(0, 1)) = 0.2
        _MoonSize ("Moon Radius", Range(0.01, 0.1)) = 0.035
        _MoonIntensity ("Moon Intensity", Range(0, 20)) = 3
        _MoonSkyGlow ("Moon Sky Glow", Range(0, 3)) = 1
        _MoonWash ("Moon Star Wash-out", Range(0, 1)) = 0.85

        [Header(Clouds)]
        _CloudScale ("Cloud Scale (higher = smaller clouds)", Range(0.5, 8)) = 2.5
        _CloudSpeed ("Cloud Speed", Range(0, 0.1)) = 0.012
        _CloudDensity ("Cloud Density (opacity)", Range(0.5, 8)) = 3
        _CloudDetail ("Cloud Detail", Range(0, 1)) = 0.35
        _CloudColor ("Cloud Base Color", Color) = (0.006, 0.007, 0.010, 1)
        _CloudMoonLight ("Cloud Moon Light", Range(0, 4)) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ZenithColor;
                float4 _HorizonColor;
                float4 _GroundColor;
                float _HorizonPower;
                float _StarIntensity;
                float _StarDensity;
                float _TwinkleSpeed;
                float _TwinkleAmount;
                float4 _MilkyWayColor;
                float _MilkyWayIntensity;
                float _MilkyWayWidth;
                float4 _MilkyWayAxis;
                float _MoonPhase;
                float _MoonSize;
                float _MoonIntensity;
                float _MoonSkyGlow;
                float _MoonWash;
                float _CloudScale;
                float _CloudSpeed;
                float _CloudDensity;
                float _CloudDetail;
                float4 _CloudColor;
                float _CloudMoonLight;
            CBUFFER_END

            // Global: lo escribe el script MoonSync desde la Directional Light de la luna
            float4 _MoonDirection;
            // Global: 0 = despejado, 1 = cubierto. Lo escribe el script cloudCoverageDriver
            float _CloudCoverage;
            float _SkyFogAmount;
            float4 _SkyFogColor;

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dir = v.positionOS.xyz;
                return o;
            }

            // ---------- Hash y ruido ----------
            float hash13(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float3 hash33(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return frac((p.xxy + p.yxx) * p.zyx);
            }

            float vnoise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = hash13(i);
                float n100 = hash13(i + float3(1, 0, 0));
                float n010 = hash13(i + float3(0, 1, 0));
                float n110 = hash13(i + float3(1, 1, 0));
                float n001 = hash13(i + float3(0, 0, 1));
                float n101 = hash13(i + float3(1, 0, 1));
                float n011 = hash13(i + float3(0, 1, 1));
                float n111 = hash13(i + float3(1, 1, 1));
                return lerp(lerp(lerp(n000, n100, f.x), lerp(n010, n110, f.x), f.y),
                            lerp(lerp(n001, n101, f.x), lerp(n011, n111, f.x), f.y), f.z);
            }

            float fbm(float3 p)
            {
                float a = 0.5, s = 0;
                for (int i = 0; i < 4; i++)
                {
                    s += a * vnoise(p);
                    p = p * 2.03 + 11.7;
                    a *= 0.5;
                }
                return s;
            }

            // fbm de 3 octavas normalizado a ~0..1 (más barato para las nubes)
            float cloudFbm(float3 p)
            {
                float amp = 0.5, sum = 0;
                for (int i = 0; i < 3; i++)
                {
                    sum += amp * vnoise(p);
                    p = p * 2.07 + 5.3;
                    amp *= 0.5;
                }
                return sum / 0.875;
            }

            // ---------- Una capa de estrellas ----------
            // scale: celdas por unidad (más alto = más estrellas, más pequeñas)
            float3 StarLayer(float3 dir, float scale, float density, float size, float seed, float mwBoost)
            {
                float3 p = dir * scale;
                float3 id = floor(p) + seed;
                float3 f = frac(p) - 0.5;

                float h = hash13(id);
                float d = saturate(density * (1.0 + mwBoost * 2.5));
                if (h > d) return 0;

                float3 offset = (hash33(id) - 0.5) * 0.5;
                float dist = length(f - offset);

                // Evita que las estrellas desaparezcan o hagan aliasing al ser sub-pixel
                float px = length(fwidth(p));
                float s = max(size, px * 0.8);

                float star = smoothstep(s, 0.0, dist);
                // Compensa brillo si se ensanchó por el tamaño mínimo
                star *= saturate(size / s);

                float r = frac(h * 37.7);
                float brightness = lerp(0.04, 1.0, pow(r, 10.0));

                // Parpadeo individual
                float amt = _TwinkleAmount * (1.0 + 2.5 * saturate(1.0 - dir.y * 2.5));
                float tw = 1.0 - amt + amt * sin(_Time.y * _TwinkleSpeed + h * 100.0)
                           * (0.6 + 0.4 * sin(_Time.y * _TwinkleSpeed * 2.7 + h * 37.0));

                // Color: de cálido a blanco-azulado
                float3 col = lerp(float3(1.0, 0.92, 0.82), float3(0.86, 0.91, 1.0), frac(h * 91.3));

                return col * star * brightness * tw;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float y = dir.y;

                // --- Gradiente del cielo ---
                float t = pow(saturate(y), _HorizonPower);
                float3 sky = lerp(_HorizonColor.rgb, _ZenithColor.rgb, t);

                // --- Luna ---
                float3 m = float3(0.35, 0.8, 0.45);
                if (dot(_MoonDirection.xyz, _MoonDirection.xyz) > 0.0001) m = _MoonDirection.xyz;
                m = normalize(m);

                float cosA = dot(dir, m);
                float a = saturate(1.0 - cosA);               // distancia angular a la luna

                float phi = _MoonPhase * 6.2831853;
                float lit = 0.5 - 0.5 * cos(phi);             // fracción iluminada: 0 nueva, 1 llena

                float3 upRef = abs(m.y) > 0.99 ? float3(0, 0, 1) : float3(0, 1, 0);
                float3 mu = normalize(upRef - m * dot(upRef, m));
                float3 mr = cross(mu, m);
                float R = max(_MoonSize, 0.002);
                float2 muv = float2(dot(dir, mr), dot(dir, mu)) / R;
                float mlen = length(muv);
                float mfw = fwidth(mlen);

                float discMask = 0;
                float3 moonLayer = 0;
                if (cosA > 0 && mlen < 1.5)
                {
                    float3 n = float3(muv, sqrt(saturate(1.0 - dot(muv, muv))));
                    float3 L = float3(sin(phi), 0, -cos(phi));  // dirección de la luz solar en el marco de la luna
                    float shade = smoothstep(-0.02, 0.12, dot(n, L));
                    float maria = smoothstep(0.45, 0.62, fbm(n * 2.2 + 5.0));
                    float albedo = lerp(0.95, 0.5, maria * 0.85) * (0.9 + 0.2 * vnoise(n * 14.0));
                    float edge = saturate((1.0 - mlen) / max(mfw * 1.5, 1e-4));
                    discMask = edge;
                    float3 moonCol = float3(0.96, 0.93, 0.85);
                    moonLayer = moonCol * albedo * (shade + 0.02) * _MoonIntensity * edge; // 0.02 = luz cenicienta
                }

                float3 halo = float3(0.75, 0.82, 1.0)
                              * (exp(-a * 90.0) * 0.10 + exp(-a * 8.0) * 0.02)
                              * lit * _MoonIntensity * 0.25;
                float3 moonSky = float3(0.016, 0.024, 0.042) * lit * _MoonSkyGlow
                                 * (0.4 + 0.6 * exp(-a * 2.0)) * smoothstep(-0.05, 0.3, dir.y);
                float wash = saturate(lit * _MoonWash * (0.55 + 0.45 * exp(-a * 6.0)));

                // --- Vía Láctea ---
                float3 axis = normalize(_MilkyWayAxis.xyz);
                float band = dot(dir, axis);
                float core = exp(-band * band * _MilkyWayWidth);
                float n1 = fbm(dir * 4.0);
                float n2 = fbm(dir * 9.0 + 17.0);
                float mw = core * saturate(0.35 + n1 * 1.2);
                float dust = smoothstep(0.5, 0.75, n2);
                mw *= 1.0 - 0.9 * dust * core;      // vetas oscuras de polvo
                float mwFade = smoothstep(-0.02, 0.25, y);
                sky += _MilkyWayColor.rgb * mw * _MilkyWayIntensity * mwFade * (1.0 - saturate(lit * _MoonWash * 1.1));

                // --- Estrellas (3 capas) ---
                float3 stars = 0;
                stars += StarLayer(dir, 22.0, 0.08 * _StarDensity, 0.14, 1.0, mw);  // pocas, brillantes
                stars += StarLayer(dir, 55.0, 0.25 * _StarDensity, 0.12, 7.0, mw);  // medias
                stars += StarLayer(dir, 120.0, 0.35 * _StarDensity, 0.10, 19.0, mw); // polvo de estrellas

                // Extinción atmosférica cerca del horizonte
                float horizonFade = smoothstep(0.0, 0.2, y);
                sky += stars * _StarIntensity * horizonFade * (1.0 - wash) * (1.0 - discMask);

                // Luna, halo y brillo de cielo (se atenúan cerca del horizonte)
                float moonFade = smoothstep(-0.03, 0.12, y);
                sky += moonSky + (halo + moonLayer) * moonFade;

                // --- Nubes ---
                float cov = _CloudCoverage;
                if (cov > 0.001 && y > -0.02)
                {
                    // Proyección sobre un plano: las nubes se aplanan y comprimen hacia el horizonte
                    float h = max(y, 0.0) + 0.12;
                    float2 cuv = dir.xz / h * _CloudScale;
                    float2 wind = float2(1.0, 0.35) * _Time.y * _CloudSpeed;
                    float evo = _Time.y * _CloudSpeed * 0.6;

                    float detailW = _CloudDetail * saturate(y * 5.0);   // menos detalle cerca del horizonte (evita aliasing)
                    float baseN = cloudFbm(float3(cuv + wind, evo));
                    float detN = cloudFbm(float3(cuv * 3.1 + wind * 1.7 + 9.0, evo * 1.5));
                    float shape = lerp(baseN, detN, detailW);

                    // La cobertura mueve el umbral: más cobertura = más zonas por encima
                    float thr = 0.95 - 0.8 * pow(cov, 0.75);
                    float thick = saturate((shape - thr) / 0.4);
                    thick *= 1.0 + 0.8 * saturate(1.0 - y * 3.0);        // bancos más espesos hacia el horizonte

                    float opacity = (1.0 - exp(-thick * _CloudDensity)) * smoothstep(0.0, 0.03, cov);
                    opacity *= smoothstep(-0.01, 0.06, y);               // se funden con el horizonte

                    // Luz de luna sobre las nubes: bordes plateados, más fuertes cerca de la luna y con luna llena
                    float scatter = lit * (exp(-a * 14.0) + 0.25 * exp(-a * 3.0)) * _CloudMoonLight;
                    float3 cloudCol = _CloudColor.rgb
                                      + float3(0.78, 0.82, 0.95) * scatter * _MoonIntensity * 0.05 * exp(-thick);

                    sky = lerp(sky, cloudCol, opacity);                  // tapan estrellas, Vía Láctea y luna
                }

                // --- Suelo / debajo del horizonte ---
                float g = smoothstep(0.0, -0.04, y);
                float3 col = lerp(sky, _GroundColor.rgb, g);

                // --- Niebla (se mezcla con el horizonte) ---
                float fogBand = lerp(0.02, 0.7, _SkyFogAmount);
                float fogMask = (1.0 - smoothstep(0.0, fogBand, y)) * smoothstep(0.0, 0.05, _SkyFogAmount);
                col = lerp(col, _SkyFogColor.rgb, fogMask);

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
