using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UpdateTargetPosition : MonoBehaviour
{
    [SerializeField] private Material eyelidMaterial, eyeballMaterial;
    private readonly int targetPositionID = Shader.PropertyToID("_targetPosition");

    private void Update()
    {
        eyelidMaterial.SetVector(targetPositionID, this.transform.position);
        eyeballMaterial.SetVector(targetPositionID, this.transform.position);
    }
}
