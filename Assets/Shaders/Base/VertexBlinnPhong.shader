Shader "WWFramework/Base/VertexBlinnPhong"
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
				fixed3 color : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Gloss;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT;

				fixed3 worldLightDir = normalize(WorldSpaceLightDir(v.vertex));
				fixed3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
				fixed3 worldViewDir = normalize(WorldSpaceViewDir(v.vertex));

				o.color = ambient + _LightColor0.rgb * DiffuseLambert(worldLightDir, worldNormal) + 
				_LightColor0.rgb * SpecularBlinnPhong(worldLightDir, worldNormal, worldViewDir, _Gloss);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				return fixed4(col.rgb * i.color, 1);
			}
			ENDCG
		}
	}
}
