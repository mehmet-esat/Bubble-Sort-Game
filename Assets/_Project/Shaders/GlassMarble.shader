Shader "ColorSortPuzzle/GlassMarble"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Mask", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "GlassMarble"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float2 Rotate2D(float2 vectorValue, float radians)
            {
                float sine;
                float cosine;
                sincos(radians, sine, cosine);
                return float2(vectorValue.x * cosine - vectorValue.y * sine, vectorValue.x * sine + vectorValue.y * cosine);
            }

            float RoundedReflection(float2 samplePosition, float2 reflectionCenter, float2 halfSize, float radians, float edgeSoftness)
            {
                float2 localPoint = abs(Rotate2D(samplePosition - reflectionCenter, radians)) - halfSize;
                float distanceToBox = length(max(localPoint, 0.0));
                return 1.0 - smoothstep(0.0, edgeSoftness, distanceToBox);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // UV merkezini birim küre yüzeyine dönüştürür.
                float2 p = input.uv * 2.0 - 1.0;
                float radialDistance = length(p);
                float circleFeather = max(fwidth(radialDistance) * 1.5, 0.002);
                half maskAlpha = 1.0 - smoothstep(1.0 - circleFeather, 1.0 + circleFeather, radialDistance);
                if (maskAlpha <= 0.0h)
                    discard;
                float z = sqrt(saturate(1.0 - dot(p, p)));
                float3 normal = float3(p, z);

                // Sol-üstten gelen ana ışık ve doygun cam gövde gradyanı.
                float3 lightDirection = normalize(float3(-0.5, 0.6, 0.65));
                float diffuse = pow(saturate(dot(normal, lightDirection)), 0.78);
                float brightness = lerp(0.60, 1.06, diffuse);

                // Geniş üst cam katmanı: referanstaki doygun, yumuşak parlak yüzey.
                float3 viewDirection = float3(0.0, 0.0, 1.0);
                float3 halfDirection = normalize(lightDirection + viewDirection);
                brightness += pow(saturate(dot(normal, halfDirection)), 3.0) * 0.18;

                // Alt-sağda ince renkli bounce/rim yansıması.
                float edge = smoothstep(0.72, 1.0, radialDistance);
                float lowerRight = saturate(p.x * 0.5 - p.y * 0.5);
                brightness += edge * lowerRight * 0.12;

                half luminance = dot(_Color.rgb, half3(0.2126h, 0.7152h, 0.0722h));
                half3 vividBaseColor = saturate(lerp(half3(luminance, luminance, luminance), _Color.rgb, 1.25h));
                half3 bodyColor = vividBaseColor * saturate(brightness);

                // Kenarlar koyu/doygun, merkez canlı: dairesel cam derinliği.
                float edgeDepth = smoothstep(0.30, 1.0, radialDistance);
                bodyColor *= lerp(1.10, 0.76, edgeDepth);

                // Merkezde düşük opaklıklı inner glow; jöle/cam derinliği verir.
                float innerGlow = 1.0 - smoothstep(0.08, 0.82, radialDistance);
                bodyColor = lerp(bodyColor, saturate(vividBaseColor * 1.22h), innerGlow * 0.24);

                // Alt yarıda pürüzsüz iç derinlik. Dalgalı gölge yerine tek, kontrollü geçiş.
                float lowerDepth = (1.0 - smoothstep(-0.72, -0.04, p.y)) * (1.0 - smoothstep(0.68, 1.0, radialDistance));
                bodyColor *= lerp(1.0, 0.82, lowerDepth);

                // Parlak, renkli dış rim topu cam küre gibi çerçeveler.
                float outerRim = smoothstep(0.80, 0.98, radialDistance);
                half3 rimColor = saturate(vividBaseColor * 1.35h + 0.12h);
                bodyColor = lerp(bodyColor, rimColor, outerRim * 0.68);
                bodyColor = saturate(bodyColor);

                // Geniş yarı saydam üst cam katmanı ve iki karemsi beyaz reflection.
                float upperGloss = 1.0 - smoothstep(0.30, 1.0, length((p - float2(-0.08, 0.30)) / float2(0.82, 0.42)));
                float mainHighlight = RoundedReflection(p, float2(-0.33, 0.31), float2(0.13, 0.075), -0.45, 0.075) * 0.78;
                float secondaryHighlight = RoundedReflection(p, float2(-0.49, 0.13), float2(0.075, 0.045), -0.45, 0.060) * 0.46;

                // Alt kenarda ince beyaz camsı yansıma.
                float lowerArc = (1.0 - smoothstep(0.30, 0.62, length((p - float2(0.05, -0.57)) / float2(0.55, 0.12)))) * 0.24;
                half3 lowerArcColor = saturate(vividBaseColor * 1.35h + 0.06h);
                bodyColor = lerp(bodyColor, lowerArcColor, lowerArc);

                // Sağ kenardaki ince dikey cam yansıması.
                float sideReflection = smoothstep(0.88, 0.98, p.x) * smoothstep(-0.60, -0.05, p.y) * (1.0 - smoothstep(0.20, 0.65, p.y)) * 0.28;
                float whiteReflection = saturate(max(max(mainHighlight, secondaryHighlight), sideReflection * 0.50));

                half3 glassLayer = lerp(bodyColor, half3(1.0h, 1.0h, 1.0h), upperGloss * 0.18);
                half3 finalColor = lerp(glassLayer, half3(1.0h, 1.0h, 1.0h), whiteReflection);

                // Cam hissi iç katmanlarla verilir; mat ızgarasının topun içinden görünmesi engellenir.
                return half4(finalColor, _Color.a * maskAlpha);
            }
            ENDHLSL
        }
    }
}
