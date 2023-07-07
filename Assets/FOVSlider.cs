using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;

public class FOVSlider : MonoBehaviour
{
    public TextMeshProUGUI fovText;

    private void Start()
    {
        fovText.text = Camera.main.fieldOfView.ToString();
    }

    public void ChangeFOV(float fov)
    {
        Camera.main.fieldOfView = fov;
        fovText.text = fov.ToString();
    }
}
