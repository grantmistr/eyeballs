#ifndef EYE_INSTANCE_PARAMETERS
#define EYE_INSTANCE_PARAMETERS

struct BufferData
{
    float4x4 transformMatrix;
    float4x4 invTransformMatrix;
    
    float4 rotationData;
    
    float3 rotationMatrixX;
    float3 rotationMatrixY;
    float3 rotationMatrixZ;
    
    float blink;
};

StructuredBuffer<BufferData> _dynamicBuffer;

void SampleEyeDataBufferRotationData_float(float instanceID, out float4 rotationData)
{
    rotationData = _dynamicBuffer[(uint) instanceID].rotationData;
}

void SampleEyeDataBufferBlink_float(float instanceID, out float blink)
{
    blink = _dynamicBuffer[(uint) instanceID].blink;
}

void SampleEyeDataBufferRotationMatrix_float(float instanceID, out float3x3 rotationMatrix)
{
    float3x3 m;
    m[0] = _dynamicBuffer[(uint) instanceID].rotationMatrixX;
    m[1] = _dynamicBuffer[(uint) instanceID].rotationMatrixY;
    m[2] = _dynamicBuffer[(uint) instanceID].rotationMatrixZ;
    
    rotationMatrix = m;
}

void InstanceDataSetup()
{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    
#define unity_ObjectToWorld unity_ObjectToWorld
#define unity_WorldToObject unity_WorldToObject
    
        unity_ObjectToWorld = _dynamicBuffer[unity_InstanceID].transformMatrix;
        unity_WorldToObject = _dynamicBuffer[unity_InstanceID].invTransformMatrix;

#endif
}

#endif // EYE_INSTANCE_PARAMETERS