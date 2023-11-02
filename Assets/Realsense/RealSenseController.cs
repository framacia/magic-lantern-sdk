using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using System.IO;
using System;

public class RealSenseController : MonoBehaviour
{
    private const string PLUGIN_NAME = "camera_motion";

    [DllImport(PLUGIN_NAME)]
    private static extern void cleanupCamera();

    [DllImport(PLUGIN_NAME)]
    private static extern void colorStreamConfig(int width, int height, int fps);
    [DllImport(PLUGIN_NAME)]
    private static extern void depthStreamConfig(int width, int height, int fps);

    [DllImport(PLUGIN_NAME)]
    private static extern void bagFileStreamConfig(string bagFileAddress);

    [DllImport(PLUGIN_NAME)]
    private static extern void initCamera();

    [DllImport(PLUGIN_NAME)]
    private static extern void initImu();

    [DllImport(PLUGIN_NAME)]
    private static extern void setParams(systemConfig config);

    [DllImport(PLUGIN_NAME)]
    private static extern void cropRectangleFeatures(int initX, int initY, int endX, int endY);

    [DllImport(PLUGIN_NAME)] 
    private static extern void createORB(int nfeatures,
                                        float scaleFactor,
                                        int nlevels,
                                        int edgeThreshold,
                                        int firstLevel,
                                        int WTA_K,
                                        int scoreType,
                                        int patchSize,
                                        int fastThreshold
                                    );

    [DllImport(PLUGIN_NAME)] 
    private static extern void createSIFT(int nfeatures = 0,
                                        int nOctaveLayers = 3,
                                        double contrastThreshold = 0.04,
                                        double edgeThreshold = 10,
                                        double sigma = 1.6,
                                        bool enable_precise_upscale = false
                                    );

    [DllImport(PLUGIN_NAME)] 
    private static extern void firstIteration();


    [DllImport(PLUGIN_NAME)] 
    private static extern void findFeatures();

    [DllImport(PLUGIN_NAME)] 
    private static extern void GetTranslationVector(float[] t_f_data);

    public static float[] RetrieveTranslationVector()
    {
        float[] t_f_data = new float[3];
        GetTranslationVector(t_f_data);
        return t_f_data;
    }

    [DllImport(PLUGIN_NAME)] 
    private static extern void GetCameraOrientation(float[] cameraAngle);

    public static float[] RetrieveCameraOrientation()
    {
        float[] cameraAngle = new float[3];
        GetCameraOrientation(cameraAngle);
        return cameraAngle;
    }

    [DllImport(PLUGIN_NAME)] 
    private static extern void resetOdom();

    [DllImport(PLUGIN_NAME)]
    private static extern IntPtr getJpegBuffer(out int bufferSize);

    public static byte[] GetJpegBuffer(out int bufferSize)
    {
        IntPtr bufferPtr = getJpegBuffer(out bufferSize);
        
        byte[] jpegBuffer = new byte[bufferSize];
        Marshal.Copy(bufferPtr, jpegBuffer, 0, bufferSize);
        
        Marshal.FreeCoTaskMem(bufferPtr);
        
        return jpegBuffer;
    }

    [DllImport(PLUGIN_NAME)]
    private static extern float GetDepthAtCenter();

    [DllImport(PLUGIN_NAME)]
    private static extern  void setProjectorZone(int sectionX, int sectionY, int sectionWidth, int sectionHeight);

  
    public struct systemConfig {
        public float ratioTresh;
        public float minDepth;
        public float maxDepth;
        public int min3DPoints;
        public float maxDistanceF2F;
        public int minFeaturesLoopClosure;
        public int framesUntilLoopClosure;
        public float noMovementThresh;
        public int framesNoMovement;
        public int maxGoodFeatures;
        public int minFeaturesFindObject;

        
    }



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
    public int minFeaturesLoopClosure = 200;
    public int framesUntilLoopClosure = 200;
    public float noMovementThresh = 0.0001f;
    public int framesNoMovement = 50;
    public int maxGoodFeatures = 500;
    public int minFeaturesFindObject = 30;
    public int xRectangle = 180;
    public int yRectangle = 65;
    public int widthRectangle = 325;
    public int heightRectangle = 200;
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
    float angleX;
    bool reset_odom = false;

