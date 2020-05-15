
// 光照相关


half DiffuseLambert(half3 normalizeWorldLightDir, half3 normallizeWorldNormal)
{
    return saturate(dot(normalizeWorldLightDir, normallizeWorldNormal));
}


half DiffuseHalfLambert(half3 normalizeWorldLightDir, half3 normallizeWorldNormal)
{
    return (dot(normalizeWorldLightDir, normallizeWorldNormal) + 1) * 0.5;
}


// 光照方向，指向光源
half SpecularPhong(float3 worldLightDir, float3 worldNormal, half3 normalizeViewDir, half3 gloss)
{
    half3 reflectDir = normalize(reflect(-worldLightDir, worldNormal));

    return pow(saturate(dot(reflectDir, normalizeViewDir)), gloss);
}


half SpecularBlinnPhong(half3 normalizeWorldLightDir, half3 normallizeWorldNormal, half3 normalizeViewDir, half3 gloss)
{
    half3 halfDir = normalize(normalizeWorldLightDir + normalizeViewDir);

    return pow(saturate(dot(halfDir, normallizeWorldNormal)), gloss);
}