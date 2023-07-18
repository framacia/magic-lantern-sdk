using AClockworkBerry;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminUIController : NetworkBehaviour
{
    Camera cam;
    PostProcessingController postProcessingController;
    [SerializeField] GameObject virtualDouble;

    private void Start()
    {
        cam = Camera.main;
        postProcessingController = FindObjectOfType<PostProcessingController>(true);
    }

    public void SetCanvasVisibility(bool b)
    {
        gameObject.GetComponentInChildren<Canvas>().enabled = b;
    }

    //Required because of the button/toggle difference
    public void ToggleScreenLogger()
    {
        ScreenLogger.Instance.ShowLog = !ScreenLogger.Instance.ShowLog;
        CmdOnScreenLoggerToggled(ScreenLogger.Instance.ShowLog);
    }

    [Command (requiresAuthority = false)]
    void CmdOnScreenLoggerToggled(bool state)
    {
        ScreenLogger.Instance.ShowLog = state;
    }

    public void TogglePhysicalCamera(bool b)
    {
        cam.usePhysicalProperties = b;
        CmdOnPhysicalCameraToggled(b);
    }

    [Command(requiresAuthority = false)]
    void CmdOnPhysicalCameraToggled(bool b)
    {
        cam.usePhysicalProperties = b;
        Debug.Log("Physical Camera: " + cam.usePhysicalProperties);
    }

    public void ChangeVerticalLensShift(float value)
    {
        //Round to 3 decimals and compare, if it's the same stop
        if ((float)Math.Round(value * 1000f)/1000f == (float)Math.Round(cam.lensShift.y * 1000f) / 1000f)
            return;

        cam.lensShift = new Vector2(cam.lensShift.x, value);
        CmdOnChangeVerticalLensShift(cam.lensShift);
    }

    [Command(requiresAuthority = false)]
    void CmdOnChangeVerticalLensShift(Vector2 shift)
    {
        cam.lensShift = shift;
        Debug.Log(string.Format("Vertical Lens Shift: {0}", cam.lensShift.ToString("F3")));
    }

    public void ToggleVignette(bool b)
    {
        postProcessingController.OnToggleVignette(b);
    }

    public void ChangeValueContrast(float value)
    {
        postProcessingController.OnChangeValueContrast(value);
    }

    public void ToggleVirtualDouble()
    {
        virtualDouble.SetActive(!virtualDouble.activeSelf);
    }

}
