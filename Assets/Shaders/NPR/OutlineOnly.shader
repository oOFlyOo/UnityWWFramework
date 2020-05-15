Shader "WWFramework/NPR/OutlineOnly"
{
	Properties
	{
		_Outline ("Outline Width", Float) = 1
		_OutlineColor ("Outline Color", Color) = (0, 0, 0, 0)
	}
	SubShader
	{
		Name "OUTLINE"

		Cull Front

		Pass
		{
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
