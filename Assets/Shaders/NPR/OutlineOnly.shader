Shader "WWFramework/NPR/OutlineOnly"
{
	Properties
	{
		_Outline ("Outline Width", Range(0, 10)) = 1
		_OutlineColor ("Outline Color", Color) = (0, 0, 0, 0)
	}
	SubShader
	{
		Pass
		{
			Name "OUTLINE"

			Cull Front

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "WWFramework_Outline.hlsl"

			ENDCG
		}
	}
}
