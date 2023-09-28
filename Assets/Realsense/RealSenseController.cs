using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using System.IO;
using System;

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

    [DllImport(PLUGIN_NAME)]
    private static extern void colorStreamConfig(int width, int height, int fps);
    [DllImport(PLUGIN_NAME)]
    private static extern void depthStreamConfig(int width, int height, int fps);

    [DllImport(PLUGIN_NAME)]
    private static extern void bagFileStreamConfig(string bagFileAddress);

    [DllImport(PLUGIN_NAME)]
    private static extern void initCamera();

    [DllImport(PLUGIN_NAME)]
    private static extern void setParams(float newRatioTresh,
                                  float newMinDepth,
                                  float newMaxDepth,
                                  int newMin3DPoints,
                                  float newMaxDistanceF2F,
                                  int newMaxFeaturesSolver,
                                  float newClipLimit,
                                  int tilesGridSize,
                                  int newFilterTemplateWindowSize,
                                  float newFilterSearchWindowSize,
                                  int newFilterStrengH,
                                  float newGamma);

    [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    private static extern void createORB(int nfeatures = 500,
                                        float scaleFactor = 1.2f,
                                        int nlevels = 8,
                                        int edgeThreshold = 31,
                                        int firstLevel = 0,
                                        int WTA_K = 2,
                                        int scoreType = 0,
                                        int patchSize = 31,
                                        int fastThreshold = 20
                                    );

    [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    private static extern void createSIFT(int nfeatures = 0,
                                        int nOctaveLayers = 3,
                                        double contrastThreshold = 0.04,
                                        double edgeThreshold = 10,
                                        double sigma = 1.6,
                                        bool enable_precise_upscale = false
                                    );

    [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    // private static extern void createORB();

    // [DllImport(PLUGIN_NAME)] // Replace PLUGIN_NAME with the name of your native plugin
    private static extern void findFeatures();

    [DllImport(PLUGIN_NAME)] // Replace with your actual native plugin name
    private static extern void GetTranslationVector(float[] t_f_data);

    public static float[] RetrieveTranslationVector()
    {
        float[] t_f_data = new float[7];
        GetTranslationVector(t_f_data);
        return t_f_data;
    }

    private float elapsedTime = 0f;
    private int frameCount = 0;
    private float fpsUpdateInterval = 0.1f;

    public int colorWidth = 640;
    public int colorHeight = 480;
    public int colorFPS = 30;
    public int depthWidth = 640;
    public int depthHeight = 480;
    public int depthFPS = 30;
    public float ratioTresh = 0.7f;
    public float minDepth = 0.0f;
    public float maxDepth = 6.0f;
    public int min3DPoints = 15;
    public float maxDistanceF2F = 0.05f;
    public int maxFeaturesSolver = 400;
    public float clipLimit = 3.0f;
    public int tilesGridSize = 5;
    public int filterTemplateWindowSize = 5;
    public float filterSearchWindowSize = 10;
    public int filterStrengH = 7;
    public float gamma = 1;

    public bool useRecord = false;

    /// <summary>
    /// FRAN STARTS HERE
    /// </summary>

    public enum FeatureExtractorType
    {
        SIFT,
        ORB
    }

    public FeatureExtractorType featureExtractorType;

    //ORB Parameters
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbNFeatures = 500;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private float orbScaleFactor = 1.2f;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbNLevels = 8;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbEdgeThreshold = 31;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbFirstLevel = 0;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbWTA_K = 2;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbScoreType = 0;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbPatchSize = 31;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.ORB)][SerializeField] private int orbFastThreshold = 20;

    //SIFT Parameters
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.SIFT)][SerializeField] private int siftNFeatures = 0;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.SIFT)][SerializeField] private int siftNOctaveLayers = 3;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.SIFT)][SerializeField] private double siftContrastThreshold = 0.04;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.SIFT)][SerializeField] private double siftEdgeThreshold = 10;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.SIFT)][SerializeField] private double siftSigma = 1.6;
    [DrawIf(nameof(featureExtractorType), FeatureExtractorType.SIFT)][SerializeField] private bool siftEnable_precise_upscale = false;

    Vector3 rotatedTranslationVector, initialPos;
    bool isStopped = false;
    Thread trackingThread;
    AutoResetEvent resetEvent;

    private void Start()
    {
#if !UNITY_EDITOR
        Invoke("DelayedStart", 0.5f);
        initialPos = transform.localPosition;
        Debug.Log("---------------------------------- INICIO PROGRAMA --------------------------------");
        // Initialize the RealSense camera when the script starts
        if (useRecord){
            string bagFilePath = System.IO.Path.Combine("/sdcard/Documents/20230918_163323.bag");

            if (File.Exists(bagFilePath))
            {
                // The file exists, you can proceed with your operations on the file.
                Debug.Log("The file exists: " + bagFilePath);
                bagFileStreamConfig(bagFilePath);
            }
            else
            {
                // The file does not exist, handle the case where the file is missing.
                Debug.LogError("The file does not exist: " + bagFilePath);
            }
            
        } else {
            colorStreamConfig(colorWidth, colorHeight, colorFPS);
            depthStreamConfig(depthWidth, depthHeight, depthFPS);
        }
        
        initCamera();
        trackingThread = new Thread(ThreadUpdate);
        trackingThread.Start();
        resetEvent = new AutoResetEvent(false);

        if (featureExtractorType == FeatureExtractorType.SIFT)
            createSIFT(siftNFeatures, siftNOctaveLayers, siftContrastThreshold, siftEdgeThreshold, siftSigma, siftEnable_precise_upscale);
        else if (featureExtractorType == FeatureExtractorType.ORB)
            createORB(orbNFeatures, orbScaleFactor, orbNLevels, orbEdgeThreshold, orbFirstLevel, orbWTA_K, orbScoreType, orbPatchSize, orbFastThreshold);


        setParams(ratioTresh, minDepth, maxDepth, min3DPoints, maxDistanceF2F, maxFeaturesSolver, clipLimit, tilesGridSize, filterTemplateWindowSize,
        filterSearchWindowSize, filterStrengH, gamma);
        
#endif
    }

#if !UNITY_EDITOR

    private void Update()
    {
        resetEvent.Set();
        transform.localPosition = initialPos + rotatedTranslationVector;

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
            Vector3 remappedTranslationVector = new Vector3(-translationVector[0], translationVector[1], -translationVector[2]);
            rotatedTranslationVector = Quaternion.AngleAxis(45, Vector3.right) * remappedTranslationVector;
        }
    }

    private void OnApplicationQuit()
    {
        CleanupCamera();
        trackingThread.Join();
        isStopped = true;
    }

    private void OnDestroy()
    {
        CleanupCamera();
        isStopped = true;
        trackingThread.Join();
    }
#endif
}
