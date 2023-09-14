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
        initialPos = transform.position;

        // Initialize the RealSense camera when the script starts
        initCamera(640, 480, 640, 480);
        trackingThread = new Thread(ThreadUpdate);
        trackingThread.Start();
        resetEvent = new AutoResetEvent(false);

        if (featureExtractorType == FeatureExtractorType.SIFT)
            createSIFT(siftNFeatures, siftNOctaveLayers, siftContrastThreshold, siftEdgeThreshold, siftSigma, siftEnable_precise_upscale);
        else if (featureExtractorType == FeatureExtractorType.ORB)
            createORB(orbNFeatures, orbScaleFactor, orbNLevels, orbEdgeThreshold, orbFirstLevel, orbWTA_K, orbScoreType, orbPatchSize, orbFastThreshold);
    }

    private void Update()
    {
        resetEvent.Set();
        transform.position = initialPos + rotatedTranslationVector;

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
        trackingThread.Abort();
        isStopped = true;
    }

    private void OnDestroy()
    {
        CleanupCamera();
        isStopped = true;
        trackingThread.Abort();
    }
}
