using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingController : MonoBehaviour
{
    public Button vignetteToggle;
    public Volume volume;
    private Vignette thisVignette;


    public void BloomCtrl()
    {
        VolumeProfile profile = volume.sharedProfile;

        volume.profile.TryGet(out thisVignette);

        thisVignette.active = vignetteToggle;
    }
}