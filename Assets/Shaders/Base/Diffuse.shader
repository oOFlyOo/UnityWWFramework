Shader "WWFramework/Base/Diffuse"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase

			#include "Lighting.cginc"
			#include "../WWFramework_Light.hlsl"

			struct appdata
			{
				half4 vertex: POSITION;
				#ifndef LIGHTMAP_ON
					half3 normal: NORMAL;
				#endif
				float4 uv: TEXCOORD0;
				float4 uv2: TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
				UNITY_FOG_COORDS(1)
				#ifdef LIGHTMAP_ON
					float2 lmap: TEXCOORD2;
				#endif
				#ifndef LIGHTMAP_ON
					float3 worldNormal: TEXCOORD2;
					#if UNITY_SHOULD_SAMPLE_SH
						half3 vertexLight: TEXCOORD3;
					#endif
				#endif
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				#ifdef LIGHTMAP_ON
					o.lmap.xy = v.uv2.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				#ifndef LIGHTMAP_ON
					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
					o.worldNormal = worldNormal;
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

					#if UNITY_SHOULD_SAMPLE_SH
						o.vertexLight = 0;

						#ifdef VERTEXLIGHT_ON
							o.vertexLight += Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb, unity_4LightAtten0, worldPos, worldNormal);
							// o.vertexLight += Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, 0, 0, 0, 0, unity_4LightAtten0, worldPos, worldNormal);
						#endif
						// o.vertexLight = half3(0, 0, 0);
						o.vertexLight = ShadeSHPerVertex(worldNormal, o.vertexLight);
					#endif
				#endif

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i): SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				#ifdef LIGHTMAP_ON
					fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap.xy));
					col.rgb = col.rgb * lm;
				#endif
				#ifndef LIGHTMAP_ON
					#if UNITY_SHOULD_SAMPLE_SH
						fixed3 ambient = i.vertexLight;
					#else
						fixed3 ambient = 0;
					#endif
					fixed3 diffuse = _LightColor0.rgb * DiffuseLambert(normalize(i.worldNormal), _WorldSpaceLightPos0.xyz);
					col.rgb = col.rgb * (ambient + diffuse);
				#endif
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG

		}


		Pass
		{
			Tags { "LightMode" = "ForwardAdd" }
			Blend One One
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma multi_compile_fwdadd

			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "../WWFramework_Light.hlsl"

			struct appdata
			{
				half4 vertex: POSITION;
				half3 normal: NORMAL;
				float4 uv: TEXCOORD0;
				float4 uv2: TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float3 worldNormal: TEXCOORD2;
				float3 worldPos: TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i): SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
				fixed3 diffuse = _LightColor0.rgb * DiffuseLambert(normalize(i.worldNormal), normalize(UnityWorldSpaceLightDir(i.worldPos)));
				col.rgb = col.rgb * (diffuse * atten);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG

		}
	}
}
