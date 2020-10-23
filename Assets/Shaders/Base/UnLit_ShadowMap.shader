Shader "WWFramework/Base/UnLit_Shadowmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 shadowPos : TEXCOORD1;
                float depth : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            half _ShadowBias;
            half _ShadowNormalBias;
            sampler2D _ShadowMap;
            float4x4 _ShadowMatrixV;
            float4x4 _ShadowMatrixVP;
            half _ShadowFarScale;
            half3 _ShadowLightDir;
            fixed _ShadowIntensity;
            float _ShadowMapWidthScale;
            float _ShadowMapHeightScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                // COMPUTE_DEPTH_01 -(UnityObjectToViewPos( v.vertex ).z * _ProjectionParams.w)
                o.depth = - mul(_ShadowMatrixV, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0))).z * _ShadowFarScale;
                float4x4 mvp = mul(_ShadowMatrixVP, unity_ObjectToWorld);
                o.shadowPos = mul(mvp, float4(v.vertex.xyz, 1));

                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            float GetBias(half bias, half normalBias, float nl)
            {
                // nl shadowCos
                float shadowSine = sqrt(1 - nl * nl);

                return bias + shadowSine * normalBias; 
            }

            float GetShadowMapDepth(sampler2D map, float2 shadowCoord, float2 offset)
            {
//                return tex2D(map, shadowCoord + offset).r;
                return DecodeFloatRGBA(tex2D(map, shadowCoord + offset));
            }

            half SampleShadow(sampler2D map, float2 shadowCoord, float2 offset, float depth, float bias, half shadowIntensity)
            {
                float mapDepth = GetShadowMapDepth(map, shadowCoord, offset);

                // 越界检查，越界修正为不在阴影中
                float2 overstep = step(0.0001, shadowCoord) * step(shadowCoord, 0.9999);;
                depth = depth * overstep.x * overstep.y;

                return max(step(depth, mapDepth + bias), 1 - shadowIntensity);
            }

            half SampleShadowmap(sampler2D map, float2 shadowCoord, float depth, float bias, half shadowIntensity)
            {
                return SampleShadow(map, shadowCoord, 0, depth, bias, shadowIntensity);
            }

            half SampleShadowmap_PCF3x3NoHardwareSupport(sampler2D map, float2 shadowCoord, float depth, float bias, float mapWidthScale, float mapHeightScale, half shadowIntensity)
            {
                float2 base_uv = shadowCoord.xy;
                float2 ts = float2(mapWidthScale, mapHeightScale);

                half shadow = 0;
                shadow += SampleShadow(map, base_uv, float2(-ts.x, -ts.y), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(0, -ts.y), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(ts.x, -ts.y), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(-ts.x, 0), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(0, 0), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(ts.x, 0), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(-ts.x, ts.y), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(0, ts.y), depth, bias, shadowIntensity);
                shadow += SampleShadow(map, base_uv, float2(ts.x, ts.y), depth, bias, shadowIntensity);
                shadow /= 9.0;

                return shadow;
            }

            half SampleShadowmap_PCF4x4(sampler2D map, float2 shadowCoord, float depth, float bias, float mapWidthScale, float mapHeightScale, half shadowIntensity)
            {
                float2 base_uv = shadowCoord.xy;
                float2 ts = float2(mapWidthScale, mapHeightScale);

                half shadow = 0;
                float x,y;
                for (y = -1.5; y <= 1.5; y += 1.0)
                {
                    for (x = -1.5; x <= 1.5; x += 1.0)
                        {
                            shadow += SampleShadow(map, base_uv, ts * float2(x, y), depth, bias, shadowIntensity);
                        }
                }
                shadow /= 16.0;

                return shadow;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 shadowCoord = ComputeGrabScreenPos(i.shadowPos);
                shadowCoord.xy = shadowCoord.xy / shadowCoord.w;

                float3 lightDir = normalize(_ShadowLightDir);
                float3 worldNormal = normalize(i.worldNormal);
                float bias = GetBias(_ShadowBias, _ShadowNormalBias, dot(worldNormal, lightDir)) * _ShadowFarScale;
//                half atten = SampleShadowmap_PCF3x3NoHardwareSupport(_ShadowMap, shadowCoord.xy, i.depth, bias, _ShadowMapWidthScale, _ShadowMapHeightScale, _ShadowIntensity);
//                half atten = SampleShadowmap_PCF4x4(_ShadowMap, shadowCoord.xy, i.depth, bias, _ShadowMapWidthScale, _ShadowMapHeightScale, _ShadowIntensity);
                half atten = SampleShadowmap(_ShadowMap, shadowCoord.xy, i.depth, bias, _ShadowIntensity);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * atten;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}