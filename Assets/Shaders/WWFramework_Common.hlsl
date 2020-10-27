

float3 RotateAroundYInDegrees (float3 vertex, float degrees)
{
    float alpha = degrees * UNITY_PI / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m = float2x2(cosa, -sina, sina, cosa);
    return float3(mul(m, vertex.xz), vertex.y).xzy;
}


float3x3 RotationMatrix(float3 vAxis, float fAngle)
{
    // compute sin/cos of fAngle
    float2 vSinCos;
    #ifdef OPENGL
        vSinCos.x = sin(fAngle);
        vSinCos.y = cos(fAngle);
    #else
        sincos(fAngle, vSinCos.x, vSinCos.y);
    #endif

    const float c = vSinCos.y;
    const float s = vSinCos.x;
    const float t = 1.0 - c;
    const float x = vAxis.x;
    const float y = vAxis.y;
    const float z = vAxis.z;

    return float3x3(t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
    t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
    t * x * z - s * y,  t * y * z + s * x,  t * z * z + c);
}