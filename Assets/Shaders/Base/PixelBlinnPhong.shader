Shader "WWFramework/Base/PixelBlinnPhong"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Gloss ("光泽度", Range(1, 256)) = 128
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "../WWFramework_Light.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldLightDir : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float3 worldViewDir : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Gloss;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.worldLightDir = WorldSpaceLightDir(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldViewDir = WorldSpaceViewDir(v.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				fixed3 worldLightDir = normalize(i.worldLightDir);
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldViewDir = normalize(i.worldViewDir);

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT;
				fixed3 light = ambient + _LightColor0.rgb * DiffuseLambert(worldLightDir, worldNormal) + 
				_LightColor0.rgb * SpecularBlinnPhong(worldLightDir, worldNormal, worldViewDir, _Gloss);

				return fixed4(col.rgb * light, 1);
			}
			ENDCG
		}
	}
}
