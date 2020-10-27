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
            "RenderType"="Opaque" "DepthType"="DrawDepth"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "../WWFramework_Shadowmap.hlsl"

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