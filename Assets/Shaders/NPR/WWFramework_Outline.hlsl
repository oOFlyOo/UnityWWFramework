
#include "../WWFramework_NPR.hlsl"

struct a2v
{
	float4 vertex: POSITION;
	float3 normal: NORMAL;
};

struct v2f
{
	UNITY_FOG_COORDS(0)
	float4 vertex: SV_POSITION;
};

float _Outline;
fixed4 _OutlineColor;

v2f vert(a2v v)
{
	v2f o;
    float4 clipPos = UnityObjectToClipPos(v.vertex);
	o.vertex = ClipSpaceOutline(clipPos, v.normal, _Outline * 0.01);
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

fixed4 frag(v2f i): SV_Target
{
	fixed4 col = _OutlineColor;
	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}