#ifndef WWFRAMEWORK_URP_NPR_INCLUDED
	#define WWFRAMEWORK_URP_NPR_INCLUDED

	// 非真实渲染

	// 摄像机Z偏移，用于描边排除内描边
	float4 ClipSpaceZOffset(float4 clipPos, half offsetZ)
	{
		#if defined(UNITY_REVERSED_Z)
			//v.2.0.4.2 (DX)
			offsetZ = offsetZ * - 0.01;
		#else
			//OpenGL
			offsetZ = offsetZ * 0.01;
		#endif

		float4 clipCameraPos = TransformWorldToHClip(_WorldSpaceCameraPos.xyz);
		clipPos.z += offsetZ * clipCameraPos.z;

		return clipPos;
	}


	// 简单理解就是模型放大，计算简单，内部褶皱也会产生描边
	// 轮廓宽度无法精确保证，对于法线突变的模型的不适应性
	// 返回模型空间位置
	float4 ObjectSpaceOutline(float4 pos, float3 normal, half outlineWidth)
	{
		return pos + float4(normal * outlineWidth, 0);
	}


	// 由于都是在同一平面，因此只有外轮廓有描边
	// 返回裁剪空间坐标
	float4 ViewSpaceOutline(float4 pos, float3 normal, half outlineWidth, half zSmooth)
	{
		float3 viewPos = TransformWorldToView(TransformObjectToWorld(pos));
		// float3 viewNormal = UnityObjectToViewPos(normal);
		real3 viewNormal = TransformWorldToViewDir(TransformObjectToWorldNormal(pos));
		// 同一平面深度
		viewNormal.z = -zSmooth;
		// 保证宽度一致
		viewPos += normalize(viewNormal) * outlineWidth;

		return TransformWViewToHClip(viewPos);
	}


	// 由于都是在同一平面，因此只有外轮廓有描边
	// 返回裁剪空间坐标
	float4 ViewSpaceOutline(float4 pos, float3 normal, half outlineWidth)
	{
		return ViewSpaceOutline(pos, normal, outlineWidth, 0.5);
	}


	// 随着远近描边会发生变化
	// 返回裁剪坐标空间
	float4 ClipSpaceOutline(float4 clipPos, float3 normal, half outlineWidth)
	{
		real3 viewNormal = TransformWorldToViewDir(TransformObjectToWorldNormal(normal));
		float2 offset = TransformWViewToHClip(viewNormal);
		clipPos.xy += normalize(offset) * outlineWidth;
		return clipPos;
	}


	// 不会随着远近描边会发生变化
	// 返回裁剪坐标空间
	float4 ClipSpaceNoneScaleOutline(float4 clipPos, float3 normal, half outlineWidth)
	{
		real3 viewNormal = TransformWorldToViewDir(TransformObjectToWorldNormal(normal));
		float2 offset = TransformWViewToHClip(viewNormal);
		clipPos.xy += normalize(offset) * outlineWidth * clipPos.w;
		return clipPos;
	}

	half LerpRampNL(half nl, half rampMinStep, half rampMinThreshold, half rampMaxStep, half rampMaxThreshold)
	{
		// return lerp(lerp(0, rampMinThreshold, smoothstep(0, rampMinStep, nl)), lerp(rampMinThreshold, 1, smoothstep(rampMinStep, 1, nl)), nl);
		return lerp(lerp(0, rampMinThreshold, smoothstep(0, rampMinStep, nl)), lerp(rampMaxThreshold, 1, smoothstep(rampMaxStep, 1, nl)), nl);
	}

	half StepRampNL(half nl, half rampMinStep, half rampMinThreshold, half rampMaxStep, half rampMaxThreshold)
	{
		half min = lerp(0, rampMinThreshold, smoothstep(0, rampMinStep, nl));
		half max = lerp(rampMaxThreshold, 1, smoothstep(rampMaxStep, 1, nl));
		half mid = lerp(min, max, nl);
		return min * step(1 - rampMinStep, 1- nl) + mid * step(rampMinStep, nl) * step(nl, rampMaxStep) + max * step(rampMaxStep, nl);
	}



	half WrapRampNL(half nl, half threshold, half smoothness)
	{
		nl = smoothstep(threshold - smoothness, threshold + smoothness, nl);

		return nl;
	}

	half WrapRampHalfNL(half nl, half threshold, half smoothness)
	{
		return WrapRampNL(nl * 0.5 + 0.5, threshold, smoothness);
	}


	half StylizedSpecular(half specularTerm, half specSmoothness, half specBlend)
	{
		half customSpecularTerm = smoothstep(specSmoothness * 0.5, specSmoothness * 0.5 + 0.5, specularTerm);

		return lerp(specularTerm, customSpecularTerm, specBlend);
	}


	// nv (0, 1)
	half3 StylizedFresnelNoneLightingDirection(half nv, half smoothness, half3 rimParams, half3 lightCol)
	{
		half rim = 1 - nv;
		rim = smoothstep(rimParams.x, rimParams.y, rim) * rimParams.z * saturate(0.33 + smoothness);
		return rim;
	}

	// nv (0, 1)
	half3 StylizedFresnel(half nv, half smoothness, half3 normalWS, half3 rimParams, half3 lightDir, half3 lightCol)
	{
		half rim = 1 - nv;
		rim = smoothstep(rimParams.x, rimParams.y, rim) * rimParams.z * saturate(0.33 + smoothness);
		return rim * saturate(dot(normalWS, lightDir)) * lightCol;
	}
#endif