using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eye : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(this.transform.position, this.transform.localScale.x);
        Gizmos.DrawLine(this.transform.position, this.transform.forward * 2 + this.transform.position);
    }

    private void OnEnable()
    {
        EyeManager.eyeList.Add(this);
    }

    private void OnDisable()
    {
        EyeManager.eyeList.Remove(this);
    }
}
