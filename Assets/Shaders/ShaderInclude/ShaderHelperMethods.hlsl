#ifndef SHADER_HELPER_METHODS
#define SHADER_HELPER_METHODS

void RotationMatrixFromAxisAndAngle_float(float theta, float3 a, out float3x3 rotationMatrix)
{
    float s = sin(theta);
    float c = cos(theta);
    float t = 1 - c;
    
    float3x3 m = {
        c + a.x * a.x * t, a.x * a.y * t - a.z * s, a.x * a.z * t + a.y * s,
        a.y * a.x * t + a.z * s, c + a.y * a.y * t, a.y * a.z * t - a.x * s,
        a.z * a.x * t - a.y * s, a.z * a.y * t + a.x * s, c + a.z * a.z * t
    };
    
    rotationMatrix = m;
}

void FastArcCos_float(float x, out float theta)
{
    theta = -0.6981317 * x;
    theta *= x;
    theta -= 0.8726646;
    theta *= x;
    theta += 1.570796;
}

#endif // SHADER_HELPER_METHODS