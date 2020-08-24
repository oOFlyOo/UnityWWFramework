Shader "WWFramework/Base/DepthMap" {
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 depth : TEXCOORD0;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                o.depth = o.pos.zw;

                // COMPUTE_EYEDEPTH(o.depth.x);

                return o;
            }

            half4 frag(v2f i) : SV_Target {
                float depth = i.depth.x / i.depth.y;

                // float depth = DECODE_EYEDEPTH(i.depth.x);

                return EncodeFloatRGBA(depth);
            }
            ENDCG
        }
    }
}