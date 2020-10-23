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
                float depth : TEXCOORD0;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                o.depth = COMPUTE_DEPTH_01;

                return o;
            }

            half4 frag(v2f i) : SV_Target {
//                return half4(i.depth, i.depth, i.depth, 1);
                return EncodeFloatRGBA(i.depth);
            }
            ENDCG
        }
    }
}