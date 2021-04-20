// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED
#define TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED

// Since 2018.3 we changed from _TERRAIN_NORMAL_MAP to _NORMALMAP to save 1 keyword.
// Since 2019.2 terrain keywords are changed to  local keywords so it doesn't really matter. You can use both.
#if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_MAP)
    #define _TERRAIN_NORMAL_MAP
#elif !defined(_NORMALMAP) && defined(_TERRAIN_NORMAL_MAP)
    #define _NORMALMAP
#endif

struct Input
{
    float4 tc;
    #ifndef TERRAIN_BASE_PASS
        UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
    #endif
};

sampler2D _Control;
float4 _Control_ST;
float4 _Control_TexelSize;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

#define CONTROL_UNITY_DECLARE_TEX2D(tex) UNITY_DECLARE_TEX2D(tex); float4 tex##_TexelSize
#define CONTROL_UNITY_DECLARE_TEX2D_NOSAMPLER(tex) UNITY_DECLARE_TEX2D_NOSAMPLER(tex); float4 tex##_TexelSize

CONTROL_UNITY_DECLARE_TEX2D(_Control_0);
CONTROL_UNITY_DECLARE_TEX2D_NOSAMPLER(_Control_1);
CONTROL_UNITY_DECLARE_TEX2D_NOSAMPLER(_Control_2);
CONTROL_UNITY_DECLARE_TEX2D_NOSAMPLER(_Control_3);

UNITY_DECLARE_TEX2DARRAY(_DiffuseArray);
UNITY_DECLARE_TEX2DARRAY(_NormalArray);

#define TEXCOUNT 16

float _DiffuseIndexes[TEXCOUNT];
float _NoralMalIndexes[TEXCOUNT];
float4 _Splats_ST[TEXCOUNT];
float _NormalScales[TEXCOUNT];


#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)
    sampler2D _TerrainHeightmapTexture;
    sampler2D _TerrainNormalmapTexture;
    float4    _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4    _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
#endif

UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

#ifdef _NORMALMAP
    sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
    float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
#endif

#ifdef _ALPHATEST_ON
    sampler2D _TerrainHolesTexture;

    void ClipHoles(float2 uv)
    {
        float hole = tex2D(_TerrainHolesTexture, uv).r;
        clip(hole == 0.0f ? -1 : 1);
    }
#endif

#if defined(TERRAIN_BASE_PASS) && defined(UNITY_PASS_META)
    // When we render albedo for GI baking, we actually need to take the ST
    float4 _MainTex_ST;
#endif

void SplatmapVert(inout appdata_full v, out Input data)
{
    UNITY_INITIALIZE_OUTPUT(Input, data);

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)

    float2 patchVertex = v.vertex.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
    float4 uvoffset = instanceData.xyxy * uvscale;
    uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
    float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

    float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
    v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;  //(x + xBase) * hmScale.x * skipScale;
    v.vertex.y = hm * _TerrainHeightmapScale.y;
    v.vertex.w = 1.0f;

    v.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);
    v.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;

    #ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL
        v.normal = float3(0, 1, 0); // TODO: reconstruct the tangent space in the pixel shader. Seems to be hard with surface shader especially when other attributes are packed together with tSpace.
        data.tc.zw = sampleCoords;
    #else
        float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
        v.normal = 2.0f * nor - 1.0f;
    #endif
#endif

    v.tangent.xyz = cross(v.normal, float3(0,0,1));
    v.tangent.w = -1;

    data.tc.xy = v.texcoord.xy;
#ifdef TERRAIN_BASE_PASS
    #ifdef UNITY_PASS_META
        data.tc.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
    #endif
#else
    float4 pos = UnityObjectToClipPos(v.vertex);
    UNITY_TRANSFER_FOG(data, pos);
#endif
}

#ifndef TERRAIN_BASE_PASS

