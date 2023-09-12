using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class RealSenseController : MonoBehaviour
{
    private const string PLUGIN_NAME = "camera_motion";
    // Import the C++ functions from the native plugin
    [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    private static extern void InitializeCamera();

    [DllImport(PLUGIN_NAME)]
    private static extern float GetDepthAtCenter();

    [DllImport(PLUGIN_NAME)]
    private static extern void CleanupCamera();


    [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    private static extern void initCamera(int color_width, int color_height, int depth_width, int depth_height);

    [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    private static extern void findFeatures();

    [DllImport(PLUGIN_NAME)] // Replace with your actual native plugin name
    private static extern void GetTranslationVector(float[] t_f_data);

    Vector3 trackedPos, initialPos;
    bool isStopped = false;
    Thread trackingThread;
    AutoResetEvent resetEvent;

    public static float[] RetrieveTranslationVector()
    {
        float[] t_f_data = new float[3];
        GetTranslationVector(t_f_data);
        return t_f_data;
    }

    private void Start()
    {
        initialPos = transform.position;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Initialize the RealSense camera when the script starts
        initCamera(424, 240, 480, 270);
        trackingThread = new Thread(ThreadUpdate);
        trackingThread.Start();
        resetEvent = new AutoResetEvent(false);
#endif
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        resetEvent.Set();
        transform.position = initialPos + trackedPos;
#endif
    }

    private void ThreadUpdate()
    {
        while (!isStopped)
        {
            resetEvent.WaitOne(); //Why this??
            // Get and use the depth value at the center of the image
            findFeatures();
            //float depth = GetDepthAtCenter();
            float[] translationVector = RetrieveTranslationVector();
            trackedPos = new Vector3(-translationVector[0], translationVector[1], -translationVector[2]);
        }
    }

    private void OnApplicationQuit()
    {
        trackingThread.Abort();
        isStopped = true;
    }

    private void onDestroy()
    {
        CleanupCamera();
        isStopped = true;
        trackingThread.Abort();
    }
}
