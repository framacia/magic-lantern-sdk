using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using Unity.VisualScripting;

public class PostProcessingController : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private DepthOfField depthOfField;

    public static Action<PostProcessingConfig, GameObject> OnPostProcessingChanged;

    private void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out depthOfField);
    }

    public void OnToggleVignette(bool toggle)
    {
        vignette.active = toggle;
        OnPostProcessingChanged?.Invoke(GetPostProcessingConfig(), this.gameObject);
    }

    public void OnChangeValueContrast(float value)
    {
        colorAdjustments.contrast.value = value * 100;
        OnPostProcessingChanged?.Invoke(GetPostProcessingConfig(), this.gameObject);
    }

    public void ChangeDoFFocalLength(float value)
    {
        if(depthOfField != null)
            depthOfField.focalLength.value = value;
        //TODO No need to send this through network or invoke event for now
    }

    public void SetPostProcessingConfig(PostProcessingConfig config)
    {
        volume.enabled = config.postProcessingOn;
        vignette.active = config.vignetteOn;
        colorAdjustments.contrast.value = config.contrastAmount;
    }

    public PostProcessingConfig GetPostProcessingConfig()
    {
        PostProcessingConfig config = new PostProcessingConfig();
        config.postProcessingOn = volume.enabled;
        config.vignetteOn = vignette.active;
        config.contrastAmount = colorAdjustments.contrast.value;
        return config;
    }
}