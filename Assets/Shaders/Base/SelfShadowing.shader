Shader "WWFramework/Base/SelfShadowing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		_ShadowOffset ("ShadowOffset", Range(0, 0.1)) = 0.01
		_ShadowTexture ("Shadow Texture", 2D) = "black" { }
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 shadowMatrix: TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _ShadowOffset;
			sampler2D _ShadowTexture;
			float4x4 _ShadowMatrix;
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float4x4 mvp = mul(_ShadowMatrix, unity_ObjectToWorld);
				o.shadowMatrix = mul(mvp, float4(v.vertex.xyz, 1));

				return o;
			}
			
			fixed4 frag(v2f i): SV_Target
			{
				float4 uvPos = i.shadowMatrix;
				// uvPos.x = uvPos.x * 0.5f + uvPos.w * 0.5f;//变换到[0,w]
				// uvPos.y = uvPos.y * 0.5f + uvPos.w * 0.5f;//变换到[0,w]

				// #if UNITY_UV_STARTS_AT_TOP	  //Dx like
				// 	uvPos.y = uvPos.w - uvPos.y;
				// #endif

				uvPos = ComputeGrabScreenPos(uvPos);

				float depth = tex2D(_ShadowTexture, uvPos.xy / uvPos.w).r;//从深度图中取出深度
				float depthPixel = uvPos.z / uvPos.w;//像素深度

				#if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)) && defined(SHADER_API_MOBILE)
					//GL like
					depthPixel = depthPixel * 0.5f + 0.5f;
				#else
					//DX like
					depthPixel = depthPixel;
				#endif

				//使用一个偏移值，手动调整深度的误差
				float shadowCol = 1.0f;
				#if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)) && defined(SHADER_API_MOBILE)
					if(depthPixel - depth > _ShadowOffset)
						shadowCol = 0.7;
				#else
					if(depthPixel - depth < - _ShadowOffset)
						shadowCol = 0.7;
				#endif

				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = col.rgb * shadowCol;

				return col;
			}
			ENDCG
			
		}
	}

	// Fallback "Mobile/VertexLit"
	// Fallback "Legacy Shaders/VertexLit"
}
