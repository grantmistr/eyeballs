using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeManager : MonoBehaviour
{
    public static List<Eye> eyeList = new List<Eye>();

    [SerializeField] private ComputeShader computeShader;
    public Transform targetPosition;
    
    [SerializeField] private Mesh eyelidMesh, eyeballMesh;
    [SerializeField] private Material eyelidMaterial, eyeballMaterial;

    [Header("Material Parameters")]
    [Header("Eyelid Parameters")]
    [SerializeField] private float blinkRandomness = 500f;
    [SerializeField] private float blinkSpeed = 15f, blinkFrequency = 100f;
    [Range(0f, 3.14f)]
    [SerializeField] private float eyeRotationLimit = 1.57f;
    [Range(0f, 1f)]
    [SerializeField] private float eyelidFollowStrength = 0.5f;
    [SerializeField] private Texture2D eyelidNormalMap;
    [SerializeField] private Color eyelidColor = Color.gray;

    [Header("Eyeball Parameters")]
    [Range(-0.99f, -0.5f)]
    [SerializeField] private float pupilSize = -0.8f;
    [SerializeField] private float pupilEdgeHardness = 50f;

    private ComputeBuffer staticBuffer, dynamicBuffer, blinkOffsetBuffer, eyelidArgsBuffer, eyeballArgsBuffer;
    private int bufferSize, threadGroupsX;
    private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

    #region
    private readonly int
        staticBufferID = Shader.PropertyToID("_staticBuffer"),
        dynamicBufferID = Shader.PropertyToID("_dynamicBuffer"),
        blinkOffsetBufferID = Shader.PropertyToID("_blinkOffsetBuffer"),
        targetPositionID = Shader.PropertyToID("_targetPosition"),
        eyeRotationLimitID = Shader.PropertyToID("_eyeRotationLimit"),
        blinkTimeID = Shader.PropertyToID("_blinkTime"),
        blinkFrequencyID = Shader.PropertyToID("_blinkFrequency"),
        timeID = Shader.PropertyToID("_Time"),
        cameraVPID = Shader.PropertyToID("_cameraVP"),
        farClipPlaneID = Shader.PropertyToID("_farClipPlane"),
        cameraPositionID = Shader.PropertyToID("_cameraPosition");
    #endregion

    private void OnValidate()
    {
        eyelidMaterial.SetFloat("_eyelidFollowStrength", eyelidFollowStrength);
        eyelidMaterial.SetTexture("_eyelidNormalMap", eyelidNormalMap);
        eyelidMaterial.SetColor("_eyelidColor", eyelidColor);

        eyeballMaterial.SetFloat("_pupilSize", pupilSize);
        eyeballMaterial.SetFloat("_pupilEdgeHardness", pupilEdgeHardness);
    }

    private void Start()
    {
        InitializeBuffers();
        SetShaderParameters();
    }

    private void Update()
    {
        UpdateShaderParameters();
        ExecuteComputeShader();
        DrawMeshes();
    }

    private struct BufferData
    {
        public Matrix4x4 transformMatrix;
        public Matrix4x4 invTransformMatrix;

        public Vector4 rotationData; // xyz: axis, w: theta

        public Vector3 rotationDataX;
        public Vector3 rotationDataY;
        public Vector3 rotationDataZ;

        public float blink;
    }

    private void InitializeBuffers()
    {
        bufferSize = eyeList.Count;
        threadGroupsX = Mathf.CeilToInt(bufferSize / 32f);

        staticBuffer = new ComputeBuffer(bufferSize, sizeof(float) * 46);
        dynamicBuffer = new ComputeBuffer(bufferSize, sizeof(float) * 46, ComputeBufferType.Append);
        dynamicBuffer.SetCounterValue(0);
        blinkOffsetBuffer = new ComputeBuffer(bufferSize, sizeof(float));

        eyelidArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
        eyeballArgsBuffer = new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);

        BufferData[] bufferData = new BufferData[bufferSize];
        float[] blinkOffsetData = new float[bufferSize];

        for (int i = 0; i < bufferSize; i++)
        {
            bufferData[i].transformMatrix = eyeList[i].transform.localToWorldMatrix;
            bufferData[i].invTransformMatrix = eyeList[i].transform.worldToLocalMatrix;

            bufferData[i].rotationData = new Vector4(1f, 0f, 0f, 0f);
            
            bufferData[i].rotationDataX = Vector3.zero;
            bufferData[i].rotationDataY = Vector3.zero;
            bufferData[i].rotationDataZ = Vector3.zero;

            blinkOffsetData[i] = Mathf.Floor(Random.value * blinkRandomness);

            UpdateBounds(eyeList[i].transform.localScale, eyeList[i].transform.position);
        }

        uint[] args = new uint[] { 0, 0, 0, 0, 0 };
        args[0] = eyelidMesh.GetIndexCount(0);
        args[1] = (uint)bufferSize;

        eyelidArgsBuffer.SetData(args);

        args[0] = eyeballMesh.GetIndexCount(0);

        eyeballArgsBuffer.SetData(args);

        staticBuffer.SetData(bufferData);
        blinkOffsetBuffer.SetData(blinkOffsetData);
    }

    private void UpdateBounds(Vector3 objectScale, Vector3 objectPosition)
    {
        Bounds objectBounds = eyelidMesh.bounds;
        objectBounds.size.Set(objectBounds.size.x * objectScale.x, objectBounds.size.y * objectScale.y, objectBounds.size.z * objectScale.z);
        objectBounds.center = objectPosition;

        bounds.Encapsulate(objectBounds);
    }

    private void SetShaderParameters()
    {
        computeShader.SetBuffer(0, staticBufferID, staticBuffer);
        computeShader.SetBuffer(0, dynamicBufferID, dynamicBuffer);
        computeShader.SetBuffer(0, blinkOffsetBufferID, blinkOffsetBuffer);
        computeShader.SetFloat(eyeRotationLimitID, eyeRotationLimit);
        computeShader.SetFloat(blinkFrequencyID, blinkFrequency);
        computeShader.SetFloat(farClipPlaneID, Camera.main.farClipPlane);

        eyelidMaterial.SetBuffer(dynamicBufferID, dynamicBuffer);

        eyeballMaterial.SetBuffer(dynamicBufferID, dynamicBuffer);
    }

    private void UpdateShaderParameters()
    {
        computeShader.SetVector(targetPositionID, targetPosition.position);
        computeShader.SetFloat(blinkTimeID, blinkSpeed * Shader.GetGlobalVector(timeID).y);
        computeShader.SetMatrix(cameraVPID, Camera.main.projectionMatrix * Camera.main.transform.worldToLocalMatrix);
        computeShader.SetVector(cameraPositionID, Camera.main.transform.position);
    }

    private void ExecuteComputeShader()
    {
        computeShader.Dispatch(0, threadGroupsX, 1, 1);
    }

    private void DrawMeshes()
    {
        ComputeBuffer.CopyCount(dynamicBuffer, eyelidArgsBuffer, sizeof(uint));
        ComputeBuffer.CopyCount(dynamicBuffer, eyeballArgsBuffer, sizeof(uint));

        dynamicBuffer.SetCounterValue(0);

        Graphics.DrawMeshInstancedIndirect(eyelidMesh, 0, eyelidMaterial, bounds, eyelidArgsBuffer, 0, null, UnityEngine.Rendering.ShadowCastingMode.Off);
        Graphics.DrawMeshInstancedIndirect(eyeballMesh, 0, eyeballMaterial, bounds, eyeballArgsBuffer);
    }

    private void OnDisable()
    {
        if (staticBuffer != null)
        {
            staticBuffer.Release();
            staticBuffer = null;
        }
        if (dynamicBuffer != null)
        {
            dynamicBuffer.Release();
            dynamicBuffer = null;
        }
        if (blinkOffsetBuffer != null)
        {
            blinkOffsetBuffer.Release();
            blinkOffsetBuffer = null;
        }
        if (eyelidArgsBuffer != null)
        {
            eyelidArgsBuffer.Release();
            eyelidArgsBuffer = null;
        }
        if (eyeballArgsBuffer != null)
        {
            eyeballArgsBuffer.Release();
            eyeballArgsBuffer = null;
        }
    }
}
