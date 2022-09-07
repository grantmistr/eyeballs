using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFlyCam : MonoBehaviour
{
    public float mouseSense = 1f;
    
    private float moveScale = 0.005f;
    private float x = 0f, y = 0f, z = 0f;
    private float scrollValue = 0f;
    private float mouseX = 0f, mouseY = 0f;
    private float xRotation, yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRotation = transform.rotation.eulerAngles.x;
        yRotation = transform.rotation.eulerAngles.y;
    } 

    void Update()
    {
        x = 0f;
        y = 0f;
        z = 0f;

        scrollValue = Input.mouseScrollDelta.y * 0.01f;
        moveScale += scrollValue;
        
        if (moveScale < 0.0f)
        {
            moveScale = 0.0f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            z += moveScale;
        }

        if (Input.GetKey(KeyCode.S))
        {
            z -= moveScale;
        }

        if (Input.GetKey(KeyCode.D))
        {
            x += moveScale;
        }

        if (Input.GetKey(KeyCode.A))
        {
            x -= moveScale;
        }

        if (Input.GetKey(KeyCode.E))
        {
            y += moveScale;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            y -= moveScale;
        }

        mouseX = Input.GetAxis("Mouse X") * mouseSense;
        mouseY = Input.GetAxis("Mouse Y") * mouseSense;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        transform.Translate(x, y, z);
    }
}