    private void Start()
    {
// #if !UNITY_EDITOR
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
        initImu();

        if (featureExtractorType == FeatureExtractorType.SIFT)
            createSIFT(siftNFeatures, siftNOctaveLayers, siftContrastThreshold, siftEdgeThreshold, siftSigma, siftEnable_precise_upscale);
        else if (featureExtractorType == FeatureExtractorType.ORB)
            createORB(orbNFeatures, orbScaleFactor, orbNLevels, orbEdgeThreshold, orbFirstLevel, orbWTA_K, orbScoreType, orbPatchSize, orbFastThreshold);

        systemConfig config = new systemConfig();
        config.ratioTresh = ratioTresh;
        config.minDepth = minDepth;
        config.maxDepth = maxDepth;
        config.min3DPoints = min3DPoints;
        config.maxDistanceF2F = maxDistanceF2F;
        config.minFeaturesLoopClosure = minFeaturesLoopClosure;
        config.minFeaturesLoopClosure = minFeaturesLoopClosure;
        config.noMovementThresh = noMovementThresh;
        config.framesNoMovement = framesNoMovement;
        config.maxGoodFeatures = maxGoodFeatures;
        config.minFeaturesFindObject = minFeaturesFindObject;
        
        
        setParams(config);
        
        setProjectorZone(xRectangle, yRectangle, widthRectangle, heightRectangle);

        firstIteration();

        
        // float[] totalCameraAngle = new float[3] { 0.0f, 0.0f, 0.0f };
        // int numberOfSamples = 1000;

        // for (int i = 0; i < numberOfSamples; i++)
        // {
        //     float[] cameraAngle = RetrieveCameraOrientation();
        //     totalCameraAngle[0] += cameraAngle[0];
        //     totalCameraAngle[1] += cameraAngle[1];
        //     totalCameraAngle[2] += cameraAngle[2];
        // }

        // float averageX = totalCameraAngle[0] / numberOfSamples;
        // float averageY = totalCameraAngle[1] / numberOfSamples;
        // float averageZ = totalCameraAngle[2] / numberOfSamples;

        
        // Debug.Log("Average Camera Orientation x: " + averageX);
        // Debug.Log("Average Camera Orientation y: " + averageY);
        // Debug.Log("Average Camera Orientation z: " + averageZ);

        // angleX = averageZ;


        trackingThread = new Thread(ThreadUpdate);
        trackingThread.Start();
        resetEvent = new AutoResetEvent(false);


      
        
        
// #endif
    }

// #if !UNITY_EDITOR

    private void Update()
    {
        resetEvent.Set();
        transform.localPosition = initialPos + rotatedTranslationVector;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Reseting Odometry...");
            reset_odom = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
             Application.Quit();
        }
       
        

    }

    private void ThreadUpdate()
    {
        while (!isStopped)
        {
            resetEvent.WaitOne(); 

            findFeatures();
            //float depth = GetDepthAtCenter();
            float[] translationVector = RetrieveTranslationVector();
            Vector3 remappedTranslationVector = new Vector3(-translationVector[0], translationVector[1], -translationVector[2]);
            rotatedTranslationVector = Quaternion.AngleAxis(0, Vector3.right) * remappedTranslationVector;

            if (reset_odom == true)
            {
                resetOdom();
                reset_odom = false;
            }
            
        }
        
    }

    private void OnDestroy()
    {
        
        isStopped = true;
        resetEvent.Set(); 
        trackingThread.Join(); 
        cleanupCamera();
    }
// #endif
}
