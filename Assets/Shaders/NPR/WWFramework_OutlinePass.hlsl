#ifndef WWFRAMEWORK_OUTLINE_PASS_INCLUDED
	#define WWFRAMEWORK_OUTLINE_PASS_INCLUDED


	#include "../WWFramework_URP_NPR.hlsl"

	struct Attributes
	{
		float4 positionOS: POSITION;
		float3  normalOS: NORMAL;

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Varyings
	{
		float4 vertex: SV_POSITION;
		float fogCoord: TEXCOORD0;

		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Varyings vert(Attributes input)
	{
		Varyings output = (Varyings)0;

		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_TRANSFER_INSTANCE_ID(input, output);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

		VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

		output.vertex = ViewSpaceOutline(input.positionOS, input.normalOS, _Outline * 0.01);
		output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

		return output;
	}

	half4 frag(Varyings input): SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		half3 color = _OutlineColor.rgb;

		color = MixFog(color, input.fogCoord);

		return half4(color, 1);
	}

#endif