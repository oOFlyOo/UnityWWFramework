
// 光照相关


fixed DiffuseLambert(fixed3 normalizeWorldLightDir, fixed3 normallizeWorldNormal)
{
    return saturate(dot(normalizeWorldLightDir, normallizeWorldNormal));
}


fixed DiffuseHalfLambert(fixed3 normalizeWorldLightDir, fixed3 normallizeWorldNormal)
{
    return (dot(normalizeWorldLightDir, normallizeWorldNormal) + 1) * 0.5;
}


// 光照方向，指向光源
fixed SpecularPhong(float3 worldLightDir, float3 worldNormal, fixed3 normalizeViewDir, fixed3 gloss)
{
    fixed3 reflectDir = normalize(reflect(-worldLightDir, worldNormal));
    
    return pow(saturate(dot(reflectDir, normalizeViewDir)), gloss);
}


fixed SpecularBlinnPhong(fixed3 normalizeWorldLightDir, fixed3 normallizeWorldNormal, fixed3 normalizeViewDir, fixed3 gloss)
{
    fixed3 halfDir = normalize(normalizeWorldLightDir + normalizeViewDir);

    return pow(saturate(dot(halfDir, normallizeWorldNormal)), gloss);
}