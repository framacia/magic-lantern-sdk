using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public static float vmag;

    const float fpsMeasurePeriod = 0.5f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    const string display = "{0} FPS";
    public TextMeshProUGUI m_GuiText;


    private void Start()
    {
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        //Is it really necessary setting this??? Find a good place to add it 
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 61;
    }


    private void Update()
    {
        // m_GuiText.text = Time.realtimeSinceStartupAsDouble.ToString("f5");
        m_GuiText.text = vmag.ToString("f5");
        // measure average frames per second
        // m_FpsAccumulator++;
        // if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        // {
        //     m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
        //     m_FpsAccumulator = 0;
        //     m_FpsNextPeriod += fpsMeasurePeriod;
        //     m_GuiText.text = string.Format(display, m_CurrentFps);

        // }
    }
}
