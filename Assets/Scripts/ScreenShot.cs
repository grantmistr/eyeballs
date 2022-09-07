using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{

    [SerializeField] private KeyCode keyCode = KeyCode.K;

    public static string ScreenShotName()
    {
        return string.Format("C:/Users/Grant Mistr/Pictures/ss_{0}.png",
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(keyCode))
        {
            ScreenCapture.CaptureScreenshot(ScreenShotName());
        }
    }
}
