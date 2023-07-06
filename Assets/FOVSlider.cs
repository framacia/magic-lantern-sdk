using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FOVSlider : MonoBehaviour
{
    public TextMeshProUGUI fovText;

    public void ChangeFOV(float fov)
    {
        Camera.main.fieldOfView = fov;
        fovText.text = fov.ToString();
    }
}
