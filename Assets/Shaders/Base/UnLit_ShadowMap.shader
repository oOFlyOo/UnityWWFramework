Shader "WWFramework/Base/UnLit_Shadowmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // _ShadowMap ("Shadow Map", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 shadowPos: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _ShadowBias;
			sampler2D _ShadowMap;
			float4x4 _ShadowMatrix;
            fixed _ShadowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float4x4 mvp = mul(_ShadowMatrix, unity_ObjectToWorld);
				o.shadowPos = mul(mvp, float4(v.vertex.xyz, 1));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // xy 变换到 [0, w]
                float4 shadowCoord = ComputeGrabScreenPos(i.shadowPos);
                // xy 变换到 [0, 1]
                shadowCoord.xyz = shadowCoord.xyz / shadowCoord.w;
                // float shadowMapDepth = UnityDecodeCubeShadowDepth(tex2D(_ShadowMap, shadowCoord.xy));
                float shadowMapDepth = DecodeFloatRGBA(tex2D(_ShadowMap, shadowCoord.xy));
                #if defined(UNITY_REVERSED_Z)
                    shadowMapDepth = 1 - shadowMapDepth;
                #endif

                float depth = shadowCoord.z / shadowCoord.w;
                #if !defined(UNITY_REVERSED_Z)
                    // [-1, 1] -> [0, 1]
                    depth = depth * 0.5 + 0.5;
                #endif
                #if defined(UNITY_REVERSED_Z)
                    depth = 1 - depth;
                #endif

                // 方便测试，临时做限定，正式不需要
                // depth = min(depth, 1);
                // depth = max(depth, 0);

                fixed atten = 1;
                if (depth - shadowMapDepth > _ShadowBias)
                {
                    atten = lerp(1, 0, _ShadowIntensity);
                }

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
