Shader "WWFramework/Toon/UnlitToon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_OutlineWidth ("描边宽度", Float) = 0.01
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
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
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float2 uv: TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex: SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i): SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG

		}


		Pass
		{
			Cull Front

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "../WWFramework_NPR.hlsl"

			fixed _OutlineWidth;

			struct appdata
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f
			{
				UNITY_FOG_COORDS(0)
				float4 vertex: SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				float4 clipPos = UnityObjectToClipPos(v.vertex);
				// o.vertex = UnityObjectToClipPos(ObjectSpaceOutline(v.vertex, v.normal, _OutlineWidth));
				// o.vertex = ViewSpaceOutline(v.vertex, v.normal, _OutlineWidth);
				o.vertex = ClipSpaceOutline(clipPos, v.normal, _OutlineWidth);

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}


			fixed4 frag(v2f i): SV_Target
			{
				fixed4 col = fixed4(0, 0, 0, 0);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG

		}
	}
}