#define CONTROL_UV(tex, coord) (coord * (tex##_TexelSize.zw - 1.0f) + 0.5f) * tex##_TexelSize.xy
#define SplatmapControl(index, coord) \
    float2 splatUV_##index = CONTROL_UV(_Control_##index, coord); \
    half4 splat_control_##index = UNITY_SAMPLE_TEX2D_SAMPLER(_Control_##index, _Control_0, splatUV_##index); \
    half weight_##index = dot(splat_control_##index, half4(1,1,1,1)); \
    // splat_control_##index /= (weight_##index + 1e-3f); \

#define SplatmapMixOne(diffuse, normal, index, controlIndex, channel, coord) \
    fixed diffuse_index_##index = _DiffuseIndexes[index]; \
    fixed normal_index_##index = _NoralMalIndexes[index]; \
    float4 splat_##index = _Splats_ST[index]; \
    float2 uvSplat_##index = coord * splat_##index.xy + splat_##index.zw; \
    diffuse += splat_control_##controlIndex.##channel * UNITY_SAMPLE_TEX2DARRAY(_DiffuseArray, float3(uvSplat_##index, diffuse_index_##index)) * half4(1.0, 1.0, 1.0, defaultAlpha.##channel) * saturate(diffuse_index_##index + 1); \
    normal += UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(uvSplat_##index, normal_index_##index)), _NormalScales[index]) * splat_control_##controlIndex.##channel * saturate(diffuse_index_##index + 1);

#define SplatmapMixFour(diffuse, normal, controlIndex, coord) \
    SplatmapControl(controlIndex, coord); \
    fixed index_##controlIndex##_0 = controlIndex * 4 + 0; \
    fixed index_##controlIndex##_1 = controlIndex * 4 + 1; \
    fixed index_##controlIndex##_2 = controlIndex * 4 + 2; \
    fixed index_##controlIndex##_3 = controlIndex * 4 + 3; \
    SplatmapMixOne(diffuse, normal, index_##controlIndex##_0, controlIndex, r, coord); \
    SplatmapMixOne(diffuse, normal, index_##controlIndex##_1, controlIndex, g, coord); \
    SplatmapMixOne(diffuse, normal, index_##controlIndex##_2, controlIndex, b, coord); \
    SplatmapMixOne(diffuse, normal, index_##controlIndex##_3, controlIndex, a, coord); \

#ifdef TERRAIN_STANDARD_SHADER
void SplatmapMix(Input IN, half4 defaultAlpha, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
#else
void SplatmapMix(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
#endif
{
    #ifdef _ALPHATEST_ON
        ClipHoles(IN.tc.xy);
    #endif

    // adjust splatUVs so the edges of the terrain tile lie on pixel centers
    //float2 splatUV = (IN.tc.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    //splat_control = tex2D(_Control, splatUV);
    //weight = dot(splat_control, half4(1,1,1,1));

    splat_control = 1;
    weight = 1;
    // SplatmapControl(0, IN.tc.xy, splat_control, weight);

    #if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
        clip(weight == 0.0f ? -1 : 1);
    #endif

    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    // splat_control /= (weight + 1e-3f);

    float2 uvSplat0 = TRANSFORM_TEX(IN.tc.xy, _Splat0);
    float2 uvSplat1 = TRANSFORM_TEX(IN.tc.xy, _Splat1);
    float2 uvSplat2 = TRANSFORM_TEX(IN.tc.xy, _Splat2);
    float2 uvSplat3 = TRANSFORM_TEX(IN.tc.xy, _Splat3);

    // float index0 = _Indexes[0];
    // float index1 = _Indexes[1];
    // float4 splat0 = _Splats_ST[index0];
    // float4 splat1 = _Splats_ST[index1];
    // uvSplat0 = IN.tc.xy * splat0.xy + splat0.zw;
    // uvSplat1 = IN.tc.xy * splat1.xy + splat1.zw;

    mixedDiffuse = 0.0f;
    // #ifdef TERRAIN_STANDARD_SHADER
    //     mixedDiffuse += splat_control.r * tex2D(_Splat0, uvSplat0) * half4(1.0, 1.0, 1.0, defaultAlpha.r);
    //     mixedDiffuse += splat_control.g * tex2D(_Splat1, uvSplat1) * half4(1.0, 1.0, 1.0, defaultAlpha.g);
    //     mixedDiffuse += splat_control.b * tex2D(_Splat2, uvSplat2) * half4(1.0, 1.0, 1.0, defaultAlpha.b);
    //     mixedDiffuse += splat_control.a * tex2D(_Splat3, uvSplat3) * half4(1.0, 1.0, 1.0, defaultAlpha.a);
    // #else
    //     mixedDiffuse += splat_control.r * tex2D(_Splat0, uvSplat0);
    //     mixedDiffuse += splat_control.g * tex2D(_Splat1, uvSplat1);
    //     mixedDiffuse += splat_control.b * tex2D(_Splat2, uvSplat2);
    //     mixedDiffuse += splat_control.a * tex2D(_Splat3, uvSplat3);
    // #endif

    // mixedDiffuse += splat_control.r * UNITY_SAMPLE_TEX2DARRAY(_DiffuseArray, float3(uvSplat0, index0)) * half4(1.0, 1.0, 1.0, defaultAlpha.r);
    // mixedDiffuse += splat_control.g * UNITY_SAMPLE_TEX2DARRAY(_DiffuseArray, float3(uvSplat1, index1)) * half4(1.0, 1.0, 1.0, defaultAlpha.g);

    #ifdef _NORMALMAP
        // mixedNormal  = UnpackNormalWithScale(tex2D(_Normal0, uvSplat0), _NormalScale0) * splat_control.r;
        // mixedNormal += UnpackNormalWithScale(tex2D(_Normal1, uvSplat1), _NormalScale1) * splat_control.g;
        // mixedNormal += UnpackNormalWithScale(tex2D(_Normal2, uvSplat2), _NormalScale2) * splat_control.b;
        // mixedNormal += UnpackNormalWithScale(tex2D(_Normal3, uvSplat3), _NormalScale3) * splat_control.a;

        mixedNormal = 0;

        SplatmapMixFour(mixedDiffuse, mixedNormal, 0, IN.tc.xy);
        SplatmapMixFour(mixedDiffuse, mixedNormal, 1, IN.tc.xy);
        SplatmapMixFour(mixedDiffuse, mixedNormal, 2, IN.tc.xy);
        SplatmapMixFour(mixedDiffuse, mixedNormal, 3, IN.tc.xy);

        // mixedNormal  = UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(uvSplat0, index0)), _NormalScale0) * splat_control.r;
        // mixedNormal += UnpackNormalWithScale(UNITY_SAMPLE_TEX2DARRAY(_NormalArray, float3(uvSplat1, index1)), _NormalScale1) * splat_control.g;
        mixedNormal.z += 1e-5f; // to avoid nan after normalizing
    #endif

    #if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        mixedNormal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
    #endif

    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        float3 geomNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
        #ifdef _NORMALMAP
            float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
            float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
            mixedNormal = mixedNormal.x * geomTangent
                          + mixedNormal.y * geomBitangent
                          + mixedNormal.z * geomNormal;
        #else
            mixedNormal = geomNormal;
        #endif
        mixedNormal = mixedNormal.xzy;
    #endif
}

#ifndef TERRAIN_SURFACE_OUTPUT
    #define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
    // color *= o.Alpha;
    #ifdef TERRAIN_SPLAT_ADDPASS
        UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
    #else
        UNITY_APPLY_FOG(IN.fogCoord, color);
    #endif
}

void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
{
    // normalSpec *= o.Alpha;
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
    UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
    // emission *= o.Alpha;
}

#endif // TERRAIN_BASE_PASS

#endif // TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED
