

half _ShadowBias;
half _ShadowNormalBias;
sampler2D _ShadowMap;
float4x4 _ShadowMatrixV;
float4x4 _ShadowMatrixVP;
half _ShadowFarScale;
half3 _ShadowLightDir;
fixed _ShadowIntensity;
float _ShadowMapWidthScale;
float _ShadowMapHeightScale;


float GetBias(half bias, half normalBias, float nl)
{
    // nl shadowCos
    float shadowSine = sqrt(1 - nl * nl);

    return bias + shadowSine * normalBias; 
}

float GetShadowMapDepth(sampler2D map, float2 shadowCoord, float2 offset)
{
//                return tex2D(map, shadowCoord + offset).r;
    return DecodeFloatRGBA(tex2D(map, shadowCoord + offset));
}

half SampleShadow(sampler2D map, float2 shadowCoord, float2 offset, float depth, float bias, half shadowIntensity)
{
    float mapDepth = GetShadowMapDepth(map, shadowCoord, offset);

    // 越界检查，越界修正为不在阴影中
    float2 overstep = step(0.0001, shadowCoord) * step(shadowCoord, 0.9999);;
    depth = depth * overstep.x * overstep.y;

    return max(step(depth, mapDepth + bias), 1 - shadowIntensity);
}

half SampleShadowmap(sampler2D map, float2 shadowCoord, float depth, float bias, half shadowIntensity)
{
    return SampleShadow(map, shadowCoord, 0, depth, bias, shadowIntensity);
}

half SampleShadowmap_PCF3x3NoHardwareSupport(sampler2D map, float2 shadowCoord, float depth, float bias, float mapWidthScale, float mapHeightScale, half shadowIntensity)
{
    float2 base_uv = shadowCoord.xy;
    float2 ts = float2(mapWidthScale, mapHeightScale);

    half shadow = 0;
    shadow += SampleShadow(map, base_uv, float2(-ts.x, -ts.y), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(0, -ts.y), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(ts.x, -ts.y), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(-ts.x, 0), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(0, 0), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(ts.x, 0), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(-ts.x, ts.y), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(0, ts.y), depth, bias, shadowIntensity);
    shadow += SampleShadow(map, base_uv, float2(ts.x, ts.y), depth, bias, shadowIntensity);
    shadow /= 9.0;

    return shadow;
}

half SampleShadowmap_PCF4x4(sampler2D map, float2 shadowCoord, float depth, float bias, float mapWidthScale, float mapHeightScale, half shadowIntensity)
{
    float2 base_uv = shadowCoord.xy;
    float2 ts = float2(mapWidthScale, mapHeightScale);

    half shadow = 0;
    float x,y;
    for (y = -1.5; y <= 1.5; y += 1.0)
    {
        for (x = -1.5; x <= 1.5; x += 1.0)
            {
                shadow += SampleShadow(map, base_uv, ts * float2(x, y), depth, bias, shadowIntensity);
            }
    }
    shadow /= 16.0;

    return shadow;
}
