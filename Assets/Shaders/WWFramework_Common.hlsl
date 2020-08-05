

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