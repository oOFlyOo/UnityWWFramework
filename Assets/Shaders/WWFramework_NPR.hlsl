
// 非真实渲染


// 简单理解就是模型放大，计算简单，内部褶皱也会产生描边
// 轮廓宽度无法精确保证，对于法线突变的模型的不适应性
// 返回模型空间位置
float4 ObjectSpaceOutline(float4 pos, float3 normal, fixed outlineWidth)
{
    return pos + float4(normal, 0) * outlineWidth;
}


// 由于都是在同一平面，因此只有外轮廓有描边
// 返回裁剪空间坐标
float4 ViewSpaceOutline(float4 pos, float3 normal, fixed outlineWidth)
{
    float4 viewPos = float4(UnityObjectToViewPos(pos), 1);
    float3 viewNormal = UnityObjectToViewPos(normal);
    // 同一平面深度
    viewNormal.z = -0.5;
    // 保证宽度一致
    viewPos += float4(normalize(viewNormal), 0) * outlineWidth;

    return mul(UNITY_MATRIX_P, viewPos);
}


// 随着远近描边会发生变化
// 返回裁剪坐标空间
float4 ClipSpaceOutline(float4 clipPos, float3 normal, fixed outlineWidth)
{
	float3 viewNormal = normalize(mul((half3x3)UNITY_MATRIX_IT_MV, normal));
	float2 offset = TransformViewToProjection(viewNormal.xy);
	clipPos.xy += offset * outlineWidth;
	return clipPos;
}


// 不会随着远近描边会发生变化
// 返回裁剪坐标空间
float4 ClipSpaceNoneScaleOutline(float4 clipPos, float3 normal, fixed outlineWidth)
{
	float3 viewNormal = normalize(mul((half3x3)UNITY_MATRIX_IT_MV, normal));
	float2 offset = TransformViewToProjection(viewNormal.xy);
	clipPos.xy += offset * outlineWidth * clipPos.w;
	return clipPos;
}